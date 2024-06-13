using Azure.Storage.Blobs;
using ManageFiles.Config;
using ManageFiles.Interfaces;
using ManageFiles.Models;
using Microsoft.Extensions.Options;

namespace ManageFiles.Repositories
{
    public class AzureBlobStorageRepository : IFilesRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobStorageRepository> _logger;

        public AzureBlobStorageRepository(IOptions<AzureBlobOptions> options, ILogger<AzureBlobStorageRepository> logger)
        {
            _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
            _logger = logger;
        }

        public async Task DeleteFiles(int TicketId)
        {
            try
            {
                await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: $"{TicketId}/"))
                {
                    var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                    await blobClient.DeleteIfExistsAsync();
                    _logger.LogInformation($"Deleted file: {blobItem.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting files in folder");
            }
        }

        public void UploadFiles(int ticketId, List<FileModel> files)
        {
            files.ForEach(async file =>
            {
                try
                {
                    if (file.Content == null || file.Name == null) throw new Exception();
                    var contentBytes = Convert.FromBase64String(file.Content);
                    _logger.LogInformation($"Uploading file {file.Name}");
                    await UploadFileAsync(file.Name, contentBytes, ticketId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading file: {file.Name}");
                }
            });
        }

        private async Task UploadFileAsync(string fileName, byte[] fileContent, int ticketId)
        {
            var blobClient = _containerClient.GetBlobClient($"{ticketId}/{fileName}");
            using var stream = new MemoryStream(fileContent);
            await blobClient.UploadAsync(stream, overwrite: true);
            _logger.LogInformation($"Uploaded file: {fileName}. TicketId: {ticketId}");
        }
    }
}
