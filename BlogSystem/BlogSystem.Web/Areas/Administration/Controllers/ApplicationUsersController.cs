﻿namespace BlogSystem.Web.Areas.Administration.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using AutoMapper.QueryableExtensions;

    using BlogSystem.Data.Models;
    using BlogSystem.Data.UnitOfWork;
    using BlogSystem.Web.Areas.Administration.InputModels.ApplicationUsers;
    using BlogSystem.Web.Areas.Administration.ViewModels.ApplicationUsers;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;

    using PagedList;

    public class ApplicationUsersController : AdministrationController
    {
        private const int UsersPerPageDefaultValue = 6;

        public ApplicationUsersController(IBlogSystemData data)
            : base(data)
        {
        }

        // GET: Administration/ApplicationUsers
        public ActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;

            var users = this.Data.Users.All().OrderByDescending(x => x.CreatedOn).ProjectTo<ApplicationUserViewModel>().ToList();
            var model = new PagedList<ApplicationUserViewModel>(users, pageNumber, UsersPerPageDefaultValue);

            return this.View(model);
        }

        // GET: Administration/ApplicationUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var applicationUser = this.Data.Users.Find(id);
            if (applicationUser == null)
            {
                return this.HttpNotFound();
            }

            return this.View(applicationUser);
        }

        // GET: Administration/ApplicationUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var applicationUser = this.Data.Users.Find(id);
            if (applicationUser == null)
            {
                return this.HttpNotFound();
            }

            var model = new ApplicationUserEditModel
                            {
                                Id = applicationUser.Id, 
                                Email = applicationUser.Email, 
                                UserName = applicationUser.UserName, 
                                PasswordHash = applicationUser.PasswordHash, 
                                SecurityStamp = applicationUser.SecurityStamp, 
                                CreatedOn = applicationUser.CreatedOn
                            };

            return this.View(model);
        }

        // POST: Administration/ApplicationUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUserEditModel applicationUser)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.Data.Users.Find(applicationUser.Id);
                var checkEmail = this.Data.Users.All().Any(e => e.Email == applicationUser.Email);
                var checkUsername = this.Data.Users.All().Any(u => u.UserName == applicationUser.UserName);

                if (checkEmail && user.Email != applicationUser.Email)
                {
                    return this.Content($"Email {applicationUser.Email} is already taken.");
                }

                if (checkUsername && user.UserName != applicationUser.UserName)
                {
                    return this.Content($"Username {applicationUser.UserName} is already taken.");
                }

                user.Id = applicationUser.Id;
                user.Email = applicationUser.Email;
                user.UserName = applicationUser.UserName;
                user.CreatedOn = applicationUser.CreatedOn;

                if (user.PasswordHash != applicationUser.PasswordHash)
                {
                    var userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>());
                    var newHashedPassword = userManager.PasswordHasher.HashPassword(applicationUser.PasswordHash);
                    user.PasswordHash = newHashedPassword;
                    user.SecurityStamp = Guid.NewGuid().ToString();
                }

                this.Data.Users.Update(user);
                this.Data.SaveChanges();

                return this.RedirectToAction("Index");
            }

            return this.View(applicationUser);
        }

        // GET: Administration/ApplicationUsers/Create
        public ActionResult Create()
        {
            return this.View();
        }

        // POST: Administration/ApplicationUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationUserCreateModel userModel)
        {
            if (this.ModelState.IsValid)
            {
                var applicationUser = new ApplicationUser
                                          {
                                              Email = userModel.Email, 
                                              UserName = userModel.UserName, 
                                              PasswordHash = userModel.Password, 
                                              CreatedOn = DateTime.Now
                                          };

                var userManager = this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var userCreateResult = userManager.Create(applicationUser, applicationUser.PasswordHash);
                if (!userCreateResult.Succeeded)
                {
                    return this.Content(string.Join("; ", userCreateResult.Errors));
                }

                this.Data.SaveChanges();

                return this.RedirectToAction("Index");
            }

            return this.View(userModel);
        }
    }
}