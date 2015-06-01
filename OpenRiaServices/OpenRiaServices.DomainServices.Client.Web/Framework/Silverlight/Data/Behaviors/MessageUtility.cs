using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
#if SILVERLIGHT
using System.Windows.Browser;
#else
using System.Web;
#endif
using System.Xml;

#if SERVERFX
namespace OpenRiaServices.DomainServices.Hosting
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    /// <summary>
    /// Utility class containing utility methods to read and/or modify messages.
    /// It is mainly used to insert(read) query options to(from) the URL or message body of the outgoing(incoming) messages.
    /// </summary>
    internal static class MessageUtility
    {
        private const string HttpRequestName = "httpRequest";
        private const string HttpPostMethodName = "POST";
        private const string MessageRootElementName = "MessageRoot";
        private const string QueryOptionsListElementName = "QueryOptions";
        private const string QueryOptionElementName = "QueryOption";
        private const string QueryNameAttribute = "Name";
        private const string QueryValueAttribute = "Value";
        private const string QueryIncludeTotalCountOption = "includeTotalCount";

        /// <summary>
        /// Checks if the HTTP method used is POST.
        /// </summary>
        /// <param name="properties">Properties for which the HTTP method is to be checked.</param>
        /// <returns><c>true</c> if the <paramref name="properties"/> specifies HTTP POST method.</returns>
        public static bool IsHttpPOSTMethod(MessageProperties properties)
        {
            object property;
            if (properties.TryGetValue(MessageUtility.HttpRequestName, out property))
            {
                HttpRequestMessageProperty httpMessageProperty = property as HttpRequestMessageProperty;
                if (httpMessageProperty != null && httpMessageProperty.Method.Equals(MessageUtility.HttpPostMethodName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

#if !SERVERFX
        /// <summary>
        /// Changes a HTTP GET into a POST.
        /// </summary>
        /// <param name="properties">Properties for which the HTTP method is to be changed.</param>
        public static void MakeHttpPOSTMethod(MessageProperties properties)
        {
            object property;
            HttpRequestMessageProperty httpMessageProperty = null;

            if (properties.TryGetValue(MessageUtility.HttpRequestName, out property))
            {
                httpMessageProperty = property as HttpRequestMessageProperty;
            }

            if (httpMessageProperty == null)
            {
                httpMessageProperty = new HttpRequestMessageProperty();
                properties.Add(MessageUtility.HttpRequestName, httpMessageProperty);
            }

            httpMessageProperty.Method = MessageUtility.HttpPostMethodName;
            httpMessageProperty.SuppressEntityBody = false;
        }

        /// <summary>
        /// Creates a new message with the QueryOptions and original message body embedded in it.
        /// </summary>
        /// <param name="message">The original message.</param>
        /// <param name="queryOptions">The query options to embed in the new message.</param>
        internal static void AddMessageQueryOptions(ref Message message, IList<KeyValuePair<string, string>> queryOptions)
        {
            BodyWriter bodyWriter = new QueryOptionsBodyWriter(message, queryOptions);
            // Note, we must use a null action, otherwise 2 actions will exist in the headers.
            Message queryOptionsMessage = Message.CreateMessage(message.Version, /*action*/ null, bodyWriter);
            queryOptionsMessage.Headers.CopyHeadersFrom(message);
            queryOptionsMessage.Properties.CopyProperties(message.Properties);
            message = queryOptionsMessage;
        }

        /// <summary>
        /// Adds the query parameters to the URL of the message.
        /// </summary>
        /// <param name="request">The original message.</param>
        /// <param name="queryOptions">The query parameters to be added to the message URL.</param>
        internal static void AddQueryToUrl(ref Message request, List<KeyValuePair<string, string>> queryOptions)
        {
            Uri originalTo = request.Headers.To;
            StringBuilder queryBuilder = new StringBuilder(originalTo.Query);

            if (queryBuilder.Length > 0 && queryBuilder[0] == '?')
            {
                queryBuilder.Remove(0, 1);
            }

            bool prepend = (string.IsNullOrEmpty(originalTo.Query)) ? false : true;
            foreach (var queryStringParameter in queryOptions)
            {
                if (prepend)
                {
                    queryBuilder.Append('&');
                }
                prepend = true;
                queryBuilder.AppendFormat(CultureInfo.InvariantCulture, "${0}={1}", queryStringParameter.Key, HttpUtility.UrlEncode(HttpUtility.UrlEncode(queryStringParameter.Value)));
            }

            UriBuilder builder = new UriBuilder(originalTo);
            builder.Query = queryBuilder.ToString();
            var uri = builder.Uri;

            if (uri.AbsoluteUri.Length <= WebHttpQueryClientMessageFormatter.MaximumUriLength)
            {
                request.Headers.To = request.Properties.Via = builder.Uri;
            }
            else // fallback to POST if the Uri would become to long
            {
                MakeHttpPOSTMethod(System.ServiceModel.OperationContext.Current.OutgoingMessageProperties);
                AddMessageQueryOptions(ref request, queryOptions);
            }
        }


#else //SERVERFX
        /// <summary>
        /// Extracts the service query and original message body from <paramref name="message"/> .
        /// May create a new message with the original body contents.
        /// </summary>
        /// <param name="message">The message to use for deserialization.</param>
        /// <returns>The extracted service query.</returns>
        /// <remarks>The message passed in will not be disposed. This message belongs to the
        /// caller.</remarks>
        internal static ServiceQuery GetServiceQuery(ref Message message)
        {
            ServiceQuery serviceQuery = null;
            if (!message.IsEmpty)
            {
                XmlDictionaryReader reader = message.GetReaderAtBodyContents();
                // If the message root is not <MessageRoot> then the message does not contain QueryOptions.
                // So re-write the stream back to a message.
                if (reader.IsStartElement(MessageUtility.MessageRootElementName))
                {
                    // Go to the <QueryOptions> node.
                    reader.Read();                                                              // <MessageRoot>
                    reader.ReadStartElement(MessageUtility.QueryOptionsListElementName);        // <QueryOptions>
                    serviceQuery = MessageUtility.ReadServiceQuery(reader);                     // <QueryOption></QueryOption>
                    // Go to the starting node of the original message.
                    reader.ReadEndElement();                                                    // </QueryOptions>
                    reader = XmlDictionaryReader.CreateDictionaryReader(reader.ReadSubtree());  // Remainder of the message
                }

                // Note: we must use this overload to get a streamed message. This incurs the least
                // cost on GetReaderAtBodyContents which is the most expensive call in this path.
                // Note: We do not care about the new message enforcing the max size of headers
                // since it has already been enforced with the old message.
                Message newMessage = Message.CreateMessage(reader, Int32.MaxValue, message.Version);
                newMessage.Headers.CopyHeadersFrom(message);
                newMessage.Properties.CopyProperties(message.Properties);
                message = newMessage;
            }
            return serviceQuery;
        }

        /// <summary>
        /// Reads the query options from the given reader and returns the resulting service query.
        /// It assumes that the reader is positioned on a stream containing the query options.
        /// </summary>
        /// <param name="reader">Reader to the stream containing the query options.</param>
        /// <returns>Extracted service query.</returns>
        internal static ServiceQuery ReadServiceQuery(XmlReader reader)
        {
            List<ServiceQueryPart> serviceQueryParts = new List<ServiceQueryPart>();
            bool includeTotalCount = false;
            while (reader.IsStartElement(MessageUtility.QueryOptionElementName))
            {
                string name = reader.GetAttribute(MessageUtility.QueryNameAttribute);
                string value = reader.GetAttribute(MessageUtility.QueryValueAttribute);
                if (name.Equals(MessageUtility.QueryIncludeTotalCountOption, StringComparison.OrdinalIgnoreCase))
                {
                    bool queryOptionValue = false;
                    if (Boolean.TryParse(value, out queryOptionValue))
                    {
                        includeTotalCount = queryOptionValue;
                    }
                }
                else
                {
                    serviceQueryParts.Add(new ServiceQueryPart { QueryOperator = name, Expression = value });
                }

                MessageUtility.ReadElement(reader);
            }

            ServiceQuery serviceQuery = new ServiceQuery()
            {
                QueryParts = serviceQueryParts,
                IncludeTotalCount = includeTotalCount
            };
            return serviceQuery;
        }

        private static void ReadElement(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.Read();
                reader.ReadEndElement();
            }
        }
#endif

#if !SERVERFX
        /// <summary>
        /// BodyWriter that writes a message with query options in the message body.
        /// </summary>
        /// <remarks>Reading and writing messages are expensive. Without a wrapping message or body
        /// writer of some kind, we would require reading the original message, and rewriting both the
        /// options and original message. With a wrapping body writer, we can reduce the write original
        /// message/read original message/write altered message, to just one write.</remarks>
        private class QueryOptionsBodyWriter : BodyWriter
        {
            private Message _originalMessage;
            private IList<KeyValuePair<string, string>> _queryOptions;

            internal QueryOptionsBodyWriter(Message originalMessage, IList<KeyValuePair<string, string>> queryOptions)
                : base(/*isBuffered*/ false)
            {
                this._originalMessage = originalMessage;
                this._queryOptions = queryOptions;
            }

            /// <summary>
            /// Writes the message with the query options first and the original message last. This is
            /// done inline.
            /// </summary>
            /// <param name="writer">The writer used to write out the message body.</param>
            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                // We add the QueryOptions before the body of the message. So the new message looks
                // like:
                // <MessageRoot>
                //  <QueryOptions>
                //    <QueryOption Name="..." Value="..." />
                //    ...
                //  </QueryOptions>
                //  - original message body -
                // </MessageRoot>

                try
                {
                    writer.WriteStartElement(MessageUtility.MessageRootElementName);        // <MessageRoot>
                    writer.WriteStartElement(MessageUtility.QueryOptionsListElementName);   // <QueryOptions>
                    foreach (var queryOption in this._queryOptions)                         // for each query option write <QueryOption Name="..." Value="..." />
                    {
                        writer.WriteStartElement(MessageUtility.QueryOptionElementName);
                        writer.WriteAttributeString(MessageUtility.QueryNameAttribute, queryOption.Key);
                        writer.WriteAttributeString(MessageUtility.QueryValueAttribute, queryOption.Value);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();                                               // </QueryOptions>
                    this._originalMessage.WriteBodyContents(writer);                        // write original message body
                    writer.WriteEndElement();                                               // </MessageRoot>
                }
                finally
                {
                    this._originalMessage.Close();
                    this._originalMessage = null;
                    this._queryOptions = null;
                }
            }
        }
#endif
    }
}
