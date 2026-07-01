using AiChatClient.Maui.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiChatClient.UnitTests
{
    public class ImageGenerationServiceTests : BaseTest
    {
        [Test]
        public async Task GenerateImageAsync_ReturnsNotNullByteArray()
        {
            // Arrange
            var service = new ImageGenerationService(ImageGenerator);

            // Act
            var result = await service.GenerateImageAsync("A beautiful sunset over the mountains", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public async Task GenerateImageAsync_EquivalenceEvaluator()
        {
            // Arrange
            const string PROMPT = "A beautiful sunset over the mountains";
            var service = new ImageGenerationService(ImageGenerator);

            var equivalenceEvaluator = new EquivalenceEvaluator();
            var equivalenceContext = new EquivalenceEvaluatorContext("The image contains a sunset and mountains");

            // Act
            var imageBytes = await service.GenerateImageAsync(PROMPT, CancellationToken.None);
            Assert.That(imageBytes, Is.Not.Null);

            var message = new List<ChatMessage>
            {
                new(ChatRole.User,
                [
                    new DataContent(imageBytes, "image/png"),
                    new TextContent("Describe what this image contains")
                    ])
            };

            var chatResponse = await ChatClient.GetResponseAsync(message);

            var equivalenceResult = await equivalenceEvaluator.EvaluateAsync(
                message,
                chatResponse,
                new Microsoft.Extensions.AI.Evaluation.ChatConfiguration(ChatClient),
                [equivalenceContext]);

            var equivalenceResultMetric = equivalenceResult.Get<NumericMetric>(EquivalenceEvaluator.EquivalenceMetricName);


            // Assert
            Assert.That(equivalenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
        }
    }
}
