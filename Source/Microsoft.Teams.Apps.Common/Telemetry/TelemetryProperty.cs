// <copyright file="TelemetryProperty.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Telemetry
{
    /// <summary>
    /// Telemetry properties
    /// </summary>
    public enum TelemetryProperty
    {
        /// <summary>
        /// Type of incoming activity
        /// </summary>
        ActivityType,

        /// <summary>
        /// The activity id
        /// </summary>
        ActivityId,

        /// <summary>
        /// The Teams-specific event type
        /// </summary>
        TeamsEventType,

        /// <summary>
        /// The 29:xxx user id of the user
        /// </summary>
        UserId,

        /// <summary>
        /// The AAD object id of the user
        /// </summary>
        UserAadObjectId,

        /// <summary>
        /// The conversation id
        /// </summary>
        ConversationId,

        /// <summary>
        /// The conversation type
        /// </summary>
        ConversationType,

        /// <summary>
        /// The client platform
        /// </summary>
        Platform,

        /// <summary>
        /// The client locale
        /// </summary>
        Locale,

        /// <summary>
        /// The command that was invoked
        /// </summary>
        CommandName,

        /// <summary>
        /// The dialog id
        /// </summary>
        DialogId,

        /// <summary>
        /// The dialog instance id
        /// </summary>
        DialogInstanceId,

        /// <summary>
        /// [OperationStep] The event step in a multi-step process
        /// </summary>
        Step,

        /// <summary>
        /// [OperationResult] The result of a multi-step process
        /// </summary>
        Result,

        /// <summary>
        /// The status/error code
        /// </summary>
        StatusCode,

        /// <summary>
        /// The kind of card that was invoked
        /// </summary>
        CardType,

        /// <summary>
        /// The command on the card that was invoked
        /// </summary>
        CardCommand,
    }
}
