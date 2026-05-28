using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Media.WebApi;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile) || p.Type == typeof(IFormFileCollection))
            .ToList();

        if (fileParams.Count == 0) return;

        var toRemove = operation.Parameters
            .Where(p => fileParams.Any(fp => fp.Name == p.Name)).ToList();
        foreach (var p in toRemove) operation.Parameters.Remove(p);

        var schema = new OpenApiSchema { Type = "object" };
        foreach (var pd in context.ApiDescription.ParameterDescriptions)
        {
            if (pd.Type == typeof(IFormFile) || pd.Type == typeof(IFormFileCollection))
                schema.Properties[pd.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
            else if (pd.Source.DisplayName == "Form")
                schema.Properties[pd.Name] = new OpenApiSchema { Type = "string" };
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = { ["multipart/form-data"] = new OpenApiMediaType { Schema = schema } }
        };
    }
}
