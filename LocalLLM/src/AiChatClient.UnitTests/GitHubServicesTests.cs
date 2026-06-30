using AiChatClient.Maui.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.NLP;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace AiChatClient.UnitTests
{
    public class GitHubServicesTests : BaseTest
    {
        [Test]
        public async Task EnsureGitHubBioMatchesExpectedResults()
        {
            // Arrange
            var chatClientService = new ChatClientServices(ChatClient);
            var gitHubService = new GitHubServices(GitHubClient);
            var f1Evaluator = new F1Evaluator();
            var f1Context = new F1EvaluatorContext("Software Engineer, Open Source Enthusiast, and AI Researcher");

            var options = new ChatOptions
            {
                Tools =
                [
                    AIFunctionFactory.Create(gitHubService.GetIgorUserName),
                    AIFunctionFactory.Create(gitHubService.GetUserBio),
                ]
            };

            var requestMessage = new ChatMessage(ChatRole.User, "What is Igor's GitHub bio?");
            // Act
            string assistantResponse = string.Empty;

            await foreach(var response in chatClientService.GetStreamingResponseAsync(requestMessage, options, CancellationToken.None))
            {
                assistantResponse += response.Text;
            }

            var assistantResponseMessage = new ChatResponse(new ChatMessage(ChatRole.Assistant, assistantResponse));
            var f1Metric = await f1Evaluator.EvaluateAsync(
                [requestMessage],
                assistantResponseMessage,
                new(ChatClient),
                [f1Context]);
            var f1ResultMetric = f1Metric.Get<NumericMetric>(F1Evaluator.F1MetricName);
            // Assert
            Assert.That(f1ResultMetric.Value, Is.GreaterThanOrEqualTo(0.8), "The F1 score is below the expected threshold.");
        }
    }
}
