using System.ClientModel;
using System.ComponentModel;
using System.Runtime.Versioning;
using AiChatClient.Maui.Models;
using AiChatClient.Maui.Services;
using Azure.AI.OpenAI;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Octokit;
using OllamaSharp;
using UglyToad.PdfPig.Filters;

namespace AiChatClient.Maui;

static class MauiProgram
{
    private static string _azureApiKey = "";
    private static string _azureEndPointUrl = "";

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureMauiHandlers(static handlers =>
			{
#if IOS || MACCATALYST
				handlers.AddHandler<CollectionView, CollectionViewNoScrollBarsHandler>();
#endif
			});


#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<App>();
		builder.Services.AddSingleton<AppShell>();

		// Add Pages + View Models
		builder.Services.AddTransientWithShellRoute<ChatPage, ChatViewModel>();
		builder.Services.AddTransientWithShellRoute<TrainedFilesPage, TrainedFilesViewModel>();

		// Add Services
		builder.Services.AddSingleton<TrainedFileNameService>();
		builder.Services.AddSingleton<IFilePicker>(static _ => FilePicker.Default);
		builder.Services.AddSingleton<IPreferences>(static _ => Preferences.Default);
		builder.Services.AddSingleton<IDeviceDisplay>(static _ => DeviceDisplay.Current);

		builder.Services.AddSingleton<ChatClientServices>();
		builder.Services.AddSingleton<GitHubServices>();

		builder.Services.AddSingleton<GitHubClient>(static _ => new GitHubClient(new ProductHeaderValue("AiChatClient")));

		// Lazy loading of the chat client to avoid creating it at startup
		builder.Services.AddChatClient(static _ => CreateOllamaChatClient());
		builder.Services.AddEmbeddingGenerator(static _ => CreateOllamaEmbeddingGenerator());
		builder.Services.AddImageGenerator(static _ => CreateAzureOpenAiImageGenerator());
		builder.Services.AddChatClient(static _ => {
			if(OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26) || OperatingSystem.IsMacOSVersionAtLeast(26)
			&& DeviceInfo.Current.DeviceType == DeviceType.Physical)
				return CreateAppleIntelligenceChatClient();
			return CreateOllamaChatClient();
        });
        builder.Services.AddEmbeddingGenerator(static _ => {
            if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)
            && DeviceInfo.Current.DeviceType == DeviceType.Physical)
                return CreateAppleEmbeddingGenerator();
            return CreateOllamaEmbeddingGenerator();
        });
        builder.Services.AddSingleton<ImageGenerationService>();
        builder.Services.AddKeyedSingleton("PdfVectorStore", static (_, _) => CreateVectorCollection());

		builder.Services.AddSingleton<PdfIngestionService>();
        return builder.Build();
	}
	static IImageGenerator CreateAzureOpenAiImageGenerator()
	{
		const string imageModel = "gpt-image-1.5";
		var apiCKeyCredential = new ApiKeyCredential(_azureApiKey);

        return new AzureOpenAIClient(new Uri(_azureEndPointUrl), apiCKeyCredential)
			.GetImageClient(imageModel)
			.AsIImageGenerator();
	}
	[SupportedOSPlatform("iOS26.0")]
	[SupportedOSPlatform("macos26.0")]
	[SupportedOSPlatform("maccatalyst26.0")]
	static IChatClient CreateAppleIntelligenceChatClient()
	{
#if IOS || MACCATALYST
		return new AppleIntelligenceClient();
#else 
		throw new NotSupportedException("Apple Intelligence is only supported on iOS and macOS platforms.");
#endif
	}

    [SupportedOSPlatform("iOS13.0")]
    [SupportedOSPlatform("macos10.15")]
    [SupportedOSPlatform("maccatalyst13.1")]
    static IEmbeddingGenerator<string, Embedding<float>> CreateAppleEmbeddingGenerator()
    {
#if IOS || MACCATALYST
		return new NLEmbeddingGenerator();
#else
        throw new NotSupportedException("Apple Intelligence is only supported on iOS and macOS platforms.");
#endif
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateOllamaEmbeddingGenerator()
    {
        const string modelId = "qwen3-embedding";

        return new OllamaApiClient(GetLocalOllamaEndpoint(), modelId);
    }

    static IChatClient CreateOllamaChatClient()
	{
		const string modelId = "qwen3.5";

		var ollamaClient = new OllamaApiClient(GetLocalOllamaEndpoint(), modelId);

		return new ChatClientBuilder(ollamaClient)
			.UseFunctionInvocation()
			.Build();

	}


	static VectorStoreCollection<string, PdfChunkRecord> CreateVectorCollection()
	{
		const string collectionName = "pdf-chunks";

#if ANDROID || IOS || MACCATALYST
	var vectorStore = new InMemoryVectorStore();
#else
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vectorstore.db");
		var vectorStore = new SqliteVectorStore($"Date Source={dbPath}");
#endif
		return vectorStore.GetCollection<string, PdfChunkRecord>(collectionName);
	}
	static IServiceCollection AddTransientWithShellRoute<TView, TViewModel>(this IServiceCollection services)
		where TView : NavigableElement, IRoutable
		where TViewModel : class, INotifyPropertyChanged
	{
		return services.AddTransientWithShellRoute<TView, TViewModel>(TView.Route);
	}

	static string GetLocalOllamaEndpoint()
	{
#if ANDROID
		return "http://10.0.2.2:11434";
#else
		return "http://127.0.0.1:11434";
#endif
	}
}