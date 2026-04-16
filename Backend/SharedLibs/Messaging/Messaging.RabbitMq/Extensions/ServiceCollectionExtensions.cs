using Messaging.Abstractions.Dispatching;
using Messaging.Abstractions.Registry;
using Messaging.RabbitMq.Connection;
using Messaging.RabbitMq.Dispatching;
using Messaging.RabbitMq.Options;
using Messaging.RabbitMq.Publishing;
using Messaging.RabbitMq.Registry;
using Messaging.RabbitMq.Topology;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Messaging.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventDispatcher(this IServiceCollection services)
        {
            services.AddSingleton<IEventDispatcher, EventDispatcher>();

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            return services;
        }

        public static IServiceCollection AddEventTypeRegistry(this IServiceCollection services)
        {
            services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();

            return services;
        }

        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

            services.AddSingleton<ConnectionManager>();
            return services;
        }

        public static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services)
        {
            services.AddSingleton<IEventPublisher>(sp =>
            {
                var connectionManager = sp.GetRequiredService<ConnectionManager>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>();

                var connection = connectionManager
                    .GetConnectionAsync()
                    .GetAwaiter()
                    .GetResult();

                var channel = connection
                    .CreateChannelAsync()
                    .GetAwaiter()
                    .GetResult();

                RabbitMqTopology
                    .EnsureTopologyAsync(channel, options.Value)
                    .GetAwaiter()
                    .GetResult();

                var exchange = options.Value.Exchanges.FirstOrDefault()?.Name
                    ?? throw new InvalidOperationException("No exchange configured");

                return new EventPublisher(channel, exchange);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMqConsumer(this IServiceCollection services)
        {
            services.AddSingleton<IChannel>(sp =>
            {
                var connectionManager = sp.GetRequiredService<ConnectionManager>();
                var options = sp.GetRequiredService<RabbitMqOptions>();

                var connection = connectionManager
                    .GetConnectionAsync()
                    .GetAwaiter()
                    .GetResult();

                var channel = connection
                    .CreateChannelAsync()
                    .GetAwaiter()
                    .GetResult();

                RabbitMqTopology
                    .EnsureTopologyAsync(channel, options)
                    .GetAwaiter()
                    .GetResult();

                return channel;
            });

            return services;
        }
    }
}
