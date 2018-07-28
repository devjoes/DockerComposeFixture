using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

// It seems that XUnit's package chain is broken. Had to extract this implementation from their code.

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="IDiagnosticMessage"/>.
    /// </summary>
    public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage

    {
        static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(DiagnosticMessage).GetInterfaces().Select(x => x.FullName));
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        public DiagnosticMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="message">The message to send</param>
        public DiagnosticMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="format">The format of the message to send</param>
        /// <param name="args">The arguments used to format the message</param>
        public DiagnosticMessage(string format, params object[] args)
        {
            Message = string.Format(format, args);
        }

        /// <inheritdoc/>
        public HashSet<string> InterfaceTypes => interfaceTypes;

        /// <inheritdoc/>
        public string Message { get; set; }
    }
}

namespace Xunit.Sdk
{
#if NETFRAMEWORK
    using System;
    using System.Security;

#if XUNIT_FRAMEWORK
    using System.Collections.Concurrent;
    using System.Runtime.Remoting;
#endif

    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
#if XUNIT_FRAMEWORK
        static ConcurrentBag<MarshalByRefObject> remoteObjects = new ConcurrentBag<MarshalByRefObject>();

        /// <summary>
        /// Creates a new instance of the <see cref="LongLivedMarshalByRefObject"/> type.
        /// </summary>
        protected LongLivedMarshalByRefObject()
        {
            remoteObjects.Add(this);
        }

        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        [SecuritySafeCritical]
        public static void DisconnectAll()
        {
            foreach (var remoteObject in remoteObjects)
                RemotingServices.Disconnect(remoteObject);

            remoteObjects = new ConcurrentBag<MarshalByRefObject>();
        }
#else
        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        public static void DisconnectAll() { }
#endif

        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed object InitializeLifetimeService()
        {
            return null;
        }
    }
#else
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject
    {
        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        public static void DisconnectAll() { }
    }
#endif
}