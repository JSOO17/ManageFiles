using Azure.Messaging.ServiceBus;
using ManageFiles.Config;
using ManageFiles.Interfaces;
using ManageFiles.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ManageFiles.Listeners
{
    public class ServiceBusListener : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger _logger;
        private readonly ServiceBusProcessor _processor;
        private readonly IFilesRepository _filesRepository;
        private readonly IEventManage _eventManage;

        public ServiceBusListener(IOptions<ServiceBusOptions> options, IFilesRepository filesRepository, ILogger<ServiceBusListener> logger, IEventManage eventManage)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _processor = _client.CreateProcessor(options.Value.QueueName, new ServiceBusProcessorOptions());
            _logger = logger;
            _filesRepository = filesRepository;
            _eventManage = eventManage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var messageBody = args.Message.Body.ToString();

            var message = JsonSerializer.Deserialize<MessageModel>(messageBody) ?? throw new Exception(messageBody);

            _logger.LogInformation($"Received message: WorkItemId: {message.WorkItemId}. File: {message.Files[0].Name}.");

            if (message.TypeMessage.Equals("Upload"))
            {
                _filesRepository.UploadFiles(message.TicketId, message.Files);

                await _eventManage.SendEvent(message.WorkItemId, message.Files.FirstOrDefault());
            }
            else
            {
                await _filesRepository.DeleteFiles(message.TicketId);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Message handler encountered an exception");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.StopProcessingAsync(stoppingToken);
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _processor?.DisposeAsync();
            _client?.DisposeAsync();
            base.Dispose();
        }
    }
}
