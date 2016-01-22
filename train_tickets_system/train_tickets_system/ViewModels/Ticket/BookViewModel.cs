using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using train_tickets_system.Models;

namespace train_tickets_system.ViewModels
{
    public class BookViewModel
    {
        public virtual Trip Trip { get; set; }

        [Display(Name = "Class")]
        public bool travelingClass { get; set; }

        [Display(Name = "Seats")]
        public int numberSeats { get; set; }
        public virtual IEnumerable<SelectListItem> Seats { get; set; }

        public decimal priceEconomy { get; set; }

        public decimal priceBusiness { get; set; }
    }
}