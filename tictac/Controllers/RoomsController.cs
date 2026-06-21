using Microsoft.AspNetCore.Mvc;
using tictac.Interfaces;

namespace tictac.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRoom(CancellationToken ct)
    {
        var id = await _roomService.CreateRoom(ct);

        return Ok(new { RoomId = id });
    }
}
