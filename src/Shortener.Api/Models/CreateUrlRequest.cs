namespace Shortener.Api.Models;

/// <summary>
/// Payload para crear una URL corta.
/// </summary>
public record CreateUrlRequest(
    string OriginalUrl,
    string? CustomSlug
);