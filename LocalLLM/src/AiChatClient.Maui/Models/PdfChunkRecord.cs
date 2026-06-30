using System;
using System.Collections.Generic;
using System.Text;

namespace AiChatClient.Maui.Models
{
    public record PdfChunkRecord
    {
        public string Text { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}
