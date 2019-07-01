// <copyright file="RankerTypes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    /// <summary>
    /// Represents the ranker type to use when querying a knowledge base
    /// </summary>
    public class RankerTypes
    {
        /// <summary>
        /// Ranks results based on the question alone.
        /// </summary>
        public const string QuestionOnly = "QuestionOnly";

        /// <summary>
        /// Ranks results based on the question, in a matter suitable for suggestions.
        /// </summary>
        public const string AutoSuggestQuestion = "AutoSuggestQuestion";
    }
}
