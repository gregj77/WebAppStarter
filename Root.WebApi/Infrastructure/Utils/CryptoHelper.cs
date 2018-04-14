namespace Utils
{
    public static class CryptoHelper
    {
        public static string CalculateHash(string password, string salt = null)
        {
            return BCrypt.Net.BCrypt.HashPassword(SaltPassword(password, salt));
        }

        public static bool VerifyHash(string hash, string password, string salt = null)
        {
            return BCrypt.Net.BCrypt.Verify(SaltPassword(password, salt), hash);
        }

        private static string SaltPassword(string password, string salt = null)
        {
            return $"{password}_iPPs.{salt}";
        }
    }
}
