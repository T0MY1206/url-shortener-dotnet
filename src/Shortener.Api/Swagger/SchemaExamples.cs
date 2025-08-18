using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Shortener.Api.Models;

namespace Shortener.Api.Swagger;

public static class SchemaExamples
{
    public static void MapExamples(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
    {
        c.MapType<CreateUrlRequest>(() => new OpenApiSchema
        {
            Example = new OpenApiObject
            {
                ["originalUrl"] = new OpenApiString("https://www.google.com"),
                ["customSlug"] = new OpenApiString("")
            }
        });

        c.MapType<CreateUrlResponse>(() => new OpenApiSchema
        {
            Example = new OpenApiObject
            {
                ["slug"] = new OpenApiString("aB9xY2"),
                ["shortUrl"] = new OpenApiString("/aB9xY2"),
                ["targetUrl"] = new OpenApiString("https://www.google.com")
            }
        });

        c.MapType<ErrorResponse>(() => new OpenApiSchema
        {
            Example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Descripción del error")
            }
        });
    }
}