// <copyright file="SettingDeserializationException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception for setting deserialization.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class SettingDeserializationException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDeserializationException"/> class.
        /// </summary>
        public SettingDeserializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDeserializationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SettingDeserializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDeserializationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If no inner exception is specified then returns null reference.
        /// </param>
        public SettingDeserializationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDeserializationException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected SettingDeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}