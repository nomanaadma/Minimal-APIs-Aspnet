﻿using Library.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.Api.Tests.Integration;

public class LibraryApiFactory : WebApplicationFactory<IApiMarker>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(collection =>
		{
			collection.RemoveAll(typeof(IDbConnectionFactory));
			collection.AddSingleton<IDbConnectionFactory>(_ =>
				new SqlliteConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared"));

		});
	}
}