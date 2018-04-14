using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Auth.Models;

namespace Auth
{
    public class AuthDataService : IAuthDataService
    {
        private readonly SynchronizationContext _ctx;

        private static readonly User[] InMemoryUsers = 
        {
            new User { Id = 1, Email = "a@b.c.pl", Username = "test", PasswordHash = "$2a$10$fk7njBqnYmi47AM3ZJUn1.QQcdZSRjLiL7m1MwfizLypwCCqflPHG", PasswordSalt = "9DF3344", Name = "john", Surname = "doe"}
        };

        private static readonly IDictionary<string, Tuple<DateTimeOffset, long>> InMemorySessions = new ConcurrentDictionary<string, Tuple<DateTimeOffset, long>>();

        public AuthDataService(SynchronizationContext ctx)
        {
            _ctx = ctx;
        }

        public IObservable<Unit> CreateSession(long userId, string token, string tokenHash, DateTimeOffset expirationDate)
        {
            InMemorySessions.Add(tokenHash, Tuple.Create(expirationDate, userId));
            return Observable.Return(Unit.Default);
        }

        public IObservable<long> GetSessionUserId(string tokenHash)
        {
            return Observable.Defer(() => Observable.Return(InMemorySessions.Where(p => p.Key == tokenHash).Where(p => p.Value.Item1 >= DateTimeOffset.Now).Select(p => p.Value.Item2).Single()));
        }

        public IObservable<User> GetExternalUserDataByUsername(string username)
        {
            return Observable.Defer(() => Observable.Return(InMemoryUsers.First(p => p.Username == username)));
        }

        public IObservable<User> GetExternalUserDataById(long id)
        {
            return Observable.Defer(() => Observable.Return(InMemoryUsers.First(p => p.Id == id)));
        }

        public IObservable<User> GetExternalUserDataByEmail(string email)
        {
            return Observable.Defer(() => Observable.Return(InMemoryUsers.First(p => p.Email == email)));
        }

        public IObservable<Unit> ChangePassword(long id, string passwordSalt, string passwordHash, string pinHash)
        {
            return Observable.Throw<Unit>(new NotImplementedException());
        }

        public IObservable<User> CreateOrUpdateUser(User user, string username)
        {
            return Observable.Throw<User>(new NotImplementedException());
        }

    }
}
