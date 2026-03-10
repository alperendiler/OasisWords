using OasisWords.Core.Security.Entities;
using OTP = OtpNet;
using System.Security.Cryptography;

namespace OasisWords.Core.Security.Authenticators;

public static class EmailAuthenticatorHelper
{
    public static string CreateEmailActivationKey()
    {
        byte[] keyBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(keyBytes)
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)[..32];
    }
}

public static class OtpNetOtpAuthenticatorHelper
{
    public static byte[] GenerateSecretKey()
    {
        return OTP.KeyGeneration.GenerateRandomKey(20);
    }

    public static string GenerateOtpUri(string accountName, byte[] secretKey, string issuer = "OasisWords")
    {
        string base32Secret = OTP.Base32Encoding.ToString(secretKey);
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}" +
               $"?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
    }

    public static bool ValidateOtp(byte[] secretKey, string code)
    {
        OTP.Totp totp = new(secretKey);
        return totp.VerifyTotp(code, out _, OTP.VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
