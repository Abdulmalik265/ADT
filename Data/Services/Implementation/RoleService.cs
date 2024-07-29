using Core.Models;
using Data.Entities;
using Data.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;
        public RoleService(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task<IEnumerable<RoleViewModel>> GetRoles()
        {
            var roleInDb = await _roleManager.Roles.ToListAsync();
            var roles = roleInDb.Select(x => new RoleViewModel
            {
                Name = x.Name,
                Id = x.Id

            }).ToList();

            return roles;
        }
    }
}
