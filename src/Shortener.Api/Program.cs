using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Shortener.Api.Models;
using Shortener.Api.Swagger;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- OpenAPI (Swashbuckle) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 2 documentos (idiomas)
    c.SwaggerDoc("v1-es", new OpenApiInfo
    {
        Title = "URL Shortener (ES)",
        Version = "v1",
        Description = "API para acortar URLs: crear un slug y redirigir."
    });
    c.SwaggerDoc("v1-en", new OpenApiInfo
    {
        Title = "URL Shortener (EN)",
        Version = "v1",
        Description = "API to shorten URLs: create a slug and redirect."
    });

    // summaries XML (si lo habilitaste en el .csproj)
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    SchemaExamples.MapExamples(c);
    c.DocInclusionPredicate((doc, api) => true); // incluye todos los endpoints en ambos docs
});
// -------------------------------------------

// In-memory store (MVP)
var urls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

// Utils
string NormalizeUrl(string url)
{
    if (!Regex.IsMatch(url, @"^https?://", RegexOptions.IgnoreCase))
        url = "https://" + url;
    return url.Trim();
}
string GenerateSlug(int length = 6)
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var data = RandomNumberGenerator.GetBytes(length);
    var sb = new StringBuilder(length);
    foreach (var b in data) sb.Append(chars[b % chars.Length]);
    return sb.ToString();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 1) Genera los JSON en /swagger/{documentName}/swagger.json
    app.UseSwagger();

    // 2) UI moderna en /docs, apuntando explícitamente a esos JSON
    app.MapScalarApiReference("/docs", options =>
    {
        options
            .WithTitle("URL Shortener – API Docs")
            .WithDarkMode(true) // toggle Dark/Light
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
            .AddDocument("v1-es", "Español")
            .AddDocument("v1-en", "English");
    });
}

/// <summary>
/// Crea una URL corta a partir de una URL original.
/// </summary>
/// <remarks>
/// Body:
/// { "originalUrl": "https://example.com", "customSlug": "opcional" }
/// Devuelve 201 con slug y URL corta.
/// </remarks>
app.MapPost("/urls", (CreateUrlRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.OriginalUrl))
        return Results.BadRequest(new ErrorResponse("originalUrl es obligatorio"));

    var target = NormalizeUrl(req.OriginalUrl);
    if (!Uri.TryCreate(target, UriKind.Absolute, out _))
        return Results.BadRequest(new ErrorResponse("originalUrl no es una URL válida"));

    string slug;
    if (!string.IsNullOrWhiteSpace(req.CustomSlug))
    {
        slug = req.CustomSlug.Trim();
        if (slug.Length > 32 || !Regex.IsMatch(slug, @"^[a-zA-Z0-9_-]+$"))
            return Results.BadRequest(new ErrorResponse("customSlug inválido (máx 32, solo a-zA-Z0-9_-)."));
        if (urls.ContainsKey(slug))
            return Results.Conflict(new ErrorResponse("Slug ya existe"));
        urls[slug] = target;
    }
    else
    {
        do { slug = GenerateSlug(); } while (urls.ContainsKey(slug));
        urls[slug] = target;
    }

    var response = new CreateUrlResponse(slug, $"/{slug}", target);
    return Results.Created($"/urls/{slug}", response);
})
.Produces<CreateUrlResponse>(StatusCodes.Status201Created)
.Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ErrorResponse>(StatusCodes.Status409Conflict)
.WithSummary("Crear URL corta")
.WithDescription("Genera un slug para redirigir a la URL original. Puede enviarse un slug personalizado si no está en uso.")
.WithTags("URLs")
.WithOpenApi();

/// <summary>
/// Redirige al destino asociado al slug.
/// </summary>
/// <remarks>
/// Responde 302 a la URL original si el slug existe; 404 en caso contrario.
/// </remarks>
app.MapGet("/{slug}", (string slug) =>
{
    if (!urls.TryGetValue(slug, out var target))
        return Results.NotFound(new ErrorResponse("Slug no encontrado"));

    return Results.Redirect(target, permanent: false);
})
.Produces(StatusCodes.Status302Found)
.Produces<ErrorResponse>(StatusCodes.Status404NotFound)
.WithSummary("Redirigir por slug")
.WithDescription("Dado un slug existente, redirige con 302 a la URL original.")
.WithTags("URLs")
.WithOpenApi();

app.Run();