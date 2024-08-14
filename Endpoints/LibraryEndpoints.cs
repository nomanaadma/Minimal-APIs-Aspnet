using FluentValidation;
using FluentValidation.Results;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;

namespace Library.Api.Endpoints;

public class LibraryEndpoints : IEndpoints
{
	
	private const string ContentType = "application/json";
	private const string Tag = "Books";
	private const string BaseRoute = "books";
	public static void DefineEndpoints(IEndpointRouteBuilder app)
	{
		
		app.MapPost(BaseRoute, CreateBookAsync)
			.WithName("CreateBook")
			.Accepts<Book>(ContentType)
			.Produces<Book>(201)
			.Produces<IEnumerable<ValidationFailure>>(400)
			.WithTags(Tag);
		
		app.MapGet(BaseRoute, GetAllBooksAsync)
			.WithName("GetBooks")
			.Produces<IEnumerable<Book>>(200)
			.WithTags(Tag);
			//.AllowAnonymous()
			
		app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
			.WithName("GetBook")
			.Produces<Book>(200)
			.Produces(404)
			.WithTags(Tag);
		
		app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
			.WithName("UpdateBook")
			.Accepts<Book>(ContentType)
			.Produces<Book>(200)
			.Produces<IEnumerable<ValidationFailure>>(400)
			.WithTags(Tag);
		
		app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync).WithName("DeleteBook") 
			.Produces(204)
			.Produces(404)
			.WithTags(Tag);
		
	}
	
	private static async Task<IResult> CreateBookAsync(
		Book book,
		IBookService bookService,
		IValidator<Book> validator
		// LinkGenerator linker,
		// HttpContext context,
	)
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
				new("Isbn", "A book with this ISBN-13 already exists")
			});
		}

		return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);

		// var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn });
		// return Results.Created(path, book);
		// var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn });
		// return Results.Created(locationUri, book);
	}
	
	// [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
	// [AllowAnonymous]
	private static async Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm)
	{
		IEnumerable<Book> books;

		if (searchTerm is not null && !string.IsNullOrEmpty(searchTerm))
			books = await bookService.SearchByTitleAsync(searchTerm);
		else
			books = await bookService.GetAllAsync();

		return Results.Ok(books);
	}
	
	private static async Task<IResult> GetBookByIsbnAsync(string isbn,  IBookService bookService)
	{
		var book = await bookService.GetByIsbnAsync(isbn);
		return book is not null ?  Results.Ok(book) : Results.NotFound();
	}

	private static async Task<IResult> UpdateBookAsync(
		string isbn, 
		Book book,
		IBookService bookService,
		IValidator<Book> validator)
	{
		book.Isbn = isbn;

		var validationResult = await validator.ValidateAsync(book);
		if (!validationResult.IsValid)
		{
			return Results.BadRequest(validationResult.Errors);
		}

		var updated = await bookService.UpdateAsync(book);

		return updated ? Results.Ok(book) : Results.NotFound();
	}

	private static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
	{
		var deleted = await bookService.DeleteAsync(isbn);
		return deleted ? Results.NoContent() : Results.NotFound();
	}

	// public static void AddServices(IServiceCollection services, IConfiguration configuration)
	// {
	// 	services.AddSingleton<IBookService, BookService>();
	// }
}