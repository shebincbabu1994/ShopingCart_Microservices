using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Service.Identity.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FastName { get; set; }
        public string LastName { get; set; }
    }
}
