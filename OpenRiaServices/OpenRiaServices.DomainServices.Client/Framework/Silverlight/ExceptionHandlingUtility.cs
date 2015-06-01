using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenRiaServices.DomainServices
{
    internal static class ExceptionHandlingUtility
    {
        /// <summary>
        /// Determines if an <see cref="Exception"/> is fatal and therefore should not be handled.
        /// </summary>
        /// <example>
        /// try
        /// {
        ///     // Code that may throw
        /// }
        /// catch (Exception ex)
        /// {
        ///     if (ex.IsFatal())
        ///     {
        ///         throw;
        ///     }
        ///     
        ///     // Handle exception
        /// }
        /// </example>
        /// <param name="exception">The exception to check</param>
        /// <returns><c>true</c> if the exception is fatal, otherwise <c>false</c>.</returns>
        public static bool IsFatal(this Exception exception)
        {
            return IsFatalCore(exception, outerException:  null);
        }

        private static bool IsFatalCore(Exception exception, Exception outerException)
        {
            if (exception == null)
                return false;

            if (IsFatalExceptionType(exception))
            {
                Debug.Assert(outerException == null 
                    || (outerException is TypeInitializationException) 
                    || (outerException is TargetInvocationException)
                    || (outerException is AggregateException),
                    "Fatal nested exception found");
                return true;
            }

            var ae = exception as AggregateException;
            if (ae != null)
                return ae.InnerExceptions.Any(inner => IsFatalCore(inner, exception));
            else
                return IsFatalCore(exception.InnerException, exception);
        }

        private static bool IsFatalExceptionType(Exception exception)
        {
            if ((exception is ThreadAbortException) ||
#if SILVERLIGHT
                (exception is OutOfMemoryException))
#else
                ((exception is OutOfMemoryException) && !(exception is InsufficientMemoryException)))
#endif
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to unwrap a TargetInvocationException and AggregateException.
        /// </summary>
        /// <param name="e">The exception to unwrap.</param>
        /// <returns>the first inner exception which is neither a TargetInvocationException nor a AggregateException</returns>
        public static Exception GetUnwrappedException(Exception e)
        {
            while (e.InnerException != null && (e is TargetInvocationException || e is AggregateException))
            {
                e = e.InnerException;
            }
            return e;
        }
    }
}