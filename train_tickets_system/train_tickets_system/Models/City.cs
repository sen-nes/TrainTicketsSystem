using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace train_tickets_system.Models
{
    public class City
    {
        // Fields
        public int CityId { get; set; }
        public string Name { get; set; }

        // Constructors
        public City()
        {
        }

        public City(int id, string name)
        {
            CityId = id;
            this.Name = name;
        }
    }
}
