using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Core.Security.Entities;

public class User : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; } = true;
    public AuthenticatorType AuthenticatorType { get; set; } = AuthenticatorType.None;

    public virtual ICollection<UserOperationClaim> UserOperationClaims { get; set; } = new List<UserOperationClaim>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual OtpAuthenticator? OtpAuthenticator { get; set; }
    public virtual EmailAuthenticator? EmailAuthenticator { get; set; }
}

public class OperationClaim : Entity
{
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<UserOperationClaim> UserOperationClaims { get; set; } = new List<UserOperationClaim>();
}

public class UserOperationClaim : Entity
{
    public Guid UserId { get; set; }
    public Guid OperationClaimId { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual OperationClaim OperationClaim { get; set; } = null!;
}

public class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
    public virtual User User { get; set; } = null!;
}

public class OtpAuthenticator : Entity
{
    public Guid UserId { get; set; }
    public byte[] SecretKey { get; set; } = Array.Empty<byte>();
    public bool IsVerified { get; set; }
    public virtual User User { get; set; } = null!;
}

public class EmailAuthenticator : Entity
{
    public Guid UserId { get; set; }
    public string? ActivationKey { get; set; }
    public bool IsVerified { get; set; }
    public virtual User User { get; set; } = null!;
}

public enum AuthenticatorType
{
    None = 0,
    Email = 1,
    Otp = 2
}
