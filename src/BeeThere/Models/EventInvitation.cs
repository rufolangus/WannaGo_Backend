using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WannaGo.Models
{
    public class EventInvitation
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public virtual ApplicationUser To { get; set; }
        public virtual ApplicationUser From { get; set; }
        public virtual Event Event { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Responded { get; set; }
    }
}
