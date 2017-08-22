using WannaGo.Models;
using WannaGo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WannaGo.Utility;
using System.IO;

namespace WannaGo.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext applicationDbContext;
        private static bool databaseChecked;
        private S3Uploader s3uploader;

        public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            applicationDbContext = context;
            s3uploader = new S3Uploader();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null && file != null)
            {
                var stream = new MemoryStream();
                var fileName = "profile_images/" + Guid.NewGuid() + "_" + file.FileName;
                await file.CopyToAsync(stream);
                var result = await s3uploader.UploadFile(stream, fileName);
                if (result)
                {
                    user.Image = s3uploader.GetBaseUrl() + fileName;
                    applicationDbContext.Update(user);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok();
                }

            }
            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ApplicationUserRegistrationModel model)
        {
            EnsureDatabaseCreated(applicationDbContext);
            Console.Write(model.Username);
            if (ModelState.IsValid)
            {
                var createdTime = DateTime.UtcNow;
                var user = new ApplicationUser()
                {
                    UserName = model.Username,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    DateOfBirth = model.DateOfBirth,
                    Image = model.Image,
                    Gender = model.Gender,
                    JoinedDate = createdTime,
                    LastEdit = createdTime,
                };
               
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                    return Ok();
                else
                {
                    foreach(var item in result.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(item.Description);
                    }
                }
            }
            return BadRequest();
        }
        [HttpGet]
        [Authorize]
        public async Task<ApplicationUserRegistrationModel> Get()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var model = new ApplicationUserRegistrationModel()
            {
                Username = user.UserName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth.Date,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Image = user.Image,
            };
            return model;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit([FromBody] ApplicationUserRegistrationModel model)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user != null)

            {
                user.Email = model.Email;
                user.UserName = model.Username;
                user.Image = model.Image;
                user.LastEdit = DateTime.UtcNow;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.DateOfBirth = model.DateOfBirth.Date;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return Ok();
                else
                    return BadRequest(result.Errors);
            }
            return BadRequest("Could not find user, are you signed in?");

        }
        private static void EnsureDatabaseCreated(ApplicationDbContext context)
        {
            if (!databaseChecked)
            {
                databaseChecked = true;
                context.Database.EnsureCreated();
            }
        }
    } }
