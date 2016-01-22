using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace train_tickets_system.Models
{
    public class Reservation
    {
        // Fields
        public int ReservationId { get; set; }

        public string CustomerID { get; set; }

        [ForeignKey("CustomerID")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Price per ticket")]
        public int PriceId { get; set; }

        [ForeignKey("PriceId")]
        public virtual Price Price { get; set; }

        public int Seats { get; set; }

        public bool Confirmed { get; set; } = false;

        public int TripRefId { get; set; }

        [ForeignKey("TripRefId")]
        public virtual Trip Trip { get; set; }

        public int Paid { get; set; }

        // Constructors
        public Reservation()
        {
        }
    }
}
