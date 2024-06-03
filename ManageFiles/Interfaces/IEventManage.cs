using ManageFiles.Models;

namespace ManageFiles.Interfaces
{
    public interface IEventManage
    {
        Task SendEvent(int workItemId, FileModel file);
    }
}
