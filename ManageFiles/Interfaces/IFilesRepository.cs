using ManageFiles.Models;

namespace ManageFiles.Interfaces
{
    public interface IFilesRepository
    {
        void UploadFiles(int ticketId, List<FileModel> files);

        Task DeleteFiles(int ticketId);
    }
}
