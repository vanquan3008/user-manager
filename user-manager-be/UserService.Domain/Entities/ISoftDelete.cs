namespace UserService.Domain.Entities;

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }

    void SoftDelete(DateTime utcNow);
    void RestoreSoftDelete();
}

