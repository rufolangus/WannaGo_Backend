using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WannaGo.Models;
using WannaGo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using WannaGo.Utility;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WannaGo.Controllers
{
    [Route("api/[controller]/[action]")]
    public class EventController : Controller
    {
        private ApplicationDbContext dB;
        private UserManager<ApplicationUser> userManager;
        private IHostingEnvironment enviroment;
        private S3Uploader uploader;

        public EventController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
                                IHostingEnvironment enviroment)
        {
            uploader = new S3Uploader();
            this.enviroment = enviroment;
            this.dB = db;
            this.userManager = userManager;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IEnumerable<EventViewModel>> Get()
        {
            var eventList = await dB.Events.Include(e => e.RSVPS).Include(e => e.CheckIns).Include(e => e.Venue).Include(e => e.Host).OrderBy(e => e.StartTime).ToListAsync();
            var user = await userManager.GetUserAsync(User);
            var models = eventList.Where(e => e != null).Select(e => new EventViewModel(e,user));
            return models;
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<EventViewModel>> GetAll()
        {
            var eventList = await dB.Events.Include(e => e.RSVPS).Include(e => e.CheckIns).Include(e => e.Venue).Include(e => e.Host).OrderBy(e => e.StartTime).ToListAsync();
            var user = await userManager.GetUserAsync(User);
            var models = eventList.Where(e => e != null).Select(e => new EventViewModel(e, user));
            return models;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<EventViewModel> Get(int id)
        {
            var entity = await dB.Events.Include(e => e.Venue).Include(e => e.CheckIns).Include(e => e.RSVPS).Include(e => e.Host).FirstOrDefaultAsync(e => e.Id == id);
            var user = await userManager.GetUserAsync(User);
            if (entity == null)
                return null;
            else
                return new EventViewModel(entity, user);
        }

        [Authorize]
        [HttpGet]
        public async Task<EventViewModel> GetAuth(int id)
        {
            var entity = await dB.Events.Include(e => e.Venue).Include(e => e.CheckIns).Include(e => e.RSVPS).Include(e => e.Host).FirstOrDefaultAsync(e => e.Id == id);
            var user = await userManager.GetUserAsync(User);
            if (entity == null)
                return null;
            else
                return new EventViewModel(entity, user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckIn(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var eventValue = await dB.Events.Include(e => e.CheckIns).FirstOrDefaultAsync(e => e.Id == id);
            if (eventValue == null || user == null)
                return BadRequest();

            var checkIn = eventValue?.CheckIns.FirstOrDefault(c => c.User.Id.Equals(user.Id));
            if(checkIn == null)
            {
                checkIn = new CheckIn();
                checkIn.User = user;
                checkIn.Event = eventValue;
                checkIn.CheckedInTime = DateTime.UtcNow;
                dB.CheckIns.Add(checkIn);

                await dB.SaveChangesAsync();
            }
            return Ok();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RSVP(int id, bool going)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest("You have to be logged in.");
            var eventModel = await dB.Events
                                      .Include(e => e.RSVPS)
                                      .FirstOrDefaultAsync(e => e != null && e.Id == id);
            if(eventModel != null)
            {
                

                if (eventModel.RSVPS != null)
                {
                    var rsvp = eventModel.RSVPS.FirstOrDefault(r => r != null && r.User.Id == user.Id);
                    if (rsvp != null)
                    {
                        rsvp.Going = going;
                        dB.RSVPS.Update(rsvp);
                    }
                    else
                        CreateRSVP(eventModel, user, going);
                }
                else
                    CreateRSVP(eventModel, user, going);
                await dB.SaveChangesAsync();
                return Ok();
                
            }else
                return BadRequest("Could not find event.");
            
        }

        void CreateRSVP(Event model, ApplicationUser user, bool going)
        {

            var rsvp = new RSVP();
            rsvp.Event = model;
            rsvp.Going = going;
            rsvp.User = user;
            dB.RSVPS.Add(rsvp);
        }

        // POST api/values
        [HttpPost]
        public async Task<int> Post([FromBody]CreateEventModel eventModel)
        {
            var user = await userManager.GetUserAsync(User);
            string ImageURL = string.Empty;
            if (ModelState.IsValid && user != null)
            {
                var venue = await dB.Venues.Where(u => eventModel.VenueId == u.Id).FirstAsync();
                var newEvent = new Event(eventModel,user);
                newEvent.ImageUrl = string.IsNullOrEmpty(ImageURL) ? "" : ImageURL;
                newEvent.Venue = venue;
                dB.Events.Add(newEvent);
                var result = await dB.SaveChangesAsync();
                return newEvent.Id;
            }
            return -1;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadImage(int eventID, IFormFile file)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var eventModel = await dB.Events.FirstOrDefaultAsync(e => e.Id == eventID);
                
                if (eventModel != null && user.Id == eventModel.Host.Id)
                {
                    var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    var fileName = "images/" + Guid.NewGuid().ToString() + "_" + file.FileName;
                    var result = await uploader.UploadFile(stream, fileName);
                    if (result)
                    {
                        eventModel.ImageUrl = uploader.GetBaseUrl() + fileName;
                        dB.Events.Update(eventModel);
                        await dB.SaveChangesAsync();
                        return Ok();
                    }else
                        return BadRequest();
                }
            }
            return BadRequest();
        }

        
        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]CreateEventModel modifiedEvent)
        {
            var item = await dB.Events.FirstOrDefaultAsync(i => i.Id == id);
            if(item != null)
            {
                var venue = await dB.Venues.Where(v => v.Id == modifiedEvent.VenueId).FirstOrDefaultAsync();
                var hasAuthorization = await CheckIfHost(item);
                if (hasAuthorization)
                {
                    item.Update(modifiedEvent,venue);
                    dB.Update(item);
                    var result = await dB.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest("You do not have authorization to modify");
            }
            return BadRequest("Event not found");
        }

        async Task<bool> CheckIfHost(Event curentEvent)
        {
            var user = await userManager.GetUserAsync(User);
            return user != null && curentEvent.Host.Id == user.Id;
        }

        // DELETE api/values/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await dB.Events.FirstOrDefaultAsync(i => i.Id == id);
            if(item != null)
            {
                var hasAuthorization = await CheckIfHost(item);
                if(hasAuthorization)
                {
                    dB.Events.Remove(item);
                    var result = await dB.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest("You do not have authorization to delete this event.");
            }
            return NotFound();
        }
    }
}
