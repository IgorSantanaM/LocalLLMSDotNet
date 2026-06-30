using AiChatClient.Maui.Models;
using AiChatClient.Maui.Services;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace AiChatClient.UnitTests
{
    public class PdfIngestionServiceTests : BaseTest
    {
        [Test]
        public async Task SearchAsync_ReturnsExpectedText_WhenScoresAreWithingThresholds()
        {
            // Arrange
            const string pdfText = "The quick brown fox jumps over the lazy dog";

            var vectorCollection = CreateVectorStoreCollection();
            var pdfIngestionService = new PdfIngestionService(vectorCollection, EmbeddingGenerator);


            var pdfStream = CreatePdfStream(pdfText);
            await pdfIngestionService.IngestPdfAsync(pdfStream, "foxes.pdf");

            // Act
            var result = await pdfIngestionService.SearchAsync(pdfText);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(pdfText));
        }

        static VectorStoreCollection<string, PdfChunkRecord> CreateVectorStoreCollection()
        {
            var vectorStore = new InMemoryVectorStore();
            return vectorStore.GetCollection<string, PdfChunkRecord>("test-pdf-chunks");
        }
        static Stream CreatePdfStream(string text)
        {
            var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Helvetica);
            page.AddText(text, 12, new PdfPoint(25, 700), font);

            var bytes = builder.Build();
            return new MemoryStream(bytes);
        }
    }
}
