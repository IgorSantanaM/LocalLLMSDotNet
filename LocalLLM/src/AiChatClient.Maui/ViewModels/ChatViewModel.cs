using AiChatClient.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AiChatClient.Maui;

public partial class ChatViewModel(
    PdfIngestionService pdfIngestionService,
    ChatClientServices chatClientServices, 
    GitHubServices gitHubServices) : BaseViewModel
{

    readonly ChatClientServices _chatClientServices = chatClientServices;
    readonly GitHubServices _gitHubServices = gitHubServices;
    readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;
    public string ImageGenerationModeImageButtonSource => IsImageGenerationMode
        ? "palette_filled.png"
        : "palette_outline.png";

    public ObservableCollection<ChatModel> ConversationHistory { get; } = [];

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitInputTextCommand))]
    public partial bool CanSubmitInputTextExecute { get; private set; } = true;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ImageGenerationModeImageButtonSource))]
    public partial bool IsImageGenerationMode { get; set; } = false;

    public async Task ClearConversationHistory(CancellationToken token)
    {
        CanSubmitInputTextExecute = false;

        try
        {
            ConversationHistory.Clear();
            _chatClientServices.ClearConversationHistory();
        }
        finally
        {
            CanSubmitInputTextExecute = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmitInputTextExecute))]
    void ToggleImageGenerationModeButton()
    {
        IsImageGenerationMode = !IsImageGenerationMode;
    }

    [RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
    async Task SubmitInputText(CancellationToken token)
    {
        var inputText = InputText;
        var isImageGenerationMode = IsImageGenerationMode;

        CanSubmitInputTextExecute = false;

        InputText = string.Empty;

        ConversationHistory.Add(new ChatModel(inputText, ChatRole.User));

        var assistantChatMode = new ChatModel(string.Empty, ChatRole.Assistant);
        ConversationHistory.Add(assistantChatMode);

        try
        {
            if (isImageGenerationMode)
            {
                throw new NotImplementedException();
            }
            else
            {

                var options = new ChatOptions
                {
                    Tools = [
                        AIFunctionFactory.Create(_gitHubServices.GetIgorUserName),
                        AIFunctionFactory.Create(_gitHubServices.GetRepositoryCount),
                        AIFunctionFactory.Create(_gitHubServices.GetUserBio)
                        ]
                };
                
                var pdfContext = await _pdfIngestionService.SearchAsync(inputText, token);

                var prompt = pdfContext is null ? inputText :
                    $@"Use the following context from the ingested documents to answer the question.
                        If the context does not contain the answer, say so.
                    
                        Context: {pdfContext}

                        Question: {inputText}
                    ";

                await foreach (var response in _chatClientServices.GetStreamingResponseAsync(new ChatMessage(ChatRole.User, inputText), options, token))
                {
                    assistantChatMode.Text += response.Text;
                }

                _chatClientServices.AddAssistantResponse(assistantChatMode.Text);
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }
        finally
        {
            CanSubmitInputTextExecute = true;
        }
    }
}