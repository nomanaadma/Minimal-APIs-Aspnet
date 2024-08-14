using Library.Api.Data;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
	new SqlliteConnectionFactory(config["Database:ConnectionString"]!));
builder.Services.AddSingleton<DbInitializer>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseSwagger();
app.UseSwaggerUI();

await app.Services.GetRequiredService<DbInitializer>().InitializeAsync();

app.Run();