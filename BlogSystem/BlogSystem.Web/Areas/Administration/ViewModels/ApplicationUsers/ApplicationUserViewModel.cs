﻿namespace BlogSystem.Web.Areas.Administration.ViewModels.ApplicationUsers
{
    using System;
    using System.Linq;

    using AutoMapper;

    using BlogSystem.Common;
    using BlogSystem.Data;
    using BlogSystem.Data.Models;
    using BlogSystem.Web.Infrastructure.Mapping;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class ApplicationUserViewModel : IMapFrom<ApplicationUser>, IHaveCustomMappings
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            var context = new ApplicationDbContext();
            var rolerManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var administratorRoleId =
                rolerManager.Roles.Where(r => r.Name == GlobalConstants.AdminRoleName)
                    .Select(r => r.Id)
                    .FirstOrDefault();

            configuration.CreateMap<ApplicationUser, ApplicationUserViewModel>()
                .ForMember(
                    model => model.IsAdmin, 
                    config => config.MapFrom(e => e.Roles.Any(r => r.RoleId == administratorRoleId)));
        }
    }
}