using System.Globalization;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StorageCompany.Core;

namespace StorageCompany.Api.Middleware;

public sealed class HmacRequestSigningMiddleware(
    RequestDelegate next,
    IOptionsMonitor<AppOptions> options,
    IMemoryCache cache)
{
    private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromMinutes(5);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldRequireSignature(context.Request))
        {
            await next(context);
            return;
        }

        var timestampHeader = context.Request.Headers["X-Request-Timestamp"].ToString();
        var nonce = context.Request.Headers["X-Request-Nonce"].ToString();
        var providedSignature = context.Request.Headers["X-Request-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(timestampHeader) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(providedSignature))
        {
            throw new AuthenticationException("Missing request signature headers");
        }

        if (!long.TryParse(timestampHeader, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            throw new AuthenticationException("Invalid request timestamp");
        }

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if (DateTimeOffset.UtcNow - timestamp > AllowedClockSkew ||
            timestamp - DateTimeOffset.UtcNow > AllowedClockSkew)
        {
            throw new AuthenticationException("Request timestamp outside allowed window");
        }

        var nonceCacheKey = $"hmac-nonce:{nonce}";
        if (cache.TryGetValue(nonceCacheKey, out _))
        {
            throw new AuthenticationException("Replay detected");
        }

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var bodyHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(body)))
            .ToLowerInvariant();

        var canonicalRequest =
            $"{context.Request.Method}\n" +
            $"{context.Request.Path}{context.Request.QueryString}\n" +
            $"{timestampHeader}\n" +
            $"{nonce}\n" +
            $"{bodyHash}";

        var expectedSignature = ComputeHmac(options.CurrentValue.RequestSigningSecret, canonicalRequest);

        if (!FixedTimeEquals(providedSignature, expectedSignature))
        {
            throw new AuthenticationException("Invalid request signature");
        }

        cache.Set(nonceCacheKey, true, AllowedClockSkew);

        await next(context);
    }

    private static bool ShouldRequireSignature(HttpRequest request)
    {
        if (!request.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        if (request.Path.StartsWithSegments("/api/auth"))
        {
            return false;
        }

        return HttpMethods.IsPost(request.Method) ||
               HttpMethods.IsPut(request.Method) ||
               HttpMethods.IsPatch(request.Method) ||
               HttpMethods.IsDelete(request.Method);
    }

    private static string ComputeHmac(string secret, string value)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(value);

        using var hmac = new HMACSHA256(key);
        return Convert.ToBase64String(hmac.ComputeHash(bytes));
    }

    private static bool FixedTimeEquals(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}