namespace Rag.Services.Backend.Domain.Models
{
    public abstract class DictionaryStringIdEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}