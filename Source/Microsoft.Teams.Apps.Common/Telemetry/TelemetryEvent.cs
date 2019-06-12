// <copyright file="TelemetryEvent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Telemetry
{
    /// <summary>
    /// Event types
    /// </summary>
    public enum TelemetryEvent
    {
        /// <summary>
        /// Activity received by the bot
        /// </summary>
        UserActivity,

        /// <summary>
        /// Bot detected a command at the root dialog
        /// </summary>
        TopLevelCommand,

        /// <summary>
        /// User executed a command on a card
        /// </summary>
        CardCommand,

        /// <summary>
        /// Bot received input that it could not recognize
        /// </summary>
        UnrecognizedIntent,

        /// <summary>
        /// Events related to a dialog flow
        /// </summary>
        DialogEvent,
    }
}
