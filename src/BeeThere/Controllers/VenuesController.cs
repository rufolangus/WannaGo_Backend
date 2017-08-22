using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WannaGo.Models;
using WannaGo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WannaGo.Controllers
{
    [Route("api/[controller]/[action]")]
    public class VenuesController : Controller
    {

        private ApplicationDbContext dB;
        private UserManager<ApplicationUser> userManager;

        public VenuesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.dB = db;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IEnumerable<Venue>> Get()
        {
            var venues = await dB.Venues.ToListAsync();
            return venues;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<Venue> Get(int id)
        {
            var venue = await dB.Venues.FirstOrDefaultAsync(v => v.Id == id);
            return venue;
        }

        [Authorize]
        [HttpPost]
        public async Task<Venue> Post([FromBody] CreateVenuModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                var newVenue = new Venue
                {
                    Name = model.Name,
                    Lat = model.Lat,
                    Lon = model.Lon,
                };
                dB.Venues.Add(newVenue);
                var result = await dB.SaveChangesAsync();
                return newVenue;
            }

            return null;
        }
        

    }
}
