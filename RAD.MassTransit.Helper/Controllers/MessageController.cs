using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace RAD.MassTransit.Helper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IBus bus;
        private readonly ILogger<MessageController> logger;

        public MessageController(IBus bus, ILogger<MessageController> logger)
        {
            this.bus = bus;
            this.logger = logger;
        }

        [HttpPost(Name = "SendMessage")]
        public async void SendMessage()
        {
            for (int i = 0; i < 10; i++)
            {
                var message = new TaskMessage { Text = $"Message #{i + 1}" };
                await bus.Publish(message);
                logger.LogInformation(" [x] Sent: {message.Text}", message.Text);
            }
        }
    }
}
