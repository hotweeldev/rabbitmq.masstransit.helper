using MassTransit;

namespace RAD.MassTransit.Helper.TaskService
{
    public class Consumer : IConsumer<TaskMessage>
    {
        private readonly ILogger<Consumer> logger;
        public Consumer(ILogger<Consumer> logger)
        {
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskMessage> context)
        {
            Random random = new Random();
            int node = random.Next(10, 100);
            var message = context.Message;
            logger.LogInformation(" [x] Received: {message.Text} at {time} with node {node}", message.Text, DateTime.Now, node);
            await Task.Delay(new Random().Next(1000, 3000));
            logger.LogInformation(" [x] Done processing: {message.Text} at {DateTime.Now} with node {node}", message.Text, DateTime.Now, node);
        }
    }
}
