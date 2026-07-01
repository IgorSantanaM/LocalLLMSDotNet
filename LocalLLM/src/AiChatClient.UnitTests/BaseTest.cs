using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Octokit;
using OllamaSharp;
using System.ClientModel;

namespace AiChatClient.UnitTests
{
    public abstract class BaseTest 
    {
        readonly Lazy<IEmbeddingGenerator<string, Embedding<float>>> _embeddingGeneratorHolder = new(CreateOllamaEmbeddingGenerator);
        readonly Lazy<IChatClient> _chatClientHolder = new(CreateOllamaChatClient);
        readonly Lazy<IImageGenerator> _imageGeneratorHolder = new(CreateAzureOpenAiImageGenerator);
        readonly Lazy<GitHubClient> _gitHubClientHolder = new(new GitHubClient(new ProductHeaderValue("AiChatClient")));
        private static string _azureApiKey = "";
        private static string _azureEndPointUrl = "";

        protected IChatClient ChatClient  => _chatClientHolder.Value;
        protected GitHubClient GitHubClient => _gitHubClientHolder.Value;
        protected IImageGenerator ImageGenerator => _imageGeneratorHolder.Value;
        protected IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator => _embeddingGeneratorHolder.Value;

        [OneTimeTearDown]
        protected virtual void TearDown()
        {
            if (_chatClientHolder.IsValueCreated)
                ChatClient.Dispose();

            if (_embeddingGeneratorHolder.IsValueCreated)
                EmbeddingGenerator.Dispose();
        }

        static IImageGenerator CreateAzureOpenAiImageGenerator()
        {
            const string imageModel = "gpt-image-1.5";
            var apiCKeyCredential = new ApiKeyCredential(_azureApiKey);

            return new AzureOpenAIClient(new Uri(_azureEndPointUrl), apiCKeyCredential)
                .GetImageClient(imageModel)
                .AsIImageGenerator();
        }

        private static IEmbeddingGenerator<string, Embedding<float>> CreateOllamaEmbeddingGenerator()
        {
            const string modelId = "qwen3-embedding";

            return new OllamaApiClient("http://127.0.0.1:11434", modelId);
        }

        static IChatClient CreateOllamaChatClient()
        {
            const string modelId = "qwen3.5";

            var ollamaClient = new OllamaApiClient("http://127.0.0.1:11434", modelId);

            return new ChatClientBuilder(ollamaClient)
                .UseFunctionInvocation()
                .Build();
        }
    }
}
