using Rag.Services.Backend.Application.Interfaces.Services;
using UglyToad.PdfPig;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class PdfLoaderService : IPdfLoaderService
    {
        public string Load(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);
            return string.Join(
                "\n",
                pdf.GetPages().Select(p => p.Text));
        }
    }
}