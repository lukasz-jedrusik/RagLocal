namespace Rag.Services.Backend.Application.DataTransferObjects
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PictureUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
