namespace Rag.Services.Backend.Domain.Models
{
    public abstract class DictionaryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}