using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using EmployeeAPI.Attributes;

namespace EmployeeAPI.Attributes
{
    public class SwaggerControllerOrderAttribute : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var orderedTags = context.ApiDescriptions
                .Select(desc => desc.ActionDescriptor)
                .OfType<ControllerActionDescriptor>()
                .Select(c =>
                {
                    var orderAttr = c.ControllerTypeInfo.GetCustomAttribute<SwaggerGroupOrderAttribute>();
                    return new
                    {
                        Name = c.ControllerName,
                        Order = orderAttr?.Order ?? int.MaxValue
                    };
                })
                .Distinct()
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name)
                .ToList();

            swaggerDoc.Tags = orderedTags
                .Select(x => new OpenApiTag { Name = x.Name })
                .ToList();
        }

    }
}
