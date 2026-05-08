namespace UserService.Domain.Interfaces;

public interface IHashingService
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}
