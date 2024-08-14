using System.Diagnostics.SymbolStore;
using FluentValidation;
using FluentValidation.Results;
using Library.Api;
using Library.Api.Auth;
using Library.Api.Data;
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

app.MapPost("books", async (Book book,
	IBookService bookService,
	IValidator<Book> validator
	// LinkGenerator linker,
	// HttpContext context,
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
		return Results.BadRequest(new List<ValidationFailure>
		{
			new("Isbn", " A book with this ISBN-13 already exists")
		});
	}

	return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);

	// var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn });
	// return Results.Created(path, book);
	// var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn });
	// return Results.Created(locationUri, book);

})
.WithName("CreateBook")
.Accepts<Book>("application/json")
.Produces<Book>(201)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Books");


app.MapGet("books",
	// [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
	// [AllowAnonymous]
	async (IBookService bookService, string? searchTerm) =>
	{

		IEnumerable<Book> books;

		if (searchTerm is not null && !string.IsNullOrEmpty(searchTerm))
			books = await bookService.SearchByTitleAsync(searchTerm);
		else
			books = await bookService.GetAllAsync();

		return Results.Ok(books);

	})
.WithName("GetBooks")
.Produces<IEnumerable<Book>>(200)
.WithTags("Books");
//.AllowAnonymous()

app.MapGet("books/{isbn}", async (string isbn,  IBookService bookService) =>
{
	var book = await bookService.GetByIsbnAsync(isbn);
	return book is not null ?  Results.Ok(book) : Results.NotFound();
})
.WithName("GetBook")
.Produces<Book>(200)
.Produces(404)
.WithTags("Books");



app.MapPut("books/{isbn}", async (string isbn, Book book,
		IBookService bookService,
		IValidator<Book> validator
	) =>
	{

		book.Isbn = isbn;

		var validationResult = await validator.ValidateAsync(book);
		if (!validationResult.IsValid)
		{
			return Results.BadRequest(validationResult.Errors);
		}

		var updated = await bookService.UpdateAsync(book);

		return updated ? Results.Ok(book) : Results.NotFound();

	}).WithName("UpdateBook")
	.Accepts<Book>("application/json")
	.Produces<Book>(200)
	.Produces<IEnumerable<ValidationFailure>>(400);


app.MapDelete("books/{isbn}", async (string isbn, 
	IBookService bookService
) =>
{
	
	var deleted = await bookService.DeleteAsync(isbn);
	return deleted ? Results.NoContent() : Results.NotFound() ;

}).WithName("DeleteBook") 
.Produces(204)
.Produces(404)
.WithTags("Books");


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