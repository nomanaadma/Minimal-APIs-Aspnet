using System.Data;
using Microsoft.Data.Sqlite;

namespace Library.Api.Data;

public class SqlliteConnectionFactory : IDbConnectionFactory
{
	private readonly string _connectionString;

	// ReSharper disable once ConvertToPrimaryConstructor
	public SqlliteConnectionFactory(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task<IDbConnection> CreateConnectionAsync()
	{
		var connection = new SqliteConnection(_connectionString);
		await connection.OpenAsync();
		return connection;
	}
}