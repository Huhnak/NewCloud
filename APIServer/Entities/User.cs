using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIServer.Entities
{
    public class User
    {
        public User()
        {
            this.RefreshTokens = new List<RefreshToken>();
        }
        public int Id { get; set; }

        public string Username { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

        [Display(Name = "Size in bytes")]
        public long Size { get; set; }
        [Display(Name = "Size in bytes")]
        public long SpaceLeft { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; }
    }
}
