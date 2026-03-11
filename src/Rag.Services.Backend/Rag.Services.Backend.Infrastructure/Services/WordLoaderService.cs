using DocumentFormat.OpenXml.Packaging;
using Rag.Services.Backend.Application.Interfaces.Services;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class WordLoaderService : IWordLoaderService
    {
        public string Load(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return doc.MainDocumentPart.Document.Body.InnerText;
        }
    }
}