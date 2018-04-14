using System;
using System.Configuration;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Auth.Models;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using NLog;
using Utils;

namespace Auth
{
    public class ExternalAuthService : IAuthService
    {
        private readonly IAuthDataService _dataService;
        private readonly ILogger _logger;
        private readonly IDataProtector _dataProtector;

        private const string DefaultKey = "7555FA64-938B-468A-BFDB-A1DE02CDBFC4";

        public ExternalAuthService(IAuthDataService dataService, ILogger logger)
        {
            _dataService = dataService;
            _logger = logger;
            var key = ConfigurationManager.AppSettings["dataProtectorKey"] ?? DefaultKey;
            _dataProtector = new AesDataProtector(key);
        }

        public IObservable<Token> Authorize(TokenRequest tokenRequest, IOwinContext context)
        {
            try
            {
                ValidateTokenRequest(tokenRequest);
            }
            catch (Exception e)
            {
                return Observable.Throw<Token>(e);
            }

            return GetExternalUser(tokenRequest)
                .Select(GenerateToken)
                .SelectMany(CreateDbSession)
                .LogInfo(_logger, t => $"User {tokenRequest.Username} authorized")
                .LogException(_logger, e => $"failed to authorize user {tokenRequest.Username} - {e.Message}<{e.GetType().FullName}>");
        }

        public IObservable<long> GetSessionUserId(string token)
        {
            return _dataService.GetSessionUserId(CreateTokenHash(token));
        }

        public IObservable<ClaimsIdentity> GetIdentityFromToken(string token)
        {
            return Observable.Defer(() =>
            {
                var secureDataFormat = new TicketDataFormat(_dataProtector);
                var data = secureDataFormat.Unprotect(token);
                if (null == data)
                    throw new UnauthorizedAccessException($"Cannot deserialize {token} data");

                return Observable.Return(data.Identity);
            });
        }

        public IObservable<Token> ChangePassword(string currentPassword, string newPassword)
        {
            string userName = Thread.CurrentPrincipal.GetClaim<string>(TokenClaimType.UserName);

            return _dataService
                .GetExternalUserDataByUsername(userName)
                .SelectMany(user =>
                {
                    if (null == user || !CryptoHelper.VerifyHash(user.PasswordHash, currentPassword, user.PasswordSalt))
                    {
                        throw new UnauthorizedAccessException($"Could not authorize '{userName}' user");
                    }

                    string newSalt = DateTimeOffset.UtcNow.GetHashCode().ToString("X");
                    string newPasswordHash = CryptoHelper.CalculateHash(newPassword, newSalt);

                    return _dataService.ChangePassword(user.Id, newSalt, newPasswordHash, null).Select(_ => user);
                })
                .Select(GenerateToken)
                .SelectMany(CreateDbSession)
                .LogInfo(_logger, t => $"changed password for user {userName}")
                .LogException(_logger);            
        }

        public IObservable<Unit> ChangeUserPassword(long userId, string newPassword, string newPin)
        {
            return _dataService
                .GetExternalUserDataById(userId)
                .SelectMany(user =>
                {
                    string newSalt = DateTimeOffset.UtcNow.GetHashCode().ToString("X");
                    string newPasswordHash = CryptoHelper.CalculateHash(newPassword ?? Guid.NewGuid().ToString(), newSalt);
                    string newPinHash = CryptoHelper.CalculateHash(newPin ?? Guid.NewGuid().ToString(), newSalt);

                    return _dataService.ChangePassword(user.Id, newSalt, newPasswordHash, newPinHash).Select(_ => user);
                })
                .LogInfo(_logger, u => $"changed password for user  with Id{u.Username}")
                .Select(_ => Unit.Default)
                .LogException(_logger);
        }

        private IObservable<User> GetExternalUser(TokenRequest tokenRequest)
        {
            return _dataService
                .GetExternalUserDataByUsername(tokenRequest.Username)
                .Select(user =>
                {
                    if (user == null || !CryptoHelper.VerifyHash(user.PasswordHash, tokenRequest.Password, user.PasswordSalt))
                    {
                        throw new UnauthorizedAccessException($"Could not authorize '{tokenRequest.Username}' user");
                    }
                    return user;
                });
        }

        private void ValidateTokenRequest(TokenRequest tokenRequest)
        {
            if (null == tokenRequest)
            {
                throw new ArgumentNullException(nameof(tokenRequest));
            }
            if (tokenRequest?.GrantType == null)
            {
                throw new NotSupportedException("Grant type not specified");
            }
            if (!tokenRequest.GrantType.Trim().Equals("credentials", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException($"Unsupported grant type - {tokenRequest.GrantType}");
            }
        }

        private IObservable<Token> CreateDbSession(Tuple<Token, User> data)
        {
            var user = data.Item2;
            var token = data.Item1;
            return _dataService
                .CreateSession(user.Id, token.AccessToken, CreateTokenHash(token.AccessToken), token.ExpiresAt)
                .Select(_ => token);
        }

        private Tuple<Token, User> GenerateToken(User user)
        {
            var secureDataFormat = new TicketDataFormat(_dataProtector);
            var expirationDate = DateTimeOffset.UtcNow.AddDays(1);

            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaims(user.GetClaims());

            var tokenValue = secureDataFormat.Protect(new AuthenticationTicket(identity, new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = expirationDate
            }));

            var token = new Token()
            {
                AccessToken = tokenValue,
                TokenType = OAuthDefaults.AuthenticationType,
                ExpiresAt = expirationDate,
                ExpiresIn = (uint)(expirationDate - DateTime.UtcNow).TotalSeconds
            };

            return Tuple.Create(token, user);
        }

        private string CreateTokenHash(string token)
        {
            using (var sha1 = new SHA256Managed())
            {
                return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(token)));
            }
        }
    }
}
