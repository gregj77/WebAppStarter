using System;

namespace Auth.Models
{
    public class RootUser
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Guid UserCode { get; set; }
        public UserType UserType { get; set; }
    }
}
