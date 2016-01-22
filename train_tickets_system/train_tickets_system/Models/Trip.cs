using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace train_tickets_system.Models
{
    public class Trip
    {
        // Fields
        [Display(Name = "Trip")]
        public int TripId { get; set; }

        public int TrainRefId { get; set; }

        [ForeignKey("TrainRefId")]
        public virtual Train Train { get; set; }

        public int RouteRefId { get; set; }

        [ForeignKey("RouteRefId")]
        public virtual Route Route { get; set; }

        [Display(Name = "Depart")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Arrive")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Seats taken")]
        public int SeatsTakenId { get; set; }

        [ForeignKey("SeatsTakenId")]
        public virtual SeatsTaken SeatsTaken { get; set; }

        public virtual List<Reservation> Reservations { get; set; }

        // Constructors
        public Trip()
        {
        }

        public Trip(int id, int trainid, int routeid, DateTime departuretime, DateTime arrivaltime)
        {
            TripId = id;
            TrainRefId = trainid;
            RouteRefId = routeid;
            this.DepartureTime = departuretime;
            this.ArrivalTime = arrivaltime;
        }

    }
}
