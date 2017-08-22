using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WannaGo.Models
{
    public class CheckIn
    {
        public int Id { get; set; }
        public virtual Event Event { get; set;}
        public virtual ApplicationUser User { get; set; }
        public DateTime CheckedInTime { get; set; }
    }
}
