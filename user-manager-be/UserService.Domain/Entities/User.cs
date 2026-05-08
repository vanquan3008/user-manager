using UserService.Domain.Enums;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class User : AuditableAggregateRoot<Guid>
{
    private User() : base() { }

    private User(
        Guid id, 
        Username username, 
        Email email, 
        PersonName name,
        DateOnly dateOfBirth,
        DateTime utcNow) : base()
    {
        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DateOfBirth = dateOfBirth;
        Address = Address.Create();
        
        ValidateDateOfBirth(dateOfBirth, utcNow);
        MarkCreated();
    }

    public static User Create(
        Guid id,
        Username username,
        Email email,
        PersonName name,
        DateOnly dateOfBirth,
        DateTime utcNow)
    {
        return new User(id, username, email, name, dateOfBirth, utcNow);
    }

    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PersonName Name { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? Bio { get; private set; }
    public string? Website { get; private set; }

    public Address Address { get; private set; } = null!;
    
    public Guid? CurrentSubscriptionId { get; private set; }
    public string PreferredLanguage { get; private set; } = "en";
    public string TimeZone { get; private set; } = "UTC";
    public bool IsMarketingEmailEnabled { get; private set; } = true;
    public bool IsPushNotificationEnabled { get; private set; } = true;
    public bool IsEmailNotificationEnabled { get; private set; } = true;
    public bool IsPrivateProfile { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsSSOAccount { get; private set; }

    public void UpdateBasicInfo(PersonName name, DateOnly dateOfBirth, DateTime utcNow)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ValidateDateOfBirth(dateOfBirth, utcNow);
        DateOfBirth = dateOfBirth;
        Touch();
    }

    public void UpdateProfile(
        PersonName name,
        string? avatarUrl = null,
        string? coverImageUrl = null,
        string? bio = null,
        string? website = null,
        string? phoneNumber = null,
        Gender? gender = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        AvatarUrl = avatarUrl?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        Bio = bio?.Trim();
        Website = website?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Gender = gender;
        Touch();
    }

    public void UpdateLocation(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Touch();
    }

    public void UpdatePreferences(
        string? preferredLanguage = null,
        string? timeZone = null,
        bool? isMarketingEmailEnabled = null,
        bool? isPushNotificationEnabled = null,
        bool? isEmailNotificationEnabled = null,
        bool? isPrivateProfile = null)
    {
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
            PreferredLanguage = preferredLanguage.Trim();

        if (!string.IsNullOrWhiteSpace(timeZone))
            TimeZone = timeZone.Trim();

        if (isMarketingEmailEnabled.HasValue)
            IsMarketingEmailEnabled = isMarketingEmailEnabled.Value;

        if (isPushNotificationEnabled.HasValue)
            IsPushNotificationEnabled = isPushNotificationEnabled.Value;

        if (isEmailNotificationEnabled.HasValue)
            IsEmailNotificationEnabled = isEmailNotificationEnabled.Value;

        if (isPrivateProfile.HasValue)
            IsPrivateProfile = isPrivateProfile.Value;

        Touch();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        AddDomainEvent(new EmailVerifiedEvent(Id, Email.Value));
        Touch();
    }

    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        Touch();
    }

    public void VerifyAccount(DateTime verifiedAt)
    {
        IsVerified = true;
        VerifiedAt = verifiedAt;
        Touch();
    }

    public void EnableTwoFactor()
    {
        IsTwoFactorEnabled = true;
        Touch();
    }

    public void DisableTwoFactor()
    {
        IsTwoFactorEnabled = false;
        Touch();
    }

    public void EnableSSO()
    {
        IsSSOAccount = true;
        Touch();
    }

    public void DisableSSO()
    {
        IsSSOAccount = false;
        Touch();
    }

    public void MarkAsDeleted()
    {
        MarkDeleted();
    }

    private static void ValidateDateOfBirth(DateOnly dateOfBirth, DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
            age--;

        if (age < 13)
            throw new InvalidDomainObjectException("DateOfBirth", dateOfBirth, "User must be at least 13 years old.");

        if (age > 120)
            throw new InvalidDomainObjectException("DateOfBirth", dateOfBirth, "Invalid date of birth.");
    }
}
