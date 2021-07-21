using IdentityModel;
using Mango.Service.Identity.DbContexts;
using Mango.Service.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mango.Service.Identity.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManger;

        public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManger)
        {
            _db = db;
            _userManager = userManager;
            _roleManger = roleManger;
        }

        public void Initialize()
        {
            if (_roleManger.FindByIdAsync(SD.Admin).Result == null)
            {
                _roleManger.CreateAsync(new IdentityRole(SD.Admin)).GetAwaiter().GetResult();
                _roleManger.CreateAsync(new IdentityRole(SD.Customer)).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }

            ApplicationUser adminUser = new ApplicationUser()
            {
                UserName = "admin1@gmail.com",
                Email = "admin1@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "7034239045",
                FastName ="Shebin",
                LastName ="C Babu"
            };

            _userManager.CreateAsync(adminUser,"Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, SD.Admin).GetAwaiter().GetResult();
            var temp1 =_userManager.AddClaimsAsync(adminUser, new Claim[] { 
                new Claim(JwtClaimTypes.Name,adminUser.FastName+" "+adminUser.LastName),
                new Claim(JwtClaimTypes.GivenName,adminUser.FastName),
                new Claim(JwtClaimTypes.FamilyName,adminUser.LastName),
                new Claim(JwtClaimTypes.Role,SD.Admin)       
            }).Result;

            ApplicationUser customerUser = new ApplicationUser()
            {
                UserName = "customer1@gmail.com",
                Email = "customer1@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "7034239045",
                FastName = "Thomas",
                LastName = "C"
            };

            _userManager.CreateAsync(customerUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(customerUser, SD.Customer).GetAwaiter().GetResult();
            var temp2 = _userManager.AddClaimsAsync(customerUser, new Claim[] {
                new Claim(JwtClaimTypes.Name,customerUser.FastName+" "+adminUser.LastName),
                new Claim(JwtClaimTypes.GivenName,customerUser.FastName),
                new Claim(JwtClaimTypes.FamilyName,customerUser.LastName),
                new Claim(JwtClaimTypes.Role,SD.Customer)
            }).Result;
        }
           
    }
}
