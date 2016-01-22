namespace train_tickets_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        CityId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CityId);
            
            CreateTable(
                "dbo.Prices",
                c => new
                    {
                        PriceId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.PriceId);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        ReservationId = c.Int(nullable: false, identity: true),
                        CustomerID = c.String(maxLength: 128),
                        PriceId = c.Int(nullable: false),
                        Seats = c.Int(nullable: false),
                        Confirmed = c.Boolean(nullable: false),
                        TripRefId = c.Int(nullable: false),
                        Paid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReservationId)
                .ForeignKey("dbo.Prices", t => t.PriceId, cascadeDelete: true)
                .ForeignKey("dbo.Trips", t => t.TripRefId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerID)
                .Index(t => t.CustomerID)
                .Index(t => t.PriceId)
                .Index(t => t.TripRefId);
            
            CreateTable(
                "dbo.Trips",
                c => new
                    {
                        TripId = c.Int(nullable: false, identity: true),
                        TrainRefId = c.Int(nullable: false),
                        RouteRefId = c.Int(nullable: false),
                        DepartureTime = c.DateTime(nullable: false),
                        ArrivalTime = c.DateTime(nullable: false),
                        SeatsTakenId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TripId)
                .ForeignKey("dbo.Routes", t => t.RouteRefId, cascadeDelete: true)
                .ForeignKey("dbo.Trains", t => t.TrainRefId, cascadeDelete: true)
                .Index(t => t.TrainRefId)
                .Index(t => t.RouteRefId);
            
            CreateTable(
                "dbo.Routes",
                c => new
                    {
                        RouteId = c.Int(nullable: false, identity: true),
                        Value = c.Int(nullable: false),
                        InitialCity_CityId = c.Int(),
                        TargetCity_CityId = c.Int(),
                    })
                .PrimaryKey(t => t.RouteId)
                .ForeignKey("dbo.Cities", t => t.InitialCity_CityId)
                .ForeignKey("dbo.Cities", t => t.TargetCity_CityId)
                .Index(t => t.InitialCity_CityId)
                .Index(t => t.TargetCity_CityId);
            
            CreateTable(
                "dbo.SeatsTakens",
                c => new
                    {
                        TrainId = c.Int(nullable: false),
                        SeatsEconomical = c.Int(nullable: false),
                        SeatsBusiness = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TrainId)
                .ForeignKey("dbo.Trips", t => t.TrainId)
                .Index(t => t.TrainId);
            
            CreateTable(
                "dbo.Trains",
                c => new
                    {
                        TrainId = c.Int(nullable: false, identity: true),
                        businessSeats = c.Int(nullable: false),
                        econimicSeats = c.Int(nullable: false),
                        businessSeatsTaken = c.Int(nullable: false),
                        econimicSeatsTaken = c.Int(nullable: false),
                        AverageSpeed = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.TrainId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        freeKilometers = c.Int(nullable: false),
                        accumulatedKilometers = c.Int(nullable: false),
                        lastPromotionCheck = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reservations", "CustomerID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Trips", "TrainRefId", "dbo.Trains");
            DropForeignKey("dbo.SeatsTakens", "TrainId", "dbo.Trips");
            DropForeignKey("dbo.Trips", "RouteRefId", "dbo.Routes");
            DropForeignKey("dbo.Routes", "TargetCity_CityId", "dbo.Cities");
            DropForeignKey("dbo.Routes", "InitialCity_CityId", "dbo.Cities");
            DropForeignKey("dbo.Reservations", "TripRefId", "dbo.Trips");
            DropForeignKey("dbo.Reservations", "PriceId", "dbo.Prices");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.SeatsTakens", new[] { "TrainId" });
            DropIndex("dbo.Routes", new[] { "TargetCity_CityId" });
            DropIndex("dbo.Routes", new[] { "InitialCity_CityId" });
            DropIndex("dbo.Trips", new[] { "RouteRefId" });
            DropIndex("dbo.Trips", new[] { "TrainRefId" });
            DropIndex("dbo.Reservations", new[] { "TripRefId" });
            DropIndex("dbo.Reservations", new[] { "PriceId" });
            DropIndex("dbo.Reservations", new[] { "CustomerID" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Trains");
            DropTable("dbo.SeatsTakens");
            DropTable("dbo.Routes");
            DropTable("dbo.Trips");
            DropTable("dbo.Reservations");
            DropTable("dbo.Prices");
            DropTable("dbo.Cities");
        }
    }
}
