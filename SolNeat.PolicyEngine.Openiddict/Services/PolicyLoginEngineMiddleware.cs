using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SolNeat.PolicyEngine.OpenIddict;
using System.Text.Json;

namespace SolNeat.PolicyEngine;

internal class PolicyLoginEngineMiddlewareStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<PolicyLoginEngineMiddleware>();
            next(app);
        };
    }
}

public class PolicyLoginEngineMiddleware
{
    private const string PendingAuthenticationCookie = "policy.pending-auth";

    private readonly RequestDelegate _next;
    private readonly AuthenticationPipelineOptions _authOptions;
    private readonly LoginEngineOptions _loginOptions;
    private readonly IDataProtector _dataProtector;

    public PolicyLoginEngineMiddleware(
        RequestDelegate next,
        IOptions<AuthenticationPipelineOptions> authOptions,
        IOptions<LoginEngineOptions> loginOptions,
        IDataProtectionProvider dataProtectionProvider)
    {
        _next = next;
        _authOptions = authOptions.Value;
        _loginOptions = loginOptions.Value;

        _dataProtector = dataProtectionProvider.CreateProtector(
            "SolNeat.PolicyEngine.PolicyLogin.PendingAuthentication.v1");
    }

    public async Task InvokeAsync(
        HttpContext context,
        IPolicyAuthenticationService authService,
        ITotpService totpService)
    {
        var path = context.Request.Path;

        if (path.Equals(
                _loginOptions.LoginPath,
                StringComparison.OrdinalIgnoreCase))
        {
            await HandleLoginAsync(
                context,
                authService);

            return;
        }

        var totpSetupPath = $"{_loginOptions.LoginPath}/totp/setup";

        if (path.Equals(
                totpSetupPath,
                StringComparison.OrdinalIgnoreCase))
        {
            await HandleTotpSetupAsync(
                context,
                authService,
                totpService);

            return;
        }

        var totpPath = $"{_loginOptions.LoginPath}/totp";

        if (path.Equals(
                totpPath,
                StringComparison.OrdinalIgnoreCase))
        {
            await HandleTotpAsync(
                context,
                authService);

            return;
        }

        await _next(context);
    }

    // ============================================================
    // LOGIN
    // ============================================================

    private async Task HandleLoginPageAsync(HttpContext context)
    {
        var returnUrl = GetSafeReturnUrl(context.Request.Query["ReturnUrl"].ToString());

        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.Redirect(
                string.IsNullOrEmpty(returnUrl)
                    ? "/"
                    : returnUrl);

            return;
        }

        var html = await HtmlTemplates.GetLoginHtmlAsync(
            _authOptions,
            returnUrl);

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";

