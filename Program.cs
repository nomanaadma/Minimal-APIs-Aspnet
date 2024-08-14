using FluentValidation;
using FluentValidation.Results;
using Library.Api;
using Library.Api.Auth;
using Library.Api.Data;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCors(options =>
{
	options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
	new SqlliteConnectionFactory(config["Database:ConnectionString"]!));
builder.Services.AddSingleton<DbInitializer>();

//builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Services.AddSingleton<IBookService, BookService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
	.AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();


var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// app.UseEndpoints<Program>();

app.UseEndpoints();

app.MapGet("status", [EnableCors("AnyOrigin")]() => Results.Extensions.Html("""
<!doctype html>
<html>
    <head><title>Status page</title></head>
    <body>
        <h1>Status</h1>
        <p>The server is working fine. Bye bye!</p>
    </body>
</html>
""")).ExcludeFromDescription();
//.RequireCors("AnyOrigin");

await app.Services.GetRequiredService<DbInitializer>().InitializeAsync();

app.Run();
