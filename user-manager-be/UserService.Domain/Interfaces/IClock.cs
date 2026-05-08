namespace UserService.Domain.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}
