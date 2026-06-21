using Microsoft.AspNetCore.SignalR;
using tictac.Interfaces;

namespace tictac.Hubs;

public class GameHub : Hub
{
    private readonly IHubService _hubService;   
    public GameHub(IHubService hubService)
    {
        _hubService = hubService;
    }

    public async Task JoinRoom(string code)
    {
        await _hubService.JoinRoomAsync(code, Context.ConnectionId, Context.ConnectionAborted);
    }

    public async Task MakeMove(string code, int cellIndex)
    {
        await _hubService.MakeMoveAsync(code, cellIndex, Context.ConnectionId, Context.ConnectionAborted);
    }

    public async Task RestartGame(string roomId)
    {
        await _hubService.RestartGameAsync(roomId, Context.ConnectionId, Context.ConnectionAborted);
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _hubService.DisconnectRoomAsync(Context.ConnectionId);
    }
}
