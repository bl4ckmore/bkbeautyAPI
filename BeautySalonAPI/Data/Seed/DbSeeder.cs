using BeautySalonAPI.Models;

namespace BeautySalonAPI.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Services.Any())
        {
            var services = new List<Service>
            {
                new() { Category = "Hair",          Name = "Balayage & Toning",    Description = "Sun-kissed colour blended seamlessly through your lengths for a natural, lived-in result.",         Duration = "3h",    Price = 180, Icon = "◈", Popular = true  },
                new() { Category = "Hair",          Name = "Keratin Smoothing",     Description = "Transform frizzy hair into sleek perfection with a treatment that lasts up to four months.",       Duration = "2h30",  Price = 140, Icon = "◆", Popular = false },
                new() { Category = "Hair",          Name = "Precision Cut & Style", Description = "An expert cut tailored to your face shape and lifestyle, finished with a flawless blowout.",        Duration = "75min", Price = 65,  Icon = "✂", Popular = false },
                new() { Category = "Nails",         Name = "Classic Manicure",      Description = "Shaping, cuticle care, a hand massage, and your choice of polish from our curated palette.",        Duration = "45min", Price = 35,  Icon = "◇", Popular = false },
                new() { Category = "Nails",         Name = "Gel Extension Set",     Description = "Full sculpted gel extensions with any design — from barely-there minimalism to elaborate art.",     Duration = "2h",    Price = 85,  Icon = "◉", Popular = true  },
                new() { Category = "Nails",         Name = "Luxury Pedicure",       Description = "Deep soak, exfoliation, callus removal, leg massage, and a flawless colour finish.",                Duration = "60min", Price = 50,  Icon = "❋", Popular = false },
                new() { Category = "Skin",          Name = "Deep Hydration Facial", Description = "Intensive moisture treatment featuring hyaluronic acid serums and a cooling jade massage.",         Duration = "60min", Price = 95,  Icon = "✦", Popular = true  },
                new() { Category = "Skin",          Name = "Microdermabrasion",     Description = "Crystal exfoliation that resurfaces the skin and reveals a dramatically more radiant complexion.",   Duration = "75min", Price = 110, Icon = "◌", Popular = false },
                new() { Category = "Brows & Lashes",Name = "Lash Lift & Tint",      Description = "A semi-permanent curl and darkening treatment that opens the eye for up to eight weeks.",           Duration = "60min", Price = 70,  Icon = "⌁", Popular = true  },
                new() { Category = "Brows & Lashes",Name = "Brow Lamination",       Description = "Brush up, sculpt, and set your brows into their most full-looking, architectural form.",            Duration = "50min", Price = 60,  Icon = "⊹", Popular = false },
                new() { Category = "Body",          Name = "Aromatherapy Massage",  Description = "Full-body relaxation with warm bespoke essential oil blends for deep de-stressing.",                Duration = "90min", Price = 120, Icon = "✿", Popular = false },
                new() { Category = "Body",          Name = "Body Wrap & Scrub",     Description = "Sugar scrub exfoliation followed by a nourishing wrap to soften and illuminate the skin.",          Duration = "90min", Price = 105, Icon = "⊛", Popular = false },
            };
            db.Services.AddRange(services);
            await db.SaveChangesAsync();
        }

        if (!db.Users.Any(u => u.Role == "admin"))
        {
            db.Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@lumiere.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "admin",
            });
            await db.SaveChangesAsync();
        }
    }
}
