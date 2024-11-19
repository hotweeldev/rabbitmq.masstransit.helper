using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RAD.MassTransit.Connector.Config;

namespace RAD.MassTransit.Connector.Extension
{
    public class RabbitMQExtension
    {
        public void ConfigureMassTransit(IServiceCollection services, RabbitConfig config)
        {
            services.AddMassTransit(x =>
            {
                foreach (var queue in config.Consumers)
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == queue.QueueAssembly)
                        ?? throw new InvalidOperationException($"Assembly '{queue.QueueAssembly}' could not be loaded.");

                    var consumerType = assembly.GetType(queue.QueueService)
                        ?? throw new InvalidOperationException($"Consumer type '{queue.QueueService}' could not be resolved in assembly '{queue.QueueAssembly}'.");

                    x.AddConsumer(consumerType);
                }

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(config.Host, h =>
                    {
                        h.Username(config.Username);
                        h.Password(config.Password);
                    });

                    foreach (var queue in config.Consumers)
                    {
                        var assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == queue.QueueAssembly);
                        var consumerType = assembly?.GetType(queue.QueueService);

                        cfg.ReceiveEndpoint(queue.QueueName, e =>
                        {
                            e.Durable = true;
                            e.ConcurrentMessageLimit = queue.MaxConcurrent;

                            e.ConfigureConsumer(context, consumerType);
                        });
                    }
                });

            });
        }
    }
}
