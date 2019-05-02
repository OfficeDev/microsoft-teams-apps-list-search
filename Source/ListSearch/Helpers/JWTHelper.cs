// <copyright file="JWTHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Helper class for JWT
    /// </summary>
    public static class JWTHelper
    {
        private const string JwtAuthenticationType = "Custom";
        private const string ClaimTypeSender = "Sender";
        private const string ClaimTypeUserTeamsId = "UserTeamsId";
        private const string ClaimTypeUserAadId = "UserAadId";
        private const string ClaimTypeTenant = "Tenant";

        /// <summary>
        /// Generates JWT
        /// </summary>
        /// <param name="userTeamsId">Teams id of sender</param>
        /// <param name="userAadId">Aad object Id of sender</param>
        /// <param name="tenantId">Id of the tenant</param>
        /// <returns><see cref="string"/> that represents generated jwt</returns>
        public static string GenerateJWT(string userTeamsId, string userAadId, string tenantId)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["JWTSecurityKey"]));
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new List<Claim>()
                    {
                        new Claim(ClaimTypeSender, "bot"),
                        new Claim(ClaimTypeUserTeamsId, userTeamsId),
                        new Claim(ClaimTypeUserAadId, userAadId),
                        new Claim(ClaimTypeTenant, tenantId)
                    }, JwtAuthenticationType),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Issuer = ConfigurationManager.AppSettings["MicrosoftAppId"],
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["JWTExpiryMinutes"])),
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(securityTokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate JWT
        /// </summary>
        /// <param name="jwt">jwt to be validated</param>
        /// <returns><see cref="bool"/> representing success/failure of jwt validation</returns>
        public static bool ValidateJWT(string jwt)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["JWTSecurityKey"]));
            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = ConfigurationManager.AppSettings["MicrosoftAppId"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            try
            {
                SecurityToken mytoken = new JwtSecurityToken();
                JwtSecurityTokenHandler myTokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal myPrincipal = myTokenHandler.ValidateToken(jwt, validationParameters, out mytoken);

                JwtSecurityToken claimsValidator = (JwtSecurityToken)mytoken;
                if (!myPrincipal.HasClaim(ClaimTypeTenant, ConfigurationManager.AppSettings["TenantId"]))
                {
                    throw new SecurityTokenException($"Claim for {ClaimTypeTenant} does not match the expected value.");
                }

                return true;
            }
            catch
            {
                // TODO: Log ex
                throw;
            }
        }
    }
}