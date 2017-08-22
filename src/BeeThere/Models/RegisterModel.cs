using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WannaGo.Models
{
    public class ApplicationUserRegistrationModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string Password { get; set; }
        public string Image { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
