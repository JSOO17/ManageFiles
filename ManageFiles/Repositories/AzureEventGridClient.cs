using Azure;
using Azure.Messaging.EventGrid;
using ManageFiles.Interfaces;
using ManageFiles.Models;

namespace ManageFiles.Repositories
{
    public class AzureEventGridClient : IEventManage
    {
        private const string TopicEndpoint = "https://uploadazuredevops.eastus-1.eventgrid.azure.net/api/events";
        private const string TopicKey = "bz6FYYaDwNryTE1xXYGqsnICfv2l0heEgAZEGKYOtD4=";

        private EventGridPublisherClient _client;
        private readonly ILogger<AzureEventGridClient> _logger;

        public AzureEventGridClient(ILogger<AzureEventGridClient> logger)
        {
            var credentials = new AzureKeyCredential(TopicKey);
            _client = new EventGridPublisherClient(new Uri(TopicEndpoint), credentials);

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
