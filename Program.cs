using FluentValidation;
using FluentValidation.Results;
using Library.Api.Data;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
	new SqlliteConnectionFactory(config["Database:ConnectionString"]!));
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("books", async (Book book, 
	IBookService bookService,
	IValidator<Book> validator
	) =>
{

	var validationResult = await validator.ValidateAsync(book);
	if (!validationResult.IsValid)
	{
		return Results.BadRequest(validationResult.Errors);
	}
	
	var created = await bookService.CreateAsync(book);

	if (!created)
	{
		return Results.BadRequest( new List<ValidationFailure>
		{
			new("Isbn"," A book with this ISBN-13 already exists")
		});
	}

	return Results.Created($"/books/{book.Isbn}", book);

});


await app.Services.GetRequiredService<DbInitializer>().InitializeAsync();

app.Run();