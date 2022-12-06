using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace APIServer.Models
{
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
