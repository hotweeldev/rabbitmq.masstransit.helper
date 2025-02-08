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

                foreach (var dlxQueue in config.DlxConsumer)
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == dlxQueue.QueueAssembly)
                        ?? throw new InvalidOperationException($"Assembly '{dlxQueue.QueueAssembly}' could not be loaded.");

                    var consumerType = assembly.GetType(dlxQueue.QueueService)
                        ?? throw new InvalidOperationException($"Consumer type '{dlxQueue.QueueService}' could not be resolved in assembly '{dlxQueue.QueueAssembly}'.");

                    x.AddConsumer(consumerType);
                }

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(config.Host, h =>
                    {
                        h.Username(config.Username);
                        h.Password(config.Password);
                        h.Heartbeat(TimeSpan.FromSeconds(30));
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

                        cfg.UseMessageRetry(retryConfig =>
                        {
                            retryConfig.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                        });
                    }

                    foreach (var dlxQueue in config.DlxConsumer)
                    {
                        var assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == dlxQueue.QueueAssembly);
                        var consumerType = assembly?.GetType(dlxQueue.QueueService);

                        cfg.ReceiveEndpoint(dlxQueue.DlxQueueName, e =>
                        {
                            e.ConfigureConsumeTopology = false;
                            e.SetQueueArgument("x-dead-letter-exchange", dlxQueue.DlxName);
                            e.SetQueueArgument("x-dead-letter-routing-key", dlxQueue.QueueName);
                            e.SetQueueArgument("x-queue-mode", "lazy");
                        });

                        cfg.ReceiveEndpoint(dlxQueue.QueueName, e =>
                        {
                            e.Durable = true;
                            e.ConcurrentMessageLimit = dlxQueue.MaxConcurrent;
                            e.ConfigureConsumer(context, consumerType);
                        });

                        cfg.UseMessageRetry(retryConfig =>
                        {
                            retryConfig.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                        });

                    }
                });

            });
        }
    }
}
