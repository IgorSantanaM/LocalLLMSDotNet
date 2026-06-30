using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiChatClient.Maui.Models
{
    public record PdfChunkRecord
    {
        [VectorStoreKey]
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string Text { get; set; } = string.Empty;
        [VectorStoreData(IsFullTextIndexed = true)]
        public string SourceFile { get; set; } = string.Empty;

        [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}
