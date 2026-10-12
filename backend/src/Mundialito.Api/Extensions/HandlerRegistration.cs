using System.Reflection;
using Mundialito.Api.Logging;
using Mundialito.Application.Abstractions.Messaging;

namespace Mundialito.Api.Extensions;

public static class HandlerRegistration
{
    // Cada interfaz de handler y el decorador de logging que la envuelve.
    private static readonly Dictionary<Type, Type> DecoratorsByHandler = new()
    {
        [typeof(ICommandHandler<>)] = typeof(LoggingCommandHandler<>),
        [typeof(ICommandHandler<,>)] = typeof(LoggingCommandHandler<,>),
        [typeof(IQueryHandler<,>)] = typeof(LoggingQueryHandler<,>)
    };

    /// <summary>
    /// Busca por reflexión todos los handlers del ensamblado y los registra envueltos en su decorador de logging.
    /// Así, al pedir ICommandHandler&lt;CreateTeamCommand, Guid&gt;, el controller recibe
    /// LoggingCommandHandler → CreateTeamCommandHandler.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false });

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(contract => contract.IsGenericType &&
                                   DecoratorsByHandler.ContainsKey(contract.GetGenericTypeDefinition()));

            foreach (var handlerInterface in handlerInterfaces)
            {
                var decoratorType = DecoratorsByHandler[handlerInterface.GetGenericTypeDefinition()]
                    .MakeGenericType(handlerInterface.GetGenericArguments());

                services.AddScoped(handlerType);
                services.AddScoped(handlerInterface, provider =>
                    ActivatorUtilities.CreateInstance(provider, decoratorType, provider.GetRequiredService(handlerType)));
            }
        }

        return services;
    }
}
