using System.Reflection;

namespace Library.Api.Endpoints.Internal;

public static class EndpointExtensions
{
	public static void AddEndpoints<TMarker>(this IServiceCollection services, 
		IConfiguration configuration)
	{
		var typeMarker = typeof(TMarker);
		var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

		foreach (var endpointType in endpointTypes)
		{
			endpointType.GetMethod(nameof(IEndpoints.AddServices))!
				.Invoke(null, [services, configuration]);
		}
	}
	
	public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
	{
		var typeMarker = typeof(TMarker);
		var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

		foreach (var endpointType in endpointTypes)
		{
			endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
				.Invoke(null, [app]);
		}
	}
	
	private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
	{
		var endpointTypes = typeMarker.Assembly.DefinedTypes
			.Where(x => x is { IsAbstract: false, IsInterface: false } &&
			            typeof(IEndpoints).IsAssignableFrom(x));
		return endpointTypes;
	}
	
}