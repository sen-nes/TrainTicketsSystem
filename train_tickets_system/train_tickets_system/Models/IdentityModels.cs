using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System;

namespace train_tickets_system.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here

           
            return userIdentity;
        }
        
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int freeKilometers { get; set; }
        public int accumulatedKilometers { get; set; }
        public DateTime lastPromotionCheck { get; set; } = DateTime.Now;
        public List<Reservation> Reservation { get; set; }
    } 
    

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public System.Data.Entity.DbSet<train_tickets_system.Models.Trip> Trips { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.Train> Trains { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.Reservation> Reservations { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.Price> Price { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.Route> Routes { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.City> Cities { get; set; }
        public System.Data.Entity.DbSet<train_tickets_system.Models.SeatsTaken> SeatsTaken { get; set; }

        //internal object Entry(object reservations)
        //{
        //    throw new NotImplementedException();
        //}
        /*protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
   base.OnModelCreating(modelBuilder);
   modelBuilder.Entity<IdentityUser>().ToTable("Users").HasKey(p=>p.Id);
   modelBuilder.Entity<ApplicationUser>().ToTable("Users").HasKey(p => p.Id);
   modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles").HasKey(p => p.RoleId);
   modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins").HasKey(p => p.UserId);
   modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
   modelBuilder.Entity<IdentityRole>().ToTable("Roles");
}*/
    }
}