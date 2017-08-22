using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WannaGo.Utility;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WannaGo.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventType Type { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime LastEditDate { get; set; }
        public bool IsPrivate { get; set; }
        public float Price { get; set; }
        public virtual ICollection<RSVP> RSVPS { get; set; }
        public virtual Venue Venue { get; set; }
        public virtual ICollection<CheckIn> CheckIns { get; set; }

        public virtual ApplicationUser Host { get; set; }
        public Event(CreateEventModel model = null, ApplicationUser user = null)
        {
            this.ImageUrl = model.ImageURL;
            this.Name = model.Name;
            this.Type = model.Type;
            this.Description = model.Description;
            this.StartTime = model.StartTime;
            this.EndTime = model.EndTime;
            this.IsPrivate = model.IsPrivate;
            this.Price = model.Price;
            Host = user;
            CreatedDate = DateTime.UtcNow;
        }
        public Event()
        {

        }

       

        public void Update(CreateEventModel model, Venue venue = null)
        {
            if(model != null)
               Binder.Bind(this, model);
            LastEditDate = DateTime.UtcNow;
            if (venue != null)
                Venue = venue;
            
        }
    }

    public class CreateEventModel
    {
        public string Name { get; set; }
        public int VenueId { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public EventType Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float Price { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Venue Venue { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public EventType Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float Price { get; set; }
        public bool IsPrivate { get; set; }
        public string HostID { get; set; }
        public int Going { get; set; }
        public int NotGoing { get; set; }
        public int Total { get; set; }
        public bool RSVPED { get; set; }
        public bool GoinOrNOt { get; set; }
        public string HostImage { get; set; }
        public bool CheckIn { get; set; }
        public int CheckIns { get; set; }

        public EventViewModel(Event entity, ApplicationUser user = null)
        {
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Venue = entity.Venue;
            this.Type = entity.Type;
            this.Description = entity.Description;
            this.ImageURL = entity.ImageUrl;
            this.StartTime = entity.StartTime;
            this.EndTime = entity.EndTime;
            this.Price = entity.Price;
            this.IsPrivate = entity.IsPrivate;
            this.HostID = entity.Host?.Id;
            this.HostImage = entity?.Host.Image;
            this.Going = entity.RSVPS != null ? entity.RSVPS.Where(r => r != null && r.Going).Count() : 0;
            this.NotGoing = entity.RSVPS != null ? entity.RSVPS.Where(r => r != null && !r.Going).Count() : 0;
            this.Total = entity.RSVPS != null ? entity.RSVPS.Count() : 0;
            this.CheckIn = user == null ? false : entity.CheckIns?.FirstOrDefault(c => c.User.Id.Equals(user.Id)) != null;
            this.CheckIns = entity.CheckIns == null ? 0 : entity.CheckIns.Count();
            if(user != null)
            {
                var rsvp = entity.RSVPS.FirstOrDefault(e => e != null && e.User.Id == user.Id);
                this.RSVPED = rsvp != null;
                this.GoinOrNOt = rsvp != null && rsvp.Going;
            }
            
        }
    }


    public enum EventType
    {
        Other, Party, Sale, HappyHour,
        Concert, BonFire, Festival, Rave,
        Celebration, Graduation, Conference, Campfire,
        Cookout, BBQ, BeachParty, PoolParty,
        Birthday, Aniversary, Demo, Gallery,
        WineTasting, Contest, Hackathon, GameJam,
        Recital, Play, ComedyNight, StandupComedy,
        MovieNight, Show, Gathering, 
    }


}
