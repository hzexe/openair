using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
#if SILVERLIGHT
using System.Windows.Browser;
#endif

namespace OpenRiaServices.DomainServices.Client
{
    internal class WebDomainClientWebHttpBehavior : WebHttpBehavior
    {
        public WebDomainClientWebHttpBehavior()
            : base()
        {
        }

        protected override void AddClientErrorInspector(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageVersionNoneFaultsEnabled = true;
            clientRuntime.MaxFaultSize = int.MaxValue;
        }

        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new WebHttpQueryStringConverter();
        }

        protected override IClientMessageFormatter GetRequestClientFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            IClientMessageFormatter formatter = base.GetRequestClientFormatter(operationDescription, endpoint);

            // The wrapping formatter is meant format just query requests. We cannot tell the
            // difference at build time, just at run time.
            return new WebHttpQueryClientMessageFormatter(formatter);
        }

        /// <summary>
        /// Apply contract behaviour such as [WebGet] and [FaultDescription] and then perform validation.
        /// 
        /// Confirms that the endpoint meets the requirements for the Web programming model.
        /// </summary>
        /// <param name="endpoint">The service endpoint.</param>
        public override void Validate(ServiceEndpoint endpoint)
        {
            foreach (var od in endpoint.Contract.Operations)
            {
                // Add FaultDescription if [FaultContractAttribute] has not already been added (by old code-generation)
                if (!od.Faults.Any(f => f.DetailType == typeof(DomainServiceFault)))
                {
                    od.Faults.Add(new FaultDescription(od.Messages[0].Action + "DomainServiceFault")
                    {
                        DetailType = typeof(DomainServiceFault),
                        Name = typeof(DomainServiceFault).Name,
                        Namespace = "DomainServices",
                    });
                }

                // If [WebGet] or [WebInvoke] has already been applied (by old code-generation) then no further action is required
                if (od.Behaviors.Contains(typeof(WebGetAttribute)) || od.Behaviors.Contains(typeof(WebInvokeAttribute)))
                    continue;

                if (od.BeginMethod != null)
                {
                    // Add [WebGet] to methods which don't have any side effects ([WebInvoke] is default action)
                    var hasSideEffectsAttribute = (HasSideEffectsAttribute)Attribute.GetCustomAttribute(od.BeginMethod, typeof(HasSideEffectsAttribute), inherit: false);
                    if (hasSideEffectsAttribute != null && hasSideEffectsAttribute.HasSideEffects == false)
                        EnsureBehavior<WebGetAttribute>(od);
                }
            }

            base.Validate(endpoint);
        }

        /// <summary>
        /// Get the behaviour of type T applied to the <see cref="OperationDescription"/>, adding it if it
        /// is not already present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operationDesc">The operation desc.</param>
        /// <returns></returns>
        protected static T EnsureBehavior<T>(OperationDescription operationDesc) where T : IOperationBehavior, new()
        {
            T behavior = operationDesc.Behaviors.Find<T>();
            if (behavior == null)
            {
                behavior = new T();
                operationDesc.Behaviors.Insert(0, behavior);
            }
            return behavior;
        }
    }

    /// <summary>
    /// A formatter for serializing query requests which requires query parameters present in the
    /// To uri or message body.
    /// </summary>
    internal class WebHttpQueryClientMessageFormatter : IClientMessageFormatter
    {
        /// <summary>
        /// For cross-browser compatibility support, the maximum supported URI length is 2083 characters.
        /// </summary>
        internal const int MaximumUriLength = 2083;
        private IClientMessageFormatter _innerFormatter;

        public WebHttpQueryClientMessageFormatter(IClientMessageFormatter innerFormatter)
        {
            Debug.Assert(innerFormatter != null, "innerFormatter cannot be null.");
            this._innerFormatter = innerFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            // The formatter is used for request serialization only.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the specified <paramref name="parameters"/> into an outbound
        /// <see cref="Message"/>. For query requests with query properties, stores the query
        /// parameters either in the To/Via or the message body.
        /// </summary>
        /// <param name="messageVersion">The version of the message to use.</param>
        /// <param name="parameters">The parameters passed to the client operation.</param>
        /// <returns>The message to send.</returns>
        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            Message request = this._innerFormatter.SerializeRequest(messageVersion, parameters);

            object queryProperty = null;
            object includeTotalCountProperty = null;
            if (OperationContext.Current != null)
            {
                OperationContext.Current.OutgoingMessageProperties.TryGetValue(WebDomainClient<object>.QueryPropertyName, out queryProperty);
                OperationContext.Current.OutgoingMessageProperties.TryGetValue(WebDomainClient<object>.IncludeTotalCountPropertyName, out includeTotalCountProperty);
            }

            List<KeyValuePair<string, string>> queryOptions = new List<KeyValuePair<string, string>>();
            if (queryProperty != null)
            {
                foreach (ServiceQueryPart queryPart in QuerySerializer.Serialize((IQueryable)queryProperty))
                {
                    queryOptions.Add(
                        new KeyValuePair<string, string>(
                            queryPart.QueryOperator,
                            queryPart.Expression));
                }
            }

            if (includeTotalCountProperty != null)
            {
                queryOptions.Add(
                    new KeyValuePair<string, string>("includeTotalCount", includeTotalCountProperty.ToString()));
            }

            if (queryOptions.Count > 0)
            {
                Debug.Assert(OperationContext.Current != null, "OpeartionContext.Current cannot be null at this point.");
                if (MessageUtility.IsHttpPOSTMethod(OperationContext.Current.OutgoingMessageProperties))
                {
                    MessageUtility.AddMessageQueryOptions(ref request, queryOptions);
                }
                else
                {
                    MessageUtility.AddQueryToUrl(ref request, queryOptions);
                }
            }

            if (request.Headers.To.AbsoluteUri.Length > WebHttpQueryClientMessageFormatter.MaximumUriLength)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resource.WebDomainClient_MaximumUriLengthExceeded,
                        WebHttpQueryClientMessageFormatter.MaximumUriLength));
            }

            return request;
        }
    }
}