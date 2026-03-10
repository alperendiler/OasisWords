using FluentAssertions;
using OasisWords.Core.Security.Hashing;
using Xunit;

namespace OasisWords.Application.Tests.Auth;

public class HashingHelperTests
{
    [Fact]
    public void CreatePasswordHash_ProducesNonEmptyHashAndSalt()
    {
        HashingHelper.CreatePasswordHash("TestPassword123!", out byte[] hash, out byte[] salt);

        hash.Should().NotBeEmpty("HMACSHA512 must produce a non-empty hash");
        salt.Should().NotBeEmpty("HMACSHA512 must produce a non-empty salt");
    }

    [Fact]
    public void CreatePasswordHash_DifferentCallsProduceDifferentSalts()
    {
        HashingHelper.CreatePasswordHash("SamePassword", out _, out byte[] salt1);
        HashingHelper.CreatePasswordHash("SamePassword", out _, out byte[] salt2);

        salt1.Should().NotBeEquivalentTo(salt2,
            "each call generates a fresh random HMAC key (salt)");
    }

    [Fact]
    public void VerifyPasswordHash_CorrectPassword_ReturnsTrue()
    {
        const string password = "SecurePass@2024";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        bool result = HashingHelper.VerifyPasswordHash(password, hash, salt);

        result.Should().BeTrue("the same password should verify successfully");
    }

    [Fact]
    public void VerifyPasswordHash_WrongPassword_ReturnsFalse()
    {
        HashingHelper.CreatePasswordHash("CorrectPassword", out byte[] hash, out byte[] salt);

        bool result = HashingHelper.VerifyPasswordHash("WrongPassword", hash, salt);

        result.Should().BeFalse("a different password must not verify against the stored hash");
    }

    [Fact]
    public void VerifyPasswordHash_TamperedHash_ReturnsFalse()
    {
        const string password = "IntegrityTest";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        byte[] tampered = (byte[])hash.Clone();
        tampered[0] ^= 0xFF; // flip the first byte

        bool result = HashingHelper.VerifyPasswordHash(password, tampered, salt);

        result.Should().BeFalse("a tampered hash must not verify");
    }

    [Fact]
    public void VerifyPasswordHash_TamperedSalt_ReturnsFalse()
    {
        const string password = "SaltIntegrityTest";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        byte[] tampered = (byte[])salt.Clone();
        tampered[0] ^= 0xFF;

        bool result = HashingHelper.VerifyPasswordHash(password, hash, tampered);

        result.Should().BeFalse("a tampered salt must cause HMAC mismatch");
    }

    [Fact]
    public void CreatePasswordHash_EmptyPassword_StillProducesHash()
    {
        // Edge case — empty string is a valid HMAC input
        HashingHelper.CreatePasswordHash(string.Empty, out byte[] hash, out byte[] salt);
        bool result = HashingHelper.VerifyPasswordHash(string.Empty, hash, salt);

        result.Should().BeTrue("empty password should round-trip correctly");
    }
}
