using OtpNet;
using Microsoft.Extensions.Options;

namespace SolNeat.PolicyEngine.OpenIddict;

public interface ITotpService
{
    string GenerateSecret();
    string GetQrCodeUri(string username, string secret);
    bool ValidateCode(string secret, string code);
}

internal sealed class TotpService : ITotpService
{
    private readonly AuthenticationPipelineOptions _options;

    public TotpService(IOptions<AuthenticationPipelineOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GetQrCodeUri(string username, string secret)
    {
        var encodedIssuer = Uri.EscapeDataString(_options.TotpIssuer);
        var encodedUser = Uri.EscapeDataString(username);
        return $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={secret}&issuer={encodedIssuer}&digits=6";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            var bytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(bytes);
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        catch
        {
            return false;
        }
    }
}