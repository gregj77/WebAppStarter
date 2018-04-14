using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Sps.Auth.Models;

namespace Sps.Auth
{
    public static class AuthHelper
    {
        protected readonly IAuthDataService _dataService;
        private readonly IDataProtector _dataProtector;
        private const string DefaultKey = "7555FA64-938B-468A-BFDB-A1DE02CDBFC4";

        protected BaseAuthService(IAuthDataService dataService)
        {
            _dataService = dataService;
            var key = ConfigurationManager.AppSettings["dataProtectorKey"] ?? DefaultKey; 
            _dataProtector = new AesDataProtector(key);
        }

        public abstract Token Authorize(TokenRequest tokenRequest, IOwinContext context);

        public long GetSessionUserId(string token)
        {
            var userId = _dataService.GetSessionUserId(CreateTokenHash(token));
            if (userId == 0)
            {
                throw new UnauthorizedAccessException($"Token {token} is not authorized");
            }
            return userId;
        }

        public static IObservable<Token> GenerateToken(this IObservable<ExternalUser> userStream, DateTime expirationDate)
        {
            var secureDataFormat = new TicketDataFormat(_dataProtector);

            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaims(claims);

            var tokenValue = secureDataFormat.Protect(new AuthenticationTicket(identity, new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = expirationDate
            }));

            return new Token()
            {
                AccessToken = tokenValue,
                TokenType = OAuthDefaults.AuthenticationType,
                ExpiresAt = expirationDate,
                ExpiresIn = (uint)(expirationDate - DateTime.UtcNow).TotalSeconds
            };
        }

        public ClaimsIdentity GetIdentityFromToken(string token)
        {
            var secureDataFormat = new TicketDataFormat(_dataProtector);
            var data = secureDataFormat.Unprotect(token);
            if (null == data)
                throw new UnauthorizedAccessException($"Cannot deserialize {token} data");
            return data.Identity;
        }

        protected string CreateTokenHash(string token)
        {
            using (var sha1 = new SHA256Managed())
            {
                return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(token)));
            }
        }
    }
}
