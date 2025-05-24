namespace MessageService.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public string? RecipientId { get; set; }
        public string? Text { get; set; }
        public DateTime SentAt { get; set; }
    }
}
