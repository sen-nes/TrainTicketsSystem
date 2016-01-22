using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace train_tickets_system.ViewModels
{
    public class SearchViewModel
    {
        [Display(Name = "From")]
        [Required(ErrorMessage = "Please select a depature city!")]
        public int? InitialCityId { get; set; }

        [Display(Name = "To")]
        [Required(ErrorMessage = "Please select an arrival city!")]
        public int? TargetCityId { get; set; }

        public virtual IEnumerable<SelectListItem> Cities { get; set; }

        [Display(Name = "Departure")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(Validator), "ValidateBookingDate")]
        public DateTime DepartureDate { get; set; }
        
        [DataType(DataType.Time)]
        public int Hour { get; set; }

        public virtual IEnumerable<SelectListItem> Hours { get; set; }
    }

    public class Validator
    {
        public static ValidationResult ValidateBookingDate(DateTime validationDate)
        {

            if (validationDate >= DateTime.Today && validationDate <= DateTime.Today.AddDays(14))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("You can only book trips for the next 14 days.");
        }
    }
}