using System;
using System.Reactive;
using System.Security.Claims;
using Auth.Models;
using Microsoft.Owin;

namespace Auth
{
    public interface IAuthService
    {
        IObservable<Token> Authorize(TokenRequest tokenRequest, IOwinContext context);

        IObservable<long> GetSessionUserId(string token);

        IObservable<ClaimsIdentity> GetIdentityFromToken(string token);

        IObservable<Token> ChangePassword(string currentPassword, string newPassword);

        IObservable<Unit> ChangeUserPassword(long userId, string newPassword, string newPin);
    }
}
