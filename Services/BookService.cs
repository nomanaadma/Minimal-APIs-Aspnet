﻿using Dapper;
using Library.Api.Data;
using Library.Api.Models;

namespace Library.Api.Services;

public class BookService : IBookService
{
	private readonly IDbConnectionFactory _connectionFactory;

	// ReSharper disable once ConvertToPrimaryConstructor
	public BookService(IDbConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task<bool> CreateAsync(Book book)
	{

		using var connection = await _connectionFactory.CreateConnectionAsync();

		var result = await connection.ExecuteAsync(
			"""
				INSERT INTO BOOKS (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
					VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)
			""", book);

		return result > 0;
	}

	public async Task<Book?> GetByIsbnAsync(string isbn)
	{
		using var connection = await _connectionFactory.CreateConnectionAsync();

		return await connection.QuerySingleOrDefaultAsync<Book>(
			"SELECT * FROM books WHERE Isbn = @Isbn LIMIT 1", new { Isbn = isbn });

	}

	public async Task<IEnumerable<Book>> GetAllAsync()
	{
		using var connection = await _connectionFactory.CreateConnectionAsync();
		return await connection.QueryAsync<Book>("SELECT * FROM books");
	}

	public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
	{
		using var connection = await _connectionFactory.CreateConnectionAsync();
		return await connection.QueryAsync<Book>(
			"SELECT * FROM books WHERE title LIKE '%' || @SearchTerm || '%'", new { SearchTerm = searchTerm });

	}

	public async Task<bool> UpdateAsync(Book book)
	{
		var existingBook = await GetByIsbnAsync(book.Isbn);

		if (existingBook is null)
		{
			return false;
		}
		
		using var connection = await _connectionFactory.CreateConnectionAsync();
		var result = await connection.ExecuteAsync(
			"""
			UPDATE Books SET Title = @Title, Author = @Author, ShortDescription = @ShortDescription,
			PageCount = @PageCount, ReleaseDate = @ReleaseDate WHERE Isbn = @Isbn
			""",
			book);
		return result > 0;
		
	}

	public async Task<bool> DeleteAsync(string isbn)
	{
		using var connection = await _connectionFactory.CreateConnectionAsync();
		var result = await connection.ExecuteAsync(
			"DELETE FROM Books WHERE Isbn = @Isbn", new {Isbn = isbn});
		return result > 0;
	}
}