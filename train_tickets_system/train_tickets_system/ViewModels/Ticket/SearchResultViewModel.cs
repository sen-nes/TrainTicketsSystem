using System.Collections.Generic;
using train_tickets_system.Models;

namespace train_tickets_system.ViewModels
{
    public class SearchResultViewModel
    {
        public virtual Trip Trip { get; set; }

        public int FreeEconomySeats { get; set; }

        public int FreeBusinessSeats { get; set; }
    }
}