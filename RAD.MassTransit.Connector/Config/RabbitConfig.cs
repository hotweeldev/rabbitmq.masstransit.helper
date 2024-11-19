namespace RAD.MassTransit.Connector.Config
{
    public class RabbitConfig
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public IList<Consumer> Consumers { get; set; } = new List<Consumer>();
    }

    public class Consumer
    {
        public string QueueName { get; set; } = string.Empty;
        public int MaxConcurrent { get; set; } = 1;
        public string QueueAssembly { get; set; } = string.Empty;
        public string QueueService {  get; set; } = string.Empty;
    }
}
