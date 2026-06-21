using System.Security.Cryptography;
using tictac.Interfaces;
using Sqids;

namespace tictac.Services;

public class RoomCodeGenerator : IRoomCodeGenerator
{
    private readonly SqidsEncoder<int> _encoder;

    public RoomCodeGenerator()
    {
        _encoder = new SqidsEncoder<int>(new() { MinLength = 6 });
    }

    public string GenerateCode()
    {
        int randomNumber = RandomNumberGenerator.GetInt32(1, int.MaxValue);
        return _encoder.Encode(randomNumber);
    }
}
