using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace train_tickets_system.Models
{
    public  class Train
    {
        // Fields
        public int TrainId { get; set; }
        public int businessSeats { get; set; }
        public int econimicSeats { get; set; }
        public int businessSeatsTaken { get; set; }
        public int econimicSeatsTaken { get; set; }
        public float AverageSpeed { get; set; }
        public virtual List<Trip> Trips { get; set; }

        // Constructors
        public Train()
        {
        }

        public Train(int id, int bseats, int eseats, float avgs)
        {
            TrainId = id;
            businessSeats = bseats;
            econimicSeats = eseats;
            AverageSpeed = avgs;
        }
    }
}
