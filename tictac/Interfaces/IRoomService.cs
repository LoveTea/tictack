namespace tictac.Interfaces;

public interface IRoomService
{
    public Task<string> CreateRoom(CancellationToken ct);
}
