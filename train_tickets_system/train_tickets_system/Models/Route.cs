using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace train_tickets_system.Models
{
    public  class Route
    {
        // Fields
        [Key]
        public int RouteId { get; set; }

        [Display(Name = "From")]
        public virtual City InitialCity { get; set; }

        [Display(Name = "To")]
        public virtual City TargetCity { get; set; }

        [Display(Name = "Distance")]
        public int Value { get; set; }//kilometers

        public virtual List<Trip> Trips { get; set; }

        // Constructors
        public Route()
        {
        }

        public Route(int id, City icity, City tcity, int value)
        {
            RouteId = id;
            InitialCity = icity;
            TargetCity = tcity;
            this.Value = value;
        }
        
    }
}
