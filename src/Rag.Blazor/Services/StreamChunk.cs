using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rag.Blazor.Services
{
    public class StreamChunk
    {
        public string Content { get; set; } = "";

        public Guid? ConversationId { get; set; }

        public bool IsConversationId { get; set; }
    }
}