using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using train_tickets_system.Models;
using train_tickets_system.ViewModels;

namespace train_tickets_system.Controllers
{
    static class Setter
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        private static SelectList Cities { get; set; }
        public static SelectList getCities()
        {
            var citiesQuery = from d in db.Cities
                              select d;
            var cities = new Dictionary<int, string>();
            foreach (var city in citiesQuery)
            {
                cities.Add(city.CityId, city.Name);
            }

            Cities = new SelectList(cities, "Key", "Value");

            return Cities;
        }

        private static SelectList Hours { get; set; }
        public static SelectList getHours()
        {
            var hours = new Dictionary<int, string>()
                {
                    { 0, "00:00" },
                    { 1, "01:00" },
                    { 2, "02:00" },
                    { 3, "03:00" },
                    { 4, "04:00" },
                    { 5, "05:00" },
                    { 6, "06:00" },
                    { 7, "07:00" },
                    { 8, "08:00" },
                    { 9, "09:00" },
                    { 10, "10:00" },
                    { 11, "11:00" },
                    { 12, "12:00" },
                    { 13, "13:00" },
                    { 14, "14:00" },
                    { 15, "15:00" },
                    { 16, "16:00" },
                    { 17, "17:00" },
                    { 18, "18:00" },
                    { 19, "19:00" },
                    { 20, "20:00" },
                    { 21, "21:00" },
                    { 22, "22:00" },
                    { 23, "23:00" }
                };

            Hours = new SelectList(hours, "Key", "Value");

            return Hours;
        }

        private static SelectList Seats { get; set; }
        public static SelectList getSeats()
        {
            var seats = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Seats = new SelectList(seats);

            return Seats;
        }
    }

    public class TicketController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // INDEX
        public ActionResult Index()
        {
            return RedirectToAction("Search");
        }

        // SEARCH
        public ActionResult Search()
        {
            var viewModel = new SearchViewModel
            {
                Cities = Setter.getCities(),
                Hours = Setter.getHours(),
                DepartureDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.InitialCityId != viewModel.TargetCityId)
                {
                    DateTime departure = viewModel.DepartureDate.AddHours(viewModel.Hour);
                    DateTime departuredDay = departure.Date.AddDays(1);
                    DateTime departureConstraint = DateTime.Today.AddDays(13);
                    DateTime now = DateTime.Now;

                    var trips = from d in db.Trips
                                select d;

                    trips = trips.Where(r => r.Route.InitialCity.CityId == viewModel.InitialCityId)
                                            .Where(t => t.Route.TargetCity.CityId == viewModel.TargetCityId)
                                            .Where(r => r.DepartureTime >= departure)
                                            .Where(r => r.DepartureTime >= now)
                                            .Where(r => r.DepartureTime < departuredDay)
                                            .Where(r => r.DepartureTime < departureConstraint)
                                            .OrderBy(d => d.DepartureTime)
                                            .Take(3);

                    var models = new List<SearchResultViewModel>();
                    foreach (var trip in trips.ToList())
                    {
                        int freeEconomySeats;
                        int freeBusinessSeats;
                        if (trip.SeatsTaken == null)
                        {
                            freeEconomySeats = trip.Train.econimicSeats;
                            freeBusinessSeats = trip.Train.businessSeats;
                        }
                        else
                        {
                            freeEconomySeats = trip.Train.econimicSeats - trip.SeatsTaken.SeatsEconomical;
                            freeBusinessSeats = trip.Train.businessSeats - trip.SeatsTaken.SeatsBusiness;
                        }

                        var model = new SearchResultViewModel
                        {
                            Trip = trip,
                            FreeEconomySeats = freeEconomySeats,
                            FreeBusinessSeats = freeBusinessSeats
                        };

                        models.Add(model);
                    }

                    return View("SearchResult", models);
                }
                else
                {
                    ModelState.AddModelError("", "Departure and arrival city should be different!");
                }
            }

            viewModel.Cities = Setter.getCities();
            viewModel.Hours = Setter.getHours();
            viewModel.DepartureDate = DateTime.Today;

            return View(viewModel);
        }

        // BOOK
        [Authorize]
        public ActionResult Book(int id)
        {
            Trip tmp = db.Trips.Find(id);
            decimal priceEconomyPerKm = db.Price.Find(1).Value;
            decimal priceBusinessPerKm = db.Price.Find(2).Value;

            BookViewModel viewModel = new BookViewModel
            {
                Trip = tmp,
                Seats = Setter.getSeats(),
                priceEconomy = (decimal)tmp.Route.Value * priceEconomyPerKm,
                priceBusiness = (decimal)tmp.Route.Value * priceBusinessPerKm,
                travelingClass = true
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public  ActionResult Book(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Reservation ticket = new Reservation();

                ticket.TripRefId = viewModel.Trip.TripId;
                ticket.Trip = db.Trips.Find(viewModel.Trip.TripId);
                if (ticket.Trip.SeatsTaken == null)
                {
                    ticket.Trip.SeatsTakenId = ticket.TripRefId;
                    ticket.Trip.SeatsTaken = new SeatsTaken
                    {
                        TrainId = ticket.TripRefId
                    };

                    if (viewModel.travelingClass)
                    {
                        ticket.PriceId = 1;
                        ticket.Trip.SeatsTaken.SeatsEconomical += viewModel.numberSeats;
                    }
                    else
                    {
                        ticket.PriceId = 2;
                        ticket.Trip.SeatsTaken.SeatsBusiness += viewModel.numberSeats;
                    }

                    db.Entry(ticket.Trip).State = EntityState.Modified;
                    db.SeatsTaken.Add(ticket.Trip.SeatsTaken);
                }
                else
                {

                    if (viewModel.travelingClass)
                    {
                        if (ticket.Trip.SeatsTaken.SeatsEconomical + viewModel.numberSeats <= ticket.Trip.Train.econimicSeats)
                        {
                            ticket.PriceId = 1;
                            ticket.Trip.SeatsTaken.SeatsEconomical += viewModel.numberSeats;

                            db.Entry(ticket.Trip.SeatsTaken).State = EntityState.Modified;
                        } else
                        {
                            viewModel.Seats = Setter.getSeats();
                            viewModel.Trip = ticket.Trip;
                            ModelState.AddModelError("", "Not enough free seats in economy class!");
                            return View(viewModel);
                        }
                    }
                    else
                    {
                        if (ticket.Trip.SeatsTaken.SeatsBusiness + viewModel.numberSeats <= ticket.Trip.Train.businessSeats)
                        {
                            ticket.PriceId = 2;
                            ticket.Trip.SeatsTaken.SeatsBusiness += viewModel.numberSeats;

                            db.Entry(ticket.Trip.SeatsTaken).State = EntityState.Modified;
                        }
                        else
                        {
                            viewModel.Seats = Setter.getSeats();
                            viewModel.Trip = ticket.Trip;
                            ModelState.AddModelError("", "Not enough free seats in business class!");
                            return View(viewModel);
                        }
                    }
                }

                ticket.Seats = viewModel.numberSeats;
                ticket.CustomerID = User.Identity.GetUserId();
                db.Reservations.Add(ticket);
                db.SaveChanges();
                
                return RedirectToRoute("Default", new { controller = "Manage", action = "SendConfirmation", id = ticket.ReservationId } );
            }

            viewModel.Seats = Setter.getSeats();
            return View(viewModel);
        }
    }
}
