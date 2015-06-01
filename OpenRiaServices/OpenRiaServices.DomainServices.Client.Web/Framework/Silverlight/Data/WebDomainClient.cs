using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using OpenRiaServices;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

#if SILVERLIGHT
using System.Windows;
#endif

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Default <see cref="DomainClient"/> implementation using WCF
    /// </summary>
    /// <typeparam name="TContract">The contract type.</typeparam>
    public sealed class WebDomainClient<TContract> : DomainClient where TContract : class
    {
        internal const string QueryPropertyName = "DomainServiceQuery";
        internal const string IncludeTotalCountPropertyName = "DomainServiceIncludeTotalCount";

        private const int MaxReceivedMessageSize = int.MaxValue;
#if !SILVERLIGHT
        // These are only settable on the desktop. In Silverlight the max is already used by default.
        private const int MaxArrayLength = int.MaxValue;
        private const int MaxBytesPerRead = int.MaxValue;
        private const int MaxDepth = int.MaxValue;
        private const int MaxNameTableCharCount = int.MaxValue;
        private const int MaxStringContentLength = int.MaxValue;
#endif

        private ChannelFactory<TContract> _channelFactory;
        private readonly bool _usesHttps;
        private IEnumerable<Type> _knownTypes;
        private Uri _serviceUri;
        private bool _initializedFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDomainClient&lt;TContract&gt;"/> class.
        /// </summary>
        /// <param name="serviceUri">The domain service Uri</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="serviceUri"/>
        /// is null.
        /// </exception>
        public WebDomainClient(Uri serviceUri)
            : this(serviceUri, /* usesHttps */ false, /* channelFactory */ null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDomainClient&lt;TContract&gt;"/> class.
        /// </summary>
        /// <param name="serviceUri">The domain service Uri</param>
        /// <param name="usesHttps">A value indicating whether the client should contact
        /// the service using an HTTP or HTTPS scheme.
        /// </param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="serviceUri"/>
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException"> is thrown if <paramref name="serviceUri"/>
        /// is absolute and <paramref name="usesHttps"/> is true.
        /// </exception>
        public WebDomainClient(Uri serviceUri, bool usesHttps)
            : this(serviceUri, usesHttps, /* channelFactory */ null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDomainClient&lt;TContract&gt;"/> class.
        /// </summary>
        /// <param name="serviceUri">The domain service Uri</param>
        /// <param name="usesHttps">A value indicating whether the client should contact
        /// the service using an HTTP or HTTPS scheme.
        /// </param>
        /// <param name="channelFactory">The channel factory that creates channels to communicate with the server.</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="serviceUri"/>
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException"> is thrown if <paramref name="serviceUri"/>
        /// is absolute and <paramref name="usesHttps"/> is true.
        /// </exception>
        public WebDomainClient(Uri serviceUri, bool usesHttps, ChannelFactory<TContract> channelFactory)
        {
            if (serviceUri == null)
            {
                throw new ArgumentNullException("serviceUri");
            }

#if !SILVERLIGHT
            if (!serviceUri.IsAbsoluteUri)
            {
                // Relative URIs currently only supported on Silverlight
                throw new ArgumentException(OpenRiaServices.DomainServices.Client.Resource.DomainContext_InvalidServiceUri, "serviceUri");
            }
#endif

            this._serviceUri = serviceUri;
            this._usesHttps = usesHttps;
            this._channelFactory = channelFactory;

#if SILVERLIGHT
            // The domain client should not be initialized at design time
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.Initialize();
            }
#endif
        }

        /// <summary>
        /// Gets the absolute path to the domain service.
        /// </summary>
        /// <remarks>
        /// The value returned is either the absolute Uri passed into the constructor, or
        /// an absolute Uri constructed from the relative Uri passed into the constructor.
        /// Relative Uris will be made absolute using the Application Host source.
        /// </remarks>
        public Uri ServiceUri
        {
            get
            {
                return this._serviceUri;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="DomainClient"/> supports cancellation.
        /// </summary>
        public override bool SupportsCancellation
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether a secure connection should be used.
        /// </summary>
        public bool UsesHttps
        {
            get
            {
                return this._usesHttps;
            }
        }

        /// <summary>
        /// Gets the list of known types.
        /// </summary>
        private IEnumerable<Type> KnownTypes
        {
            get
            {
                if (this._knownTypes == null)
                {
                    // KnownTypes is the set of all types we'll need to serialize,
                    // which is the union of the entity types and the framework
                    // message types
                    List<Type> types = this.EntityTypes.ToList();
                    types.Add(typeof(QueryResult));
                    types.Add(typeof(DomainServiceFault));
                    types.Add(typeof(ChangeSetEntry));
                    types.Add(typeof(EntityOperationType));
                    types.Add(typeof(ValidationResultInfo));

                    this._knownTypes = types;
                }
                return this._knownTypes;
            }
        }

        /// <summary>
        /// Gets the channel factory that is used to create channels for communication 
        /// with the server.
        /// </summary>
        public ChannelFactory<TContract> ChannelFactory
        {
            get
            {
#if SILVERLIGHT
                // Initialization prepares the client for use and will fail at design time
                if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                {
                    throw new InvalidOperationException("Domain operations cannot be started at design time.");
                }
                this.Initialize();
#endif
                if (this._channelFactory == null)
                {
                    this._channelFactory = this.CreateDefaultChannelFactory();
                }

                if (!this._initializedFactory)
                {
                    foreach (OperationDescription op in this._channelFactory.Endpoint.Contract.Operations)
                    {
                        foreach (Type knownType in this.KnownTypes)
                        {
                            op.KnownTypes.Add(knownType);
                        }
                    }

                    this._initializedFactory = true;
                }

                return this._channelFactory;
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// Initializes this domain client
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the current application
        /// or its host are <c>null</c>.
        /// </exception>
        private void Initialize()
        {
            this.ComposeAbsoluteServiceUri();
        }
#endif

        /// <summary>
        /// Creates a default channel factory.
        /// </summary>
        /// <returns>The channel used to communicate with the server.</returns>
        private ChannelFactory<TContract> CreateDefaultChannelFactory()
        {
            ChannelFactory<TContract> factory = null;

            try
            {
                TransportBindingElement transport;
                if (this._serviceUri.Scheme == Uri.UriSchemeHttps)
                {
                    transport = new HttpsTransportBindingElement();
                }
                else
                {
                    transport = new HttpTransportBindingElement();
                }
                transport.ManualAddressing = true;
                transport.MaxReceivedMessageSize = WebDomainClient<TContract>.MaxReceivedMessageSize;

                // By default, use "REST" w/ binary encoding.
                PoxBinaryMessageEncodingBindingElement encoder = new PoxBinaryMessageEncodingBindingElement();
#if !SILVERLIGHT
                encoder.ReaderQuotas.MaxArrayLength = WebDomainClient<TContract>.MaxArrayLength;
                encoder.ReaderQuotas.MaxBytesPerRead = WebDomainClient<TContract>.MaxBytesPerRead;
                encoder.ReaderQuotas.MaxDepth = WebDomainClient<TContract>.MaxDepth;
                encoder.ReaderQuotas.MaxNameTableCharCount = WebDomainClient<TContract>.MaxNameTableCharCount;
                encoder.ReaderQuotas.MaxStringContentLength = WebDomainClient<TContract>.MaxStringContentLength;
#endif
                
                this._serviceUri = new Uri(this._serviceUri.OriginalString + "/binary", UriKind.Absolute);

                Binding binding = new CustomBinding(encoder, transport);
                factory = new ChannelFactory<TContract>(binding, new EndpointAddress(this._serviceUri));
                factory.Endpoint.Behaviors.Add(new WebDomainClientWebHttpBehavior()
                {
                    DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped
                });

#if DEBUG
                if (Debugger.IsAttached)
                {
                    // in debug mode set the timeout to a higher number to
                    // facilitate debugging
                    factory.Endpoint.Binding.OpenTimeout = TimeSpan.FromMinutes(5);
                }
#endif
            }
            catch(Exception ex)
            {
                ((IDisposable)factory).Dispose();
                throw;
            }

            return factory;
        }

        /// <summary>
        /// Method called by the framework to begin an asynchronous query operation
        /// </summary>
        /// <param name="query">The query to invoke.</param>
        /// <param name="callback">The callback to invoke when the query has been executed.</param>
        /// <param name="userState">Optional state associated with this operation.</param>
        /// <returns>An asynchronous result that identifies this query.</returns>
        /// <exception cref="InvalidOperationException">The specified query does not exist.</exception>
        protected sealed override IAsyncResult BeginQueryCore(EntityQuery query, AsyncCallback callback, object userState)
        {
            MethodInfo beginQueryMethod = WebDomainClient<TContract>.ResolveBeginMethod(query.QueryName);
            MethodInfo endQueryMethod = WebDomainClient<TContract>.ResolveEndMethod(query.QueryName);

            // Pass query parameters.
            ParameterInfo[] parameterInfos = beginQueryMethod.GetParameters();
            object[] realParameters = new object[parameterInfos.Length];
            int parametersCount = (query.Parameters == null) ? 0 : query.Parameters.Count;
            for (int i = 0; i < parametersCount; i++)
            {
                realParameters[i] = query.Parameters[parameterInfos[i].Name];
            }

            TContract channel = this.ChannelFactory.CreateChannel();

            WebDomainClientAsyncResult<TContract> wcfAsyncResult = WebDomainClientAsyncResult<TContract>.CreateQueryResult(this, channel, endQueryMethod, callback, userState);

            // Pass async operation related parameters.
            realParameters[parameterInfos.Length - 2] = new AsyncCallback(delegate(IAsyncResult asyncResponseResult)
            {
                wcfAsyncResult.InnerAsyncResult = asyncResponseResult;
                wcfAsyncResult.Complete();
            });
            realParameters[parameterInfos.Length - 1] = userState;

            IAsyncResult asyncResult;
            try
            {
                // Pass the query as a message property.
                using (OperationContextScope scope = new OperationContextScope((IContextChannel)channel))
                {
                    if (query.Query != null)
                    {
                        OperationContext.Current.OutgoingMessageProperties.Add(WebDomainClient<object>.QueryPropertyName, query.Query);
                    }
                    if (query.IncludeTotalCount)
                    {
                        OperationContext.Current.OutgoingMessageProperties.Add(WebDomainClient<object>.IncludeTotalCountPropertyName, true);
                    }

                    asyncResult = (IAsyncResult)beginQueryMethod.Invoke(channel, realParameters);
                }
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }

                throw;
            }

            if (!asyncResult.CompletedSynchronously)
            {
                wcfAsyncResult.InnerAsyncResult = asyncResult;
            }

            return wcfAsyncResult;
        }

        /// <summary>
        /// Attempts to cancel the query request specified by the <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> specifying what query operation to cancel.</param>
        protected sealed override void CancelQueryCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Query, /* cancel */ true);
            ((IChannel)wcfAsyncResult.Channel).Abort();
        }

        /// <summary>
        /// Gets the results of a query.
        /// </summary>
        /// <param name="asyncResult">An asynchronous result that identifies a query.</param>
        /// <returns>The results returned by the query.</returns>
        protected sealed override QueryCompletedResult EndQueryCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Query, /* cancel */ false);
            MethodInfo endQueryMethod = (MethodInfo)wcfAsyncResult.EndOperationMethod;

            IEnumerable<ValidationResult> validationErrors = null;
            QueryResult returnValue = null;
            try
            {
                try
                {
                    returnValue = (QueryResult)endQueryMethod.Invoke(wcfAsyncResult.Channel, new object[] { wcfAsyncResult.InnerAsyncResult });
                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException != null)
                    {
                        throw tie.InnerException;
                    }

                    throw;
                }
                finally
                {
                    ((IChannel)wcfAsyncResult.Channel).Close();
                }
            }
            catch (FaultException<DomainServiceFault> fe)
            {
                if (fe.Detail.OperationErrors != null)
                {
                    validationErrors = fe.Detail.GetValidationErrors();
                }
                else
                {
                    throw WebDomainClient<TContract>.GetExceptionFromServiceFault(fe.Detail);
                }
            }

            if (returnValue != null)
            {
                return new QueryCompletedResult(
                    returnValue.GetRootResults().Cast<Entity>(),
                    returnValue.GetIncludedResults().Cast<Entity>(),
                    returnValue.TotalCount,
                    Enumerable.Empty<ValidationResult>());
            }
            else
            {
                return new QueryCompletedResult(
                    new Entity[0],
                    new Entity[0],
                    /* totalCount */ 0,
                    validationErrors ?? Enumerable.Empty<ValidationResult>());
            }
        }

        /// <summary>
        /// Submit the specified <see cref="EntityChangeSet"/> to the DomainService, with the results of the operation
        /// being returned on the SubmitCompleted event args.
        /// </summary>
        /// <param name="changeSet">The changeset to submit. If the changeset is empty, an <see cref="InvalidOperationException"/> will
        /// be thrown.</param>
        /// <param name="callback">The callback to invoke when the submit has been executed.</param>
        /// <param name="userState">Optional state that will flow through to the SubmitCompleted event</param>
        /// <returns>An asynchronous result that identifies this submit.</returns>
        /// <exception cref="InvalidOperationException">The changeset is empty.</exception>
        /// <exception cref="InvalidOperationException">The specified query does not exist.</exception>
        protected sealed override IAsyncResult BeginSubmitCore(EntityChangeSet changeSet, AsyncCallback callback, object userState)
        {
            MethodInfo beginSubmitMethod = WebDomainClient<TContract>.ResolveBeginMethod("SubmitChanges");
            MethodInfo endSubmitMethod = WebDomainClient<TContract>.ResolveEndMethod("SubmitChanges");

            IEnumerable<ChangeSetEntry> submitOperations = changeSet.GetChangeSetEntries();

            TContract channel = this.ChannelFactory.CreateChannel();
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = WebDomainClientAsyncResult<TContract>.CreateSubmitResult(this, channel, endSubmitMethod, changeSet, submitOperations.ToList(), callback, userState);

            object[] parameters = 
            {
                submitOperations,
                new AsyncCallback(delegate(IAsyncResult asyncResponseResult)
                {
                    wcfAsyncResult.InnerAsyncResult = asyncResponseResult;
                    wcfAsyncResult.Complete();
                }),
                userState
            };

            IAsyncResult asyncResult;
            try
            {
                asyncResult = (IAsyncResult)beginSubmitMethod.Invoke(channel, parameters);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }

                throw;
            }

            if (!asyncResult.CompletedSynchronously)
            {
                wcfAsyncResult.InnerAsyncResult = asyncResult;
            }
            return wcfAsyncResult;
        }

        /// <summary>
        /// Attempts to cancel the submit request specified by the <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> specifying what submit operation to cancel.</param>
        protected sealed override void CancelSubmitCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Submit, /* cancel */ true);
            ((IChannel)wcfAsyncResult.Channel).Abort();
        }

        /// <summary>
        /// Gets the results of a submit.
        /// </summary>
        /// <param name="asyncResult">An asynchronous result that identifies a submit.</param>
        /// <returns>The results returned by the submit.</returns>
        protected sealed override SubmitCompletedResult EndSubmitCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Submit, /* cancel */ false);
            MethodInfo endSubmitMethod = wcfAsyncResult.EndOperationMethod;
            EntityChangeSet changeSet = wcfAsyncResult.EntityChangeSet;

            IEnumerable<ChangeSetEntry> returnValue;
            try
            {
                try
                {
                    returnValue = (IEnumerable<ChangeSetEntry>)endSubmitMethod.Invoke(wcfAsyncResult.Channel, new object[] { wcfAsyncResult.InnerAsyncResult });
                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException != null)
                    {
                        throw tie.InnerException;
                    }

                    throw;
                }
                finally
                {
                    ((IChannel)wcfAsyncResult.Channel).Close();
                }
            }
            catch (FaultException<DomainServiceFault> fe)
            {
                throw WebDomainClient<TContract>.GetExceptionFromServiceFault(fe.Detail);
            }

            return new SubmitCompletedResult(changeSet, returnValue ?? Enumerable.Empty<ChangeSetEntry>());
        }

        /// <summary>
        /// Invokes an operation asynchronously.
        /// </summary>
        /// <param name="invokeArgs">The arguments to the Invoke operation.</param>
        /// <param name="callback">The callback to invoke when the invocation has been completed.</param>
        /// <param name="userState">Optional user state that will be passed through on the <see cref="InvokeCompletedResult"/>.</param>
        /// <returns>An asynchronous result that identifies this invocation.</returns>
        /// <exception cref="InvalidOperationException">The specified query does not exist.</exception>
        protected sealed override IAsyncResult BeginInvokeCore(InvokeArgs invokeArgs, AsyncCallback callback, object userState)
        {
            MethodInfo beginInvokeMethod = WebDomainClient<TContract>.ResolveBeginMethod(invokeArgs.OperationName);
            MethodInfo endInvokeMethod = WebDomainClient<TContract>.ResolveEndMethod(invokeArgs.OperationName);

            // Pass operation parameters.
            ParameterInfo[] parameterInfos = beginInvokeMethod.GetParameters();
            object[] realParameters = new object[parameterInfos.Length];
            int parametersCount = (invokeArgs.Parameters == null) ? 0 : invokeArgs.Parameters.Count;
            for (int i = 0; i < parametersCount; i++)
            {
                realParameters[i] = invokeArgs.Parameters[parameterInfos[i].Name];
            }

            TContract channel = this.ChannelFactory.CreateChannel();
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = WebDomainClientAsyncResult<TContract>.CreateInvokeResult(this, channel, endInvokeMethod, invokeArgs, callback, userState);

            // Pass async operation related parameters.
            realParameters[parameterInfos.Length - 2] = new AsyncCallback(delegate(IAsyncResult asyncResponseResult)
            {
                wcfAsyncResult.InnerAsyncResult = asyncResponseResult;
                wcfAsyncResult.Complete();
            });
            realParameters[parameterInfos.Length - 1] = userState;

            IAsyncResult asyncResult;
            try
            {
                asyncResult = (IAsyncResult)beginInvokeMethod.Invoke(channel, realParameters);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }

                throw;
            }

            if (!asyncResult.CompletedSynchronously)
            {
                wcfAsyncResult.InnerAsyncResult = asyncResult;
            }
            return wcfAsyncResult;
        }

        /// <summary>
        /// Attempts to cancel the invocation request specified by the <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> specifying what invocation operation to cancel.</param>
        protected sealed override void CancelInvokeCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Invoke, /* cancel */ true);
            ((IChannel)wcfAsyncResult.Channel).Abort();
        }

        /// <summary>
        /// Gets the results of an invocation.
        /// </summary>
        /// <param name="asyncResult">An asynchronous result that identifies an invocation.</param>
        /// <returns>The results returned by the invocation.</returns>
        protected sealed override InvokeCompletedResult EndInvokeCore(IAsyncResult asyncResult)
        {
            WebDomainClientAsyncResult<TContract> wcfAsyncResult = this.EndAsyncResult(asyncResult, AsyncOperationType.Invoke, /* cancel */ false);
            MethodInfo endInvokeMethod = (MethodInfo)wcfAsyncResult.EndOperationMethod;

            IEnumerable<ValidationResult> validationErrors = null;
            object returnValue = null;
            try
            {
                try
                {
                    returnValue = endInvokeMethod.Invoke(wcfAsyncResult.Channel, new object[] { wcfAsyncResult.InnerAsyncResult });
                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException != null)
                    {
                        throw tie.InnerException;
                    }

                    throw;
                }
                finally
                {
                    ((IChannel)wcfAsyncResult.Channel).Close();
                }
            }
            catch (FaultException<DomainServiceFault> fe)
            {
                if (fe.Detail.OperationErrors != null)
                {
                    validationErrors = fe.Detail.GetValidationErrors();
                }
                else
                {
                    throw WebDomainClient<TContract>.GetExceptionFromServiceFault(fe.Detail);
                }
            }

            return new InvokeCompletedResult(returnValue, validationErrors ?? new ValidationResult[0]);
        }

        /// <summary>
        /// Transitions an <see cref="IAsyncResult"/> instance to a completed state.
        /// </summary>
        /// <param name="asyncResult">An asynchronous result that identifies an invocation.</param>
        /// <param name="operationType">The expected operation type.</param>
        /// <param name="cancel">Boolean indicating whether or not the operation has been canceled.</param>
        /// <returns>A <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> reference.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="asyncResult"/> is null.</exception>
        /// <exception cref="ArgumentException"> if <paramref name="asyncResult"/> is not of type <cref name="TAsyncResult"/>.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/> has been canceled.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/>'s End* method has already been invoked.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/> has not completed.</exception>
        private WebDomainClientAsyncResult<TContract> EndAsyncResult(IAsyncResult asyncResult, AsyncOperationType operationType, bool cancel)
        {
            WebDomainClientAsyncResult<TContract> wcfClientResult = asyncResult as WebDomainClientAsyncResult<TContract>;

            if ((wcfClientResult != null) && (!object.ReferenceEquals(this, wcfClientResult.DomainClient) || wcfClientResult.AsyncOperationType != operationType))
            {
                throw new ArgumentException(Resources.WrongAsyncResult, "asyncResult");
            }

            return AsyncResultBase.EndAsyncOperation<WebDomainClientAsyncResult<TContract>>(asyncResult, cancel);
        }

        private static MethodInfo ResolveBeginMethod(string operationName)
        {
            MethodInfo m = typeof(TContract).GetMethod("Begin" + operationName);
            if (m == null)
            {
                throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, OpenRiaServices.DomainServices.Client.Resource.WebDomainClient_OperationDoesNotExist, operationName));
            }
            return m;
        }

        private static MethodInfo ResolveEndMethod(string operationName)
        {
            MethodInfo m = typeof(TContract).GetMethod("End" + operationName);
            if (m == null)
            {
                throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, OpenRiaServices.DomainServices.Client.Resource.WebDomainClient_OperationDoesNotExist, operationName));
            }
            return m;
        }

        /// <summary>
        /// Constructs an exception based on a service fault.
        /// </summary>
        /// <param name="serviceFault">The fault received from a service.</param>
        /// <returns>The constructed exception.</returns>
        private static Exception GetExceptionFromServiceFault(DomainServiceFault serviceFault)
        {
            // Status was OK but there still was a server error. We need to transform
            // the error into the appropriate client exception
            if (serviceFault.IsDomainException)
            {
                return new DomainException(serviceFault.ErrorMessage, serviceFault.ErrorCode, serviceFault.StackTrace);
            }
            else if (serviceFault.ErrorCode == 400)
            {
                return new DomainOperationException(serviceFault.ErrorMessage, OperationErrorStatus.NotSupported, serviceFault.ErrorCode, serviceFault.StackTrace);
            }
            else if (serviceFault.ErrorCode == 401)
            {
                return new DomainOperationException(serviceFault.ErrorMessage, OperationErrorStatus.Unauthorized, serviceFault.ErrorCode, serviceFault.StackTrace);
            }
            else
            {
                // for anything else: map to ServerError
                return new DomainOperationException(serviceFault.ErrorMessage, OperationErrorStatus.ServerError, serviceFault.ErrorCode, serviceFault.StackTrace);
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// If the service Uri is relative, this method uses the application
        /// source to create an absolute Uri.
        /// </summary>
        /// <remarks>
        /// If usesHttps in the constructor was true, the Uri will be created using
        /// a https scheme instead.
        /// </remarks>
        private void ComposeAbsoluteServiceUri()
        {
            // if the URI is relative, compose with the source URI
            if (!this._serviceUri.IsAbsoluteUri)
            {
                Application current = Application.Current;

                // Only proceed if we can determine a root uri
                if ((current == null) || (current.Host == null) || (current.Host.Source == null))
                {
                    throw new InvalidOperationException(OpenRiaServices.DomainServices.Client.Resource.DomainClient_UnableToDetermineHostUri);
                }

                string sourceUri = current.Host.Source.AbsoluteUri;
                if (this._usesHttps)
                {
                    // We want to replace a http scheme (everything before the ':' in a Uri) with https.
                    // Doing this via UriBuilder loses the OriginalString. Unfortunately, this leads
                    // the builder to include the original port in the output which is not what we want.
                    // To stay as close to the original Uri as we can, we'll just do some simple string
                    // replacement.
                    //
                    // Desired output: http://my.domain/mySite.aspx -> https://my.domain/mySite.aspx
                    // Builder output: http://my.domain/mySite.aspx -> https://my.domain:80/mySite.aspx
                    //   The actual port is probably 443, but including it increases the cross-domain complexity.
                    if (sourceUri.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceUri = "https:" + sourceUri.Substring(5 /*("http:").Length*/);
                    }
                }

                this._serviceUri = new Uri(new Uri(sourceUri), this._serviceUri);
            }
        }
#endif
    }
}
