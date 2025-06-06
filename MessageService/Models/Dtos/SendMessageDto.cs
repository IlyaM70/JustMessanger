using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.Dtos
{
    public class SendMessageDto
    {
        [Required]
        public string? SenderId { get; set; }
        [Required]
        public string? RecipientId { get; set; }
        [Required]
        public string? Text { get; set; }
    }
}
