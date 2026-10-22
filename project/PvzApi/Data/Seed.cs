using PvzApi.Entities;
using PvzApi.Security;

namespace PvzApi.Data;

public static class Seed
{
    private static readonly string[] Names =
    [
        "Алия", "Данияр", "Мария", "Сергей", "Асель", "Тимур", "Елена", "Нурлан",
    ];

    public static void Run(AppDbContext db)
    {
        if (db.Users.Count() == 0)
        {
            db.Users.AddRange(
                new User { Login = "admin", PasswordHash = Passwords.Hash("Admin123"), Role = "Admin" },
                new User { Login = "operator", PasswordHash = Passwords.Hash("Operator123"), Role = "Operator" });
            db.SaveChanges();
        }

        if (db.Clients.Count() < 20)
        {
            var rnd = new Random(42);
            for (var i = 0; i < 17; i++)
                db.Clients.Add(new Client
                {
                    FullName = $"{Names[i % Names.Length]} Клиент{i + 4}",
                    Phone = $"+7702{rnd.Next(1000000, 9999999)}",
                });
            db.SaveChanges();
        }

        if (db.Parcels.Count() == 0)
        {
            var rnd = new Random(7);
            var clients = db.Clients.Select(c => c.Id).ToList();
            var points = db.PickupPoints.Select(p => p.Id).ToList();

            for (var i = 1; i <= 100; i++)
            {
                var arrived = DateTime.UtcNow.AddDays(-rnd.Next(0, 14));
                db.Parcels.Add(new Parcel
                {
                    TrackNumber = $"PVZ-{100000 + i}",
                    Weight = System.Math.Round((decimal)(rnd.NextDouble() * 20 + 0.1), 3),
                    Price = rnd.Next(1000, 90000),
                    StorageDays = rnd.Next(3, 15),
                    ArrivedAt = arrived,
                    Status = i % 7 == 0 ? ParcelStatus.Issued : ParcelStatus.InStock,
                    IssuedAt = i % 7 == 0 ? arrived.AddDays(1) : null,
                    ClientId = clients[rnd.Next(clients.Count)],
                    PickupPointId = points[rnd.Next(points.Count)],
                });
            }

            db.SaveChanges();
        }
    }
}