        await context.Response.WriteAsync(html);
        return;
    }

    private async Task HandleLoginAsync(
        HttpContext context,
        IPolicyAuthenticationService authService)
    {
        if (HttpMethods.IsGet(context.Request.Method))
        {
            await HandleLoginPageAsync(context);
            return;
        }

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return;
        }

        var form = await context.Request.ReadFormAsync();

        var username = form["username"].ToString();
        var password = form["password"].ToString();

        var returnUrl = GetSafeReturnUrl(
            form["returnUrl"].ToString());

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "Username and password are required.");

            return;
        }

        var actor = await authService
            .AuthenticatePasswordAsync(
                username,
                password,
                context.RequestAborted)
            .ConfigureAwait(false);

        if (actor == null)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync(
                "Invalid credentials.");

            return;
        }

        // --------------------------------------------------------
        // MFA disabled
        // --------------------------------------------------------

        if (!_authOptions.EnableTotpMfa)
        {
            await CompleteAuthenticationAsync(
                context,
                actor.SubjectId,
                returnUrl);

            return;
        }

        // --------------------------------------------------------
        // MFA enabled
        // --------------------------------------------------------

        var totpSecret = await authService
            .GetTotpSecretAsync(
                actor.Id,
                context.RequestAborted)
            .ConfigureAwait(false);

        // No secret or not confirmed:
        // user must configure/confirm TOTP.
        if (totpSecret == null || !totpSecret.IsConfirmed)
        {
            await CreatePendingAuthenticationCookieAsync(
                context,
                actor.Id,
                returnUrl);

            context.Response.Redirect(
                GetTotpSetupPath());

            return;
        }

        // TOTP already configured.
        await CreatePendingAuthenticationCookieAsync(
            context,
            actor.Id,
            returnUrl);

        context.Response.Redirect(
            GetTotpPath());
    }

    // ============================================================
    // TOTP SETUP
    // ============================================================

    private async Task HandleTotpSetupAsync(
        HttpContext context,
        IPolicyAuthenticationService authService,
        ITotpService totpService)
    {
        var pending = await GetPendingAuthenticationAsync(
            context,
            authService);

        if (pending == null)
            return;

        var totpSecret = await authService
            .GetTotpSecretAsync(
                pending.Actor.Id,
                context.RequestAborted)
            .ConfigureAwait(false);

        if (totpSecret == null)
        {
            totpSecret = await authService
                .CreateTotpSecretAsync(
                    pending.Actor.Id,
                    context.RequestAborted)
                .ConfigureAwait(false);
        }

        // Another request may have confirmed it already.
        if (totpSecret.IsConfirmed)
        {
            context.Response.Redirect(
                GetTotpPath());

            return;
        }

        // --------------------------------------------------------
        // GET
        // --------------------------------------------------------

        if (HttpMethods.IsGet(context.Request.Method))
        {
            var qrCodeUri = totpService.GetQrCodeUri(
                pending.Actor.SubjectId,
                totpSecret.Secret);

            var html = await HtmlTemplates.GetTotpSetupPageHtmlAsync(
                _authOptions,
                pending.ReturnUrl,
                qrCodeUri,
                totpSecret.Secret);

            context.Response.StatusCode =
                StatusCodes.Status200OK;

            context.Response.ContentType =
                "text/html; charset=utf-8";

            await context.Response.WriteAsync(html);

            return;
        }

        // --------------------------------------------------------
        // POST
        // --------------------------------------------------------

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode =
                StatusCodes.Status405MethodNotAllowed;

            return;
        }

        var form = await context.Request.ReadFormAsync();

        var code = form["totpCode"].ToString();

        if (string.IsNullOrWhiteSpace(code))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "TOTP code is required.");

            return;
        }

        // IMPORTANT:
        // This method is specifically for the first confirmation.
        var confirmed = await authService
            .VerifyAndConfirmTotpAsync(
                pending.Actor.Id,
                code,
                context.RequestAborted)
            .ConfigureAwait(false);

        if (!confirmed)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "Invalid TOTP code.");

            return;
        }

        // TOTP setup is complete.
        DeletePendingAuthenticationCookie(context);

        await CompleteAuthenticationAsync(
            context,
            pending.Actor.SubjectId,
            pending.ReturnUrl);
    }

    // ============================================================
    // TOTP VERIFY
    // ============================================================

    private async Task HandleTotpAsync(
        HttpContext context,
        IPolicyAuthenticationService authService)
    {
        var pending = await GetPendingAuthenticationAsync(
            context,
            authService);

        if (pending == null)
            return;

        var totpSecret = await authService
            .GetTotpSecretAsync(
                pending.Actor.Id,
                context.RequestAborted)
            .ConfigureAwait(false);

        // If TOTP was removed or became unconfirmed,
        // redirect to setup.
        if (totpSecret == null || !totpSecret.IsConfirmed)
        {
            context.Response.Redirect(
                GetTotpSetupPath());

            return;
        }

        // --------------------------------------------------------
        // GET
        // --------------------------------------------------------

        if (HttpMethods.IsGet(context.Request.Method))
        {
            var html = await HtmlTemplates.GetTotpPageHtmlAsync(
                _authOptions,
                pending.ReturnUrl);

            context.Response.StatusCode =
                StatusCodes.Status200OK;

            context.Response.ContentType =
                "text/html; charset=utf-8";

            await context.Response.WriteAsync(html);

            return;
        }

        // --------------------------------------------------------
        // POST
        // --------------------------------------------------------

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode =
                StatusCodes.Status405MethodNotAllowed;

            return;
        }

        var form = await context.Request.ReadFormAsync();

        var code = form["totpCode"].ToString();

        if (string.IsNullOrWhiteSpace(code))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "TOTP code is required.");

            return;
        }

        var valid = await authService
            .VerifyTotpAsync(
                pending.Actor.Id,
                code,
                context.RequestAborted)
            .ConfigureAwait(false);

        if (!valid)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "Invalid TOTP code.");

            return;
        }

        DeletePendingAuthenticationCookie(context);

        await CompleteAuthenticationAsync(
            context,
            pending.Actor.SubjectId,
            pending.ReturnUrl);
    }

    // ============================================================
    // PENDING AUTHENTICATION
    // ============================================================

    private async Task CreatePendingAuthenticationCookieAsync(
        HttpContext context,
        Guid actorId,
        string? returnUrl)
    {
        var state = new PendingAuthenticationState
        {
            ActorId = actorId,
            ReturnUrl = GetSafeReturnUrl(returnUrl),
            CreatedAtUtc = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(state);

        var protectedValue = _dataProtector.Protect(json);

        context.Response.Cookies.Append(
            PendingAuthenticationCookie,
            protectedValue,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,

                // Authentication should not remain pending indefinitely.
                MaxAge = TimeSpan.FromMinutes(5)
            });
    }

    private async Task<PendingAuthentication?> GetPendingAuthenticationAsync(
        HttpContext context,
        IPolicyAuthenticationService authService)
    {
        if (!context.Request.Cookies.TryGetValue(
                PendingAuthenticationCookie,
                out var protectedValue))
        {
            await AuthenticationStateErrorAsync(
                context,
                "Authentication session expired.");

            return null;
        }

        PendingAuthenticationState? state;

        try
        {
            var json = _dataProtector.Unprotect(
                protectedValue);

            state = JsonSerializer.Deserialize<PendingAuthenticationState>(
                json);
        }
        catch (Exception)
        {
            DeletePendingAuthenticationCookie(context);

            await AuthenticationStateErrorAsync(
                context,
                "Invalid authentication session.");

            return null;
        }

        if (state == null ||
            state.ActorId == Guid.Empty)
        {
            DeletePendingAuthenticationCookie(context);

            await AuthenticationStateErrorAsync(
                context,
                "Invalid authentication session.");

            return null;
        }

        // Explicit expiration check.
        if (state.CreatedAtUtc <
            DateTime.UtcNow.AddMinutes(-5))
        {
            DeletePendingAuthenticationCookie(context);

            await AuthenticationStateErrorAsync(
                context,
                "Authentication session expired.");

            return null;
        }

        var actor = await authService
            .GetActorByIdAsync(
                state.ActorId,
                context.RequestAborted)
            .ConfigureAwait(false);

        if (actor == null)
        {
            DeletePendingAuthenticationCookie(context);

            await AuthenticationStateErrorAsync(
                context,
                "Authentication session expired.");

            return null;
        }

        return new PendingAuthentication(
            actor,
            state.ReturnUrl);
    }

    private void DeletePendingAuthenticationCookie(
        HttpContext context)
    {
        context.Response.Cookies.Delete(
            PendingAuthenticationCookie,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
    }

    private static async Task AuthenticationStateErrorAsync(
        HttpContext context,
        string message)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        await context.Response.WriteAsync(message);
    }

    // ============================================================
    // URLS
    // ============================================================

    private string GetTotpSetupPath()
    {
        return $"{_loginOptions.LoginPath}/totp/setup";
    }

    private string GetTotpPath()
    {
        return $"{_loginOptions.LoginPath}/totp";
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        // Only local relative URLs are accepted.
        if (!Uri.TryCreate(
                returnUrl,
                UriKind.Relative,
                out var uri))
        {
            return "/";
        }

        if (uri.IsAbsoluteUri)
            return "/";

        if (!returnUrl.StartsWith("/"))
            return "/";

        // Prevent //evil.com style URLs.
        if (returnUrl.StartsWith("//"))
            return "/";

        return returnUrl;
    }

    // ============================================================
    // COMPLETE AUTHENTICATION
    // ============================================================

    private static async Task CompleteAuthenticationAsync(
        HttpContext context,
        string subjectId,
        string returnUrl)
    {
        // TODO:
        // Здесь будет фактическое создание ClaimsPrincipal
        // и SignIn/OpenIddict ticket.

        if (!string.IsNullOrEmpty(returnUrl) &&
            Uri.IsWellFormedUriString(
                returnUrl,
                UriKind.Relative))
        {
            context.Response.Redirect(returnUrl);
            return;
        }

        context.Response.StatusCode =
            StatusCodes.Status200OK;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(
            new
            {
                success = true,
                subjectId
            },
            context.RequestAborted);
    }

    // ============================================================
    // STATE
    // ============================================================

    private sealed class PendingAuthenticationState
    {
        public Guid ActorId { get; set; }

        public string ReturnUrl { get; set; } = "/";

        public DateTime CreatedAtUtc { get; set; }
    }

    private sealed record PendingAuthentication(
        Actor Actor,
        string ReturnUrl);
}