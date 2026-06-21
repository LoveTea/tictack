using AsyncKeyedLock;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using tictac.Data;
using tictac.Dto;
using tictac.Hubs;
using tictac.Interfaces;
using tictac.Models;

namespace tictac.Services;

public class HubService : IHubService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<GameHub> _hubContext;
    private static AsyncKeyedLocker<string> _locker;
    private static int[][] winLines = [[0, 1, 2], [3, 4, 5], [6, 7, 8], [0, 3, 6], [1, 4, 7], [2, 5, 8], [0, 4, 8], [2, 4, 6]];

    public HubService(AppDbContext db, IHubContext<GameHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
        _locker = new();
    }

    public async Task<Room> JoinRoomAsync(string code, string connectionId, CancellationToken ct)
    {
        using (await _locker.LockAsync(code))
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == code, ct);

            if (room == null)
                throw new Exception("Такой комнаты нет");

            if (string.IsNullOrEmpty(room.CreatorConnectionId))
            {
                room.CreatorConnectionId = connectionId;
            }
            else if (string.IsNullOrEmpty(room.GuestConnectionId))
            {
                room.GuestConnectionId = connectionId;

                room.IsGameActive = true;
            }
            else
            {
                throw new InvalidOperationException("Комната занята");
            }

            room.LastUpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await BroadcastRoomUpdate(room, connectionId);

            return room;
        }
    }

    public async Task BroadcastRoomUpdate(Room room, string connectionId)
    {
        if (!string.IsNullOrEmpty(room.CreatorConnectionId))
            await _hubContext.Clients.Client(room.CreatorConnectionId).SendAsync("RoomUpdated",
                new RoomDto
                {
                    BoardState = room.BoardState,
                    CreatedAt = DateTime.UtcNow,
                    CreatorConnectionId = room.CreatorConnectionId,
                    CreatorWins = room.CreatorWins,
                    CurrentTurn = room.CurrentTurn,
                    Draws = room.Draws,
                    GuestConnectionId = room.GuestConnectionId,
                    GuestWins = room.GuestWins,
                    HasCreator = !string.IsNullOrEmpty(room.CreatorConnectionId),
                    HasGuest = !string.IsNullOrEmpty(room.GuestConnectionId),
                    IsDeleted = room.IsDeleted,
                    IsGameActive = room.IsGameActive,
                    LastUpdatedAt = room.LastUpdatedAt,
                    RoomId = room.RoomId,
                    Winner = room.Winner,
                    Role = "X"
                }
                );

        if (!string.IsNullOrEmpty(room.GuestConnectionId))
            await _hubContext.Clients.Client(room.GuestConnectionId).SendAsync("RoomUpdated", new RoomDto
            {
                BoardState = room.BoardState,
                CreatedAt = DateTime.UtcNow,
                CreatorConnectionId = room.CreatorConnectionId,
                CreatorWins = room.CreatorWins,
                CurrentTurn = room.CurrentTurn,
                Draws = room.Draws,
                GuestConnectionId = room.GuestConnectionId,
                GuestWins = room.GuestWins,
                HasCreator = !string.IsNullOrEmpty(room.CreatorConnectionId),
                HasGuest = !string.IsNullOrEmpty(room.GuestConnectionId),
                IsDeleted = room.IsDeleted,
                IsGameActive = room.IsGameActive,
                LastUpdatedAt = room.LastUpdatedAt,
                RoomId = room.RoomId,
                Winner = room.Winner,
                Role = "O"
            });
    }

    public async Task MakeMoveAsync(
        string code,
        int cellIndex,
        string connectionId,
        CancellationToken ct)
    {
        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.RoomId == code, ct);

        if (room == null)
            throw new InvalidOperationException("Комната не найдена");

        string playerSign;

        if (connectionId == room.CreatorConnectionId)
            playerSign = "X";
        else if (connectionId == room.GuestConnectionId)
            playerSign = "O";
        else
            throw new InvalidOperationException("Вы не участник комнаты");

        var board = room.BoardState.ToCharArray();

        if (board[cellIndex] != '-')
            throw new InvalidOperationException("Клетка занята");

        if (room.CurrentTurn != playerSign)
            throw new InvalidOperationException("Сейчас ход другого игрока");

        board[cellIndex] = playerSign[0];

        room.BoardState = new string(board);

        if (CheckWin(board, playerSign[0]))
        {
            room.Winner = playerSign;
            room.IsGameActive = false;

            if (playerSign == "X")
                room.CreatorWins++;
            else
                room.GuestWins++;
        }
        else if (!board.Contains('-'))
        {
            room.Winner = "Draw";
            room.IsGameActive = false;
            room.Draws++;
        }
        else
        {
            room.CurrentTurn =
                room.CurrentTurn == "X" ? "O" : "X";
        }

        room.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await BroadcastRoomUpdate(room, connectionId);
    }

    private static bool CheckWin(char[] board, char p)
    {
        return winLines.Any(l => board[l[0]] == p && board[l[1]] == p && board[l[2]] == p);
    }

    public async Task RestartGameAsync(string roomId, string connectionId, CancellationToken ct)
    {
        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.RoomId == roomId, ct);

        if (room == null)
            throw new InvalidOperationException("Комната не найдена");

        if (room.IsGameActive)
            return;

        room.BoardState = "---------";
        room.CurrentTurn = "X";
        room.Winner = null;
        room.IsGameActive = true;
        room.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await BroadcastRoomUpdate(room, connectionId);
    }

    public async Task DisconnectRoomAsync(string connectionId)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(x => x.CreatorConnectionId == connectionId || x.GuestConnectionId == connectionId);
        if (room is null) return;

        using (await _locker.LockAsync(room.RoomId))
        {
            if (room.CreatorConnectionId == connectionId)
            {
                room.CreatorConnectionId = null;
            }

            if (room.GuestConnectionId == connectionId)
            {
                room.GuestConnectionId = null;
            }
            room.LastUpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await BroadcastRoomUpdate(room, connectionId);
        }
    }
}
