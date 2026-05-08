namespace UserService.Domain.Entities;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }

    void SetCreatedAt(DateTime utcNow);
    void SetUpdatedAt(DateTime utcNow);
}

