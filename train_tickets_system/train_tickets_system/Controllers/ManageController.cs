using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using train_tickets_system.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Data.Entity;

namespace train_tickets_system.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            
            List<Trip> ReservedTrips = new List<Trip>();
            List<Reservation> TripSeatsReservation = new List<Reservation>();
            List<Route> userFilteredRoutes = new List<Route>() ;
            Trip buffTrip = new Trip();
            foreach (Reservation userReservation in db.Reservations.ToList().FindAll(x => x.CustomerID == userId))
            {
                buffTrip = db.Trips.ToList().Find(x => x.TripId == userReservation.TripRefId);
                if (buffTrip.DepartureTime > DateTime.Now)
                { 
                    ReservedTrips.Add(buffTrip);
                    TripSeatsReservation.Add(userReservation);
                    userFilteredRoutes.Add(db.Routes.ToList().Find(x => x.RouteId == buffTrip.RouteRefId));
                }
            }
            
           // var user = db.Users.Find(User.Identity.GetUserId());
          

            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                BookedTrips = ReservedTrips,
                userReservations = TripSeatsReservation,
                promoKilometers = calculatePromo(),
                userRoutes=userFilteredRoutes,
                userTrips=ReservedTrips
            };

            return View(model);
        }

        public ActionResult DeleteReservation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new DeleteViewModel
            {
                userReservation = db.Reservations.Find(id),
                reservationTrip = db.Trips.Find(db.Reservations.Find(id).TripRefId)
            };
           
            if (model.userReservation == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult ChoosePaymentMethod (int id)
        {
            var distanceQuary = from r in db.Routes
                           join t in db.Trips on r.RouteId equals t.RouteRefId
                           join re in db.Reservations on t.TripId equals re.TripRefId
                           where re.ReservationId == id
                           select r.Value;
            var priceQuary = from p in db.Price
                             join r in db.Reservations on p.PriceId equals r.PriceId
                             where r.ReservationId == id
                             select p.Value;
            decimal price = priceQuary.ToList().First();
            int distance = distanceQuary.ToList().First();
            int seats = db.Reservations.ToList().Find(x => x.ReservationId == id).Seats;
            var model = new ChoosePaymentMethodViewModel
            {
                distance = distance * seats,
                money = distance * seats * price,
                resId = id,
                promoKm = calculatePromo()
            };

            return View(model);
        }
        [HttpPost, ActionName("DeleteReservation")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation res = db.Reservations.Find(id);

            if (res.PriceId == 1)
            {
                res.Trip.SeatsTaken.SeatsEconomical -= res.Seats;
            } else
            {
                res.Trip.SeatsTaken.SeatsBusiness -= res.Seats;
            }
            db.Entry(res.Trip.SeatsTaken).State = EntityState.Modified;

            db.Reservations.Remove(res);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public int calculatePromo ()
        {
            var curUser = db.Users.Find(User.Identity.GetUserId());
            int sum=0;
            foreach(var reservation in db.Reservations.ToList().FindAll(x=>x.CustomerID == User.Identity.GetUserId()))
            {
                if (reservation.Trip.DepartureTime < DateTime.Now&& reservation.Trip.DepartureTime>= curUser.lastPromotionCheck&&reservation.Paid==1)
                {
                    sum += reservation.Trip.Route.Value * reservation.Seats;
                }
            }
            
            int accKm = curUser.accumulatedKilometers + sum;
            int freeKm = curUser.freeKilometers;
            do
            {
                if (accKm >= 2000)
                {
                    freeKm += 400;
                    accKm -= 2000;
                }
            }
            while (accKm >= 2000);
            curUser.freeKilometers = freeKm;
            curUser.accumulatedKilometers = accKm;
            curUser.lastPromotionCheck = DateTime.Now;
            db.SaveChanges();
            return curUser.freeKilometers;
        }
        
        public ActionResult SendPaymentConfirmation(int id)
        {
            try
            {
                var model = new SendPaymentConfirmationViewModel
                {
                    paymentConfirmed = db.Reservations.Find(id).Paid != 0
                };

                if (db.Reservations.Find(id).Paid==0)
                {
                    var depcityQuery = from c in db.Cities
                                  join r in db.Routes on c equals r.InitialCity
                                  join t in db.Trips on r.RouteId equals t.RouteRefId
                                  join re in db.Reservations on t.TripId equals re.TripRefId
                                  where re.ReservationId == id
                                  select c.Name;
                    var arrcityQuery = from c in db.Cities
                                  join r in db.Routes on c equals r.TargetCity
                                  join t in db.Trips on r.RouteId equals t.RouteRefId
                                  join re in db.Reservations on t.TripId equals re.TripRefId
                                  where re.ReservationId == id
                                  select c.Name;
                    var timeQuery = from t in db.Trips
                                    join r in db.Reservations on t.TripId equals r.TripRefId
                                    where r.ReservationId == id
                                    select t.DepartureTime;
                    var distanceQuery = from r in db.Routes
                                   join t in db.Trips on r.RouteId equals t.RouteRefId
                                   join re in db.Reservations on t.TripId equals re.TripRefId
                                   where re.ReservationId == id
                                   select r.Value;
                    var priceQuery = from p in db.Price
                                     join r in db.Reservations on p.PriceId equals r.PriceId
                                     where r.ReservationId == id
                                     select p.Value;
                    decimal price = priceQuery.ToList().First();
                    int distance = distanceQuery.ToList().First();
                    DateTime depTime = timeQuery.ToList().First();
                    string arrCity= arrcityQuery.ToList().First();
                    string depCity = depcityQuery.ToList().First();
                    int seats = db.Reservations.ToList().Find(x => x.ReservationId == id).Seats;
                    var smtpClient = new SmtpClient();
                    var msg = new MailMessage();
                    msg.To.Add(db.Users.Find(User.Identity.GetUserId()).Email);
                    msg.Subject = "Ticket purchase";
                    msg.Body = "Hello " + db.Users.Find(User.Identity.GetUserId()).FirstName + ",<br>You have purchased the following ticket:<br><b>Ticket ID:</b>" + id +
                        "<br><b>Seats:</b> " + seats.ToString() +
                        "<br><b>Departure City:</b> " + depCity+
                        "<br><b>Arrival City:</b> " + arrCity +
                        "<br><b>Departure time:</b> " + depTime.ToString() +
                        "<br>You have paid <b>&euro;" + distance * price * seats  + "</b> for your ticket.";
                    msg.IsBodyHtml = true;
                    
                    smtpClient.Send(msg);
                    db.Reservations.ToList().Find(x => x.ReservationId == id).Paid = 1;
                    db.SaveChanges();
                }

                return View(model);
            }

            catch (Exception e)
            {
                return View("NoSuchTicketView");
            }
        }
        public ActionResult SendPaymentConfirmationPromo(int id)
        {
            try
            {
                var model = new SendPaymentConfirmationViewModel
                {
                    paymentConfirmed = db.Reservations.Find(id).Paid != 0
                };

                if (db.Reservations.Find(id).Paid == 0)
                {
                    var depcityQuery = from c in db.Cities
                                       join r in db.Routes on c equals r.InitialCity
                                       join t in db.Trips on r.RouteId equals t.RouteRefId
                                       join re in db.Reservations on t.TripId equals re.TripRefId
                                       where re.ReservationId == id
                                       select c.Name;
                    var arrcityQuery = from c in db.Cities
                                       join r in db.Routes on c equals r.TargetCity
                                       join t in db.Trips on r.RouteId equals t.RouteRefId
                                       join re in db.Reservations on t.TripId equals re.TripRefId
                                       where re.ReservationId == id
                                       select c.Name;
                    var timeQuery = from t in db.Trips
                                    join r in db.Reservations on t.TripId equals r.TripRefId
                                    where r.ReservationId == id
                                    select t.DepartureTime;
                    var distanceQuery = from r in db.Routes
                                        join t in db.Trips on r.RouteId equals t.RouteRefId
                                        join re in db.Reservations on t.TripId equals re.TripRefId
                                        where re.ReservationId == id
                                        select r.Value;
                    var priceQuery = from p in db.Price
                                     join r in db.Reservations on p.PriceId equals r.PriceId
                                     where r.ReservationId == id
                                     select p.Value;
                    decimal price = priceQuery.ToList().First();
                    int distance = distanceQuery.ToList().First();
                    DateTime depTime = timeQuery.ToList().First();
                    string arrCity = arrcityQuery.ToList().First();
                    string depCity = depcityQuery.ToList().First();
                    int seats = db.Reservations.ToList().Find(x => x.ReservationId == id).Seats;
                    var smtpClient = new SmtpClient();
                    var msg = new MailMessage();
                    msg.To.Add(db.Users.Find(User.Identity.GetUserId()).Email);
                    msg.Subject = "Ticket purchase";
                    msg.Body = "Hello " + db.Users.Find(User.Identity.GetUserId()).FirstName + ",<br>You have purchased the following ticket:<br><b>Ticket ID:</b>" + id +
                        "<br><b>Seats:</b> " + seats.ToString() +
                        "<br><b>Departure City:</b> " + depCity +
                        "<br><b>Arrival City:</b> " + arrCity +
                        "<br><b>Departure time:</b> " + depTime.ToString() +
                        "<br>You used <b>" + distance * seats + "</b> of your free kilometers for your ticket.";
                    msg.IsBodyHtml = true;

                    smtpClient.Send(msg);
                    db.Reservations.ToList().Find(x => x.ReservationId == id).Paid = 2;
                    db.Users.Find(User.Identity.GetUserId()).freeKilometers -= distance * seats;
                    db.SaveChanges();
                }

                return View(model);
            }

            catch (Exception e)
            {
                return View("NoSuchTicketView");
            }
        }
        public ActionResult SendConfirmation (int id)
        {
            try
            {
                var model = new SendConfirmationViewModel
                {
                    Confirmed = db.Reservations.Find(id).Confirmed
                };
                if (!db.Reservations.Find(id).Confirmed)
                {
                    var confirmToken = UserManager.GenerateUserToken("reservation confirmation", User.Identity.GetUserId());
                    confirmToken = HttpUtility.HtmlEncode(confirmToken.Replace("+", ""));
                    var smtpClient = new SmtpClient();
                    var msg = new MailMessage();
                    msg.To.Add(db.Users.Find(User.Identity.GetUserId()).Email);
                    msg.Subject = "Reservation confirmation email";
                    var depcityQuery = from c in db.Cities
                                       join r in db.Routes on c equals r.InitialCity
                                       join t in db.Trips on r.RouteId equals t.RouteRefId
                                       join re in db.Reservations on t.TripId equals re.TripRefId
                                       where re.ReservationId == id
                                       select c.Name;
                    var arrcityQuery = from c in db.Cities
                                       join r in db.Routes on c equals r.TargetCity
                                       join t in db.Trips on r.RouteId equals t.RouteRefId
                                       join re in db.Reservations on t.TripId equals re.TripRefId
                                       where re.ReservationId == id
                                       select c.Name;
                    var timeQuery = from t in db.Trips
                                    join r in db.Reservations on t.TripId equals r.TripRefId
                                    where r.ReservationId == id
                                    select t.DepartureTime;
                   
                 
                    DateTime depTime = timeQuery.ToList().First();
                    string arrCity = arrcityQuery.ToList().First();
                    string depCity = depcityQuery.ToList().First();
                    var fullUrl = this.Url.Action("CheckConfirmation", "Manage", new { token = confirmToken }, this.Request.Url.Scheme);
                    msg.Body = "Hello " + db.Users.Find(User.Identity.GetUserId()).FirstName + ",<br>You have reserved the following ticket:<br><b>Ticket ID:</b>" + id +
                        "<br><b>Seats:</b> " + db.Reservations.ToList().Find(x => x.ReservationId == id).Seats.ToString() +
                        "<br><b>Departure City:</b> " + depCity +
                        "<br><b>Arrival City:</b> " + arrCity +
                        "<br><b>Departure time:</b> " + depTime.ToString() +
                        "<br>Click on the link to confirm your ticket.<br> <a href=" + fullUrl + ">Confirm</a>";
                    msg.IsBodyHtml = true;
                    smtpClient.Send(msg);

                    var myClaim = new Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim();
                    myClaim.ClaimType = id.ToString();
                    myClaim.ClaimValue = confirmToken;
                    myClaim.UserId = User.Identity.GetUserId();
                    db.Users.Find(User.Identity.GetUserId()).Claims.Add(myClaim);
                    db.SaveChanges();
                }

                return View(model);
            } catch (Exception e)
            {
                return View("NoSuchTicketView");
            }
        }

        //
        public ActionResult CheckConfirmation (string token)
        {
            token = token.Replace(" ", "");
            var model = new CheckConfirmationViewModel();
            var tokenCheck = db.Users.Find(User.Identity.GetUserId()).Claims.Last();
            if (tokenCheck.ClaimValue.Equals(token))
            {
                model.Success = true;
                var currUser = db.Reservations.ToList().Find(x=> x.ReservationId == Int32.Parse(tokenCheck.ClaimType));
                currUser.Confirmed = true;
                db.SaveChanges();
            }

            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}