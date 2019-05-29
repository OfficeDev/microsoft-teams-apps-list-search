// <copyright file="JwtHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Helper class for JWT
    /// </summary>
    public class JwtHelper
    {
        private const string JwtAuthenticationType = "Custom";
        private const string ClaimTypeSender = "Sender";
        private const string ClaimTypeUserTeamsId = "UserTeamsId";
        private const string ClaimTypeUserAadId = "UserAadId";
        private const string ClaimTypeTenant = "Tenant";

        private readonly string jwtSecurityKey;
        private readonly string appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtHelper"/> class.
        /// </summary>
        /// <param name="jwtSecurityKey">security key</param>
        /// <param name="botId">microsoft app id of bot</param>
        public JwtHelper(string jwtSecurityKey, string botId)
        {
            this.jwtSecurityKey = jwtSecurityKey;
            this.appId = botId;
        }

        /// <summary>
        /// Generates JWT
        /// </summary>
        /// <param name="userTeamsId">Teams id of sender</param>
        /// <param name="userAadId">Aad object Id of sender</param>
        /// <param name="botTenantId">Tenant Id of bot</param>
        /// <param name="jwtExpiryMinutes">minutes in which jwt expires</param>
        /// <returns><see cref="string"/> that represents generated jwt</returns>
        public string GenerateJWT(string userTeamsId, string userAadId, string botTenantId, int jwtExpiryMinutes)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.jwtSecurityKey));
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new List<Claim>()
                    {
                        new Claim(ClaimTypeSender, "bot"),
                        new Claim(ClaimTypeUserTeamsId, userTeamsId),
                        new Claim(ClaimTypeUserAadId, userAadId),
                        new Claim(ClaimTypeTenant, botTenantId)
                    }, JwtAuthenticationType),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Issuer = this.appId,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(securityTokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate JWT
        /// </summary>
        /// <param name="jwt">jwt to be validated</param>
        /// <param name="acceptingTenantId">TenantId of web app</param>
        /// <returns><see cref="bool"/> representing success/failure of jwt validation</returns>
        public bool ValidateJWT(string jwt, string acceptingTenantId)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.jwtSecurityKey));
            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = this.appId,
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
                if (!myPrincipal.HasClaim(ClaimTypeTenant, acceptingTenantId))
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
