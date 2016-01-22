using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace train_tickets_system.Models
{
    public class SeatsTaken
    {
        [Key, ForeignKey("Trip")]
        public int TrainId { get; set; }

        public virtual Trip Trip { get; set; }

        // Should rename to Economy
        public int SeatsEconomical { get; set; } 

        public int SeatsBusiness { get; set; }
    }
}
