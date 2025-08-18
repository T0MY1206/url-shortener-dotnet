namespace Shortener.Api.Models;

/// <summary>
/// Respuesta al crear una URL corta.
/// </summary>
public record CreateUrlResponse(
    string Slug,
    string ShortUrl,
    string TargetUrl
);