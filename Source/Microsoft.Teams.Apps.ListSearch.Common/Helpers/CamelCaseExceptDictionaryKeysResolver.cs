// <copyright file="CamelCaseExceptDictionaryKeysResolver.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Camel Case for Dictionary Resolver
    /// </summary>
    public class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// Create Dictionary Contract
        /// </summary>
        /// <param name="objectType">Type of object.</param>
        /// <returns>Json Dictionary Contract</returns>
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}
