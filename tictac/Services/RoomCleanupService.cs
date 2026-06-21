using Microsoft.EntityFrameworkCore;
using tictac.Data;

namespace tictac.Services;

public class RoomCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RoomCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cutoffTime = DateTime.UtcNow.AddMinutes(-5);

                var abandonedRooms = await db.Rooms
                    .Where(r => string.IsNullOrEmpty(r.CreatorConnectionId)
                             && string.IsNullOrEmpty(r.GuestConnectionId)
                             && r.LastUpdatedAt <= cutoffTime
                             && !r.IsDeleted)
                    .ToListAsync(stoppingToken);

                if (abandonedRooms.Any())
                {
                    foreach (var room in abandonedRooms)
                    {
                        room.IsDeleted = true;
                        room.LastUpdatedAt = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
