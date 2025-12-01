using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using WebApiDuende;

var builder = WebApplication.CreateBuilder(args);

// Open up security restrictions to allow this to work
// Not recommended in production
var deploySwaggerUI = builder.Configuration.GetValue<bool>("DeploySwaggerUI");
var isDev = builder.Environment.IsDevelopment();

builder.Services.AddSecurityHeaderPolicies()
    .SetPolicySelector((PolicySelectorContext ctx) =>
    {
        // sum is weak security headers due to Swagger UI deployment
        // should only use in development
        if (deploySwaggerUI)
        {
            // Weakened security headers for Swagger UI
            if (ctx.HttpContext.Request.Path.StartsWithSegments("/swagger"))
            {
                return SecurityHeadersDefinitionsSwagger.GetHeaderPolicyCollection(isDev);
            }

            // Strict security headers
            return SecurityHeadersDefinitionsAPI.GetHeaderPolicyCollection(isDev);
        }
        // Strict security headers for production
        else
        {
            return SecurityHeadersDefinitionsAPI.GetHeaderPolicyCollection(isDev);
        }
    });

builder.Services.AddControllers();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var oauthConfig = builder.Configuration.GetSection("ProfileApiConfigurations");
        options.Authority = oauthConfig["Authority"];
        options.Audience = oauthConfig["Audience"];
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidTypes = ["at+jwt"];
    });

builder.Services.AddOpenApi(options =>
{
    //options.UseTransformer((document, context, cancellationToken) =>
    //{
    //    document.Info = new()
    //    {
    //        Title = "My API",
    //        Version = "v1",
    //        Description = "API for Damien"
    //    };
    //    return Task.CompletedTask;
    //});
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

//app.MapOpenApi(); // /openapi/v1.json
app.MapOpenApi("/openapi/v1/openapi.json");
//app.MapOpenApi("/openapi/{documentName}/openapi.json");

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1/openapi.json", "v1");
    });
}


app.Run();