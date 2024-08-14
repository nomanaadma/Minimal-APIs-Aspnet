using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
	private readonly WebApplicationFactory<IApiMarker> _factory;

	public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
	{
		_factory = factory;
	}

	[Fact]
	public async void CreateBook_CreatesBook_WhenDataIsCorrect()
	{
		// Arrange
		var httpClient = _factory.CreateClient();
		var book = GenerateBook();
		
		// Act
		var result = await httpClient.PostAsJsonAsync("/books", book);
		var createdBook = await result.Content.ReadFromJsonAsync<Book>();

		// Assert
		result.StatusCode.Should().Be(HttpStatusCode.Created);
		createdBook.Should().BeEquivalentTo(book);
		// result.Headers.Location.Should().Be($"/books/{book.Isbn}");
	}
	
	[Fact]
	public async void CreateBook_Fails_WhenIsbnIsInvalid()
	{
		// Arrange
		var httpClient = _factory.CreateClient();
		var book = GenerateBook();
		book.Isbn = "invalid";
		
		// Act
		var result = await httpClient.PostAsJsonAsync("/books", book);
		var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationErrors>>();
		var error = errors!.Single();

		// Assert
		result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		error.PropertyName.Should().Be("Isbn");
		error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
	}

	private Book GenerateBook(string title = "Test Title")
	{
		return new Book
		{
			Isbn = GenerateIsbn(),
			Title = title,
			Author = "Test Author",
			PageCount = 210,
			ShortDescription = "Short Description",
			ReleaseDate = new DateTime(2025, 1, 1)
		};
	}

	private string GenerateIsbn()
	{
		return $"{Random.Shared.Next(100,999)}-{Random.Shared.Next(1000000000,2100000009)}";
	}
}