namespace Rag.Services.Backend.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
