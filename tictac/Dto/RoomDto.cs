namespace tictac.Dto;

public class RoomDto
{
    public string RoomId { get; set; }
    public string? CreatorConnectionId { get; set; }
    public string? GuestConnectionId { get; set; }
    public bool HasCreator { get; set; } = false;
    public bool HasGuest { get; set; } = false;
    public int CreatorWins { get; set; }
    public int GuestWins { get; set; }
    public int Draws { get; set; }

    public string BoardState { get; set; } = "---------";
    public string CurrentTurn { get; set; } = "X";
    public string? Winner { get; set; }
    public bool IsGameActive { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public string Role { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
