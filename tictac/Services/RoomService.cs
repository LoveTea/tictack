using Microsoft.EntityFrameworkCore;
using tictac.Data;
using tictac.Interfaces;
using tictac.Models;

namespace tictac.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;
    private readonly IRoomCodeGenerator _codeGenerator;

    public RoomService(AppDbContext db, IRoomCodeGenerator codeGenerator)
    {
        _db = db;
        _codeGenerator = codeGenerator;
    }

    public async Task<string> CreateRoom(CancellationToken ct)
    {
        string code = "";
        bool isUnique = false;

        while (!isUnique)
        {
            code = _codeGenerator.GenerateCode();
            isUnique = !await _db.Rooms.AnyAsync(r => r.RoomId == code, ct);
        }

        var room = new Room
        {
            RoomId = code,
            LastUpdatedAt = DateTime.UtcNow
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);

        return code;
    }
}
