using System.Globalization;
using Ums.Globalization.Access;

namespace Ums.Presentation.Middleware;

public class CultureMiddleware(RequestDelegate next)
{
    private const string DefaultCulture = "en";
    private const string CultureHeader = "X-Language";
    private const string AcceptLanguageHeader = "Accept-Language";

    public async Task InvokeAsync(HttpContext context)
    {
        var culture = ResolveCulture(context);

        using (CultureContext.Set(culture))
        {
            CultureInfo cultureInfo;
            try
            {
                cultureInfo = new CultureInfo(culture);
            }
            catch (CultureNotFoundException)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            await next(context);
        }
    }

    private static string ResolveCulture(HttpContext context)
    {
        var language = context.Request.Headers[CultureHeader].FirstOrDefault()
            ?? context.Request.Headers[AcceptLanguageHeader].FirstOrDefault()
            ?? DefaultCulture;

        var cultureCode = language.Split(',')[0].Trim();

        return cultureCode.Length >= 2 ? cultureCode[..2].ToLowerInvariant() : DefaultCulture;
    }
}

public static class CultureMiddlewareExtensions
{
    public static IApplicationBuilder UseCulture(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CultureMiddleware>();
    }
}
