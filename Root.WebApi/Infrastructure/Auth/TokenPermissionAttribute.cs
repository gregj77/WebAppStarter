using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class TokenPermissionAttribute : Attribute, IEquatable<TokenPermissionAttribute>, IEquatable<Claim>
    {
        private readonly string _claimType;
        private readonly ICollection<string> _value;

        public string ClaimType => _claimType;
        public string Value => string.Join(",", _value);

        public TokenPermissionAttribute(string claimType, string value) 
        {
            _claimType = claimType;
            _value = new[] { value };
        }

        public TokenPermissionAttribute(string claimType, params string[] values)
        {
            _claimType = claimType;
            _value = values;
        }
        public bool Equals(TokenPermissionAttribute other)
        {
            return Equals((object)other);
        }

        public bool Equals(Claim other)
        {
            return Equals((object)other);
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;

            TokenPermissionAttribute rhsAtr = other as TokenPermissionAttribute;
            if (null != rhsAtr)
            {
                bool result = string.Equals(_claimType, rhsAtr._claimType, StringComparison.OrdinalIgnoreCase);
                result &= _value.All(v => rhsAtr._value.Any(r => string.Equals(v, r, StringComparison.OrdinalIgnoreCase)));
                return result;
            }

            Claim rhsClaim = other as Claim;
            if (null != rhsClaim)
            {
                bool result = string.Equals(_claimType, rhsClaim.Type, StringComparison.OrdinalIgnoreCase);
                result &= _value.Any(v => string.Equals(v, rhsClaim.Value, StringComparison.OrdinalIgnoreCase));
                return result;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (_claimType?.GetHashCode() ?? 0) ^ (_value?.GetHashCode() ?? 0);
        }
    }
}
