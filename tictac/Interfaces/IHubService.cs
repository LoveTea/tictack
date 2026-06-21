using tictac.Models;

namespace tictac.Interfaces;

public interface IHubService
{
    Task<Room> JoinRoomAsync(string code, string connectionId, CancellationToken ct);
    Task BroadcastRoomUpdate(Room room, string connectionId);
    Task MakeMoveAsync(string code, int cellIndex, string connectionId, CancellationToken ct);
    Task RestartGameAsync(string roomId, string connectionId, CancellationToken ct);
    Task DisconnectRoomAsync(string connectionId);
}
