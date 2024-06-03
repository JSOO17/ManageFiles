namespace ManageFiles.Models
{
    public class MessageModel
    {
        public int TicketId { get; set; }
        public int WorkItemId { get; set; }
        public List<FileModel> Files { get; set; }
        public string TypeMessage { get; set; }
    }
}
