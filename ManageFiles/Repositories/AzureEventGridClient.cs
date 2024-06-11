using Azure;
using Azure.Messaging.EventGrid;
using ManageFiles.Config;
using ManageFiles.Interfaces;
using ManageFiles.Models;
using Microsoft.Extensions.Options;

namespace ManageFiles.Repositories
{
    public class AzureEventGridClient : IEventManage
    {
        private EventGridPublisherClient _client;
        private readonly ILogger<AzureEventGridClient> _logger;

        public AzureEventGridClient(IOptions<EventGridOptions> options, ILogger<AzureEventGridClient> logger)
        {
            var credentials = new AzureKeyCredential(options.Value.TopicKey);
            _client = new EventGridPublisherClient(new Uri(options.Value.TopicEndpoint), credentials);

            _logger = logger;
        }

        public async Task SendEvent(int workItemId, FileModel file)
        {
            try
            {
                var data = new
                {
                    WorkItemId = workItemId,
                    File = file
                };

                var events = new List<EventGridEvent>
                {
                    new EventGridEvent(
                        subject: "/ManageFiles/FileCreated",
                        eventType: "FileCreated",
                        dataVersion: "1.0",
                        data: data)
                };

                await _client.SendEventsAsync(events);

                _logger.LogInformation($"event sent. data: {data}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong. {ex}", ex);
            }
        }
    }
}
