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

        public AzureEventGridClient()
        {
            var credentials = new AzureKeyCredential(TopicKey);
            _client = new EventGridPublisherClient(new Uri(TopicEndpoint), credentials);
        }

        public async Task SendEvent(int workItemId, FileModel file)
        {
            try
            {
                var events = new List<EventGridEvent>
                {
                    new EventGridEvent(
                        subject: "/ManageFiles/FileCreated",
                        eventType: "FileCreated",
                        dataVersion: "1.0",
                        data: new
                        {
                            WorkItemId = workItemId,
                            File = file
                    })
                };

                await _client.SendEventsAsync(events);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
