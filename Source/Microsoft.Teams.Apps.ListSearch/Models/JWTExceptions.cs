// <copyright file="JWTExceptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    /// <summary>
    /// JWT Exception
    /// </summary>
    public static class JWTExceptions
    {
        /// <summary>
        /// Exception code for lifetime validation failure of jwt
        /// </summary>
        public static readonly string LifetimeValidationFailedExceptionCode = "IDX10223";

        /// <summary>
        /// Expected sources for JWT Exceptions
        /// </summary>
        public static readonly string[] ExpectedJWTExceptionSources = new string[] { "System.IdentityModel.Tokens.Jwt", "Microsoft.IdentityModel.Tokens" };
    }
}