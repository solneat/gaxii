using System.Net;

namespace SolNeat.PolicyEngine.OpenIddict;

internal static class HtmlTemplates
{
    private static string? _cachedLoginTemplate;
    private static string? _cachedTotpSetupTemplate;
    private static string? _cachedTotpTemplate;

    // ============================================================
    // LOGIN
    // ============================================================

    internal static async Task<string> GetPageHtmlAsync(
        AuthenticationPipelineOptions options,
        string? returnUrl)
    {
        return await GetLoginHtmlAsync(
            options,
            returnUrl);
    }

    internal static async Task<string> GetLoginHtmlAsync(
        AuthenticationPipelineOptions options,
        string? returnUrl)
    {
        var template = await GetTemplateAsync(
            "LoginTemplate.html",
            () => _cachedLoginTemplate,
            value => _cachedLoginTemplate = value);

        return template
            .Replace(
                "{{ReturnUrl}}",
                HtmlEncode(returnUrl));
    }

    // ============================================================
    // TOTP SETUP
    // ============================================================

    internal static async Task<string> GetTotpSetupPageHtmlAsync(
        AuthenticationPipelineOptions options,
        string? returnUrl,
        string qrCodeUri,
        string secret)
    {
        var template = await GetTemplateAsync(
            "TotpSetupTemplate.html",
            () => _cachedTotpSetupTemplate,
            value => _cachedTotpSetupTemplate = value);

        return template
            .Replace(
                "{{ReturnUrl}}",
                HtmlEncode(returnUrl))
            .Replace(
                "{{QrCodeUri}}",
                HtmlEncode(qrCodeUri))
            .Replace(
                "{{Secret}}",
                HtmlEncode(secret));
    }

    // ============================================================
    // TOTP VERIFY
    // ============================================================

    internal static async Task<string> GetTotpPageHtmlAsync(
        AuthenticationPipelineOptions options,
        string? returnUrl)
    {
        var template = await GetTemplateAsync(
            "TotpTemplate.html",
            () => _cachedTotpTemplate,
            value => _cachedTotpTemplate = value);

        return template
            .Replace(
                "{{ReturnUrl}}",
                HtmlEncode(returnUrl));
    }

    // ============================================================
    // TEMPLATE LOADING
    // ============================================================

    private static async Task<string> GetTemplateAsync(
        string fileName,
        Func<string?> getCachedTemplate,
        Action<string> setCachedTemplate)
    {
        var cached = getCachedTemplate();

        if (cached != null)
            return cached;

        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            fileName);

        if (!File.Exists(filePath))
        {
            throw new SystemException(
                $"{fileName} not found");
        }

        var template = await File.ReadAllTextAsync(filePath);

        setCachedTemplate(template);

        return template;
    }

    private static string HtmlEncode(string? value)
    {
        return WebUtility.HtmlEncode(
            value ?? string.Empty);
    }
}