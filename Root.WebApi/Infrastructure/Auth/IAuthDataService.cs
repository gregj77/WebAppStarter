using System;
using System.Collections.Generic;
using System.Reactive;
using Auth.Models;

namespace Auth
{
    public interface IAuthDataService
    {
        IObservable<Unit> CreateSession(long userId, string token, string tokenHash, DateTimeOffset expirationDate);

        IObservable<long> GetSessionUserId(string tokenHash);

        IObservable<User> GetExternalUserDataByUsername(string username);

        IObservable<User> GetExternalUserDataById(long id);

        IObservable<User> GetExternalUserDataByEmail(string email);

        IObservable<Unit> ChangePassword(long userId, string salt, string passwordHash, string pinHash);

        //IObservable<ICollection<User>> FindUsers(UserQueryArguments args);

        IObservable<User> CreateOrUpdateUser(User user, string getUserClaim);
    }
}
