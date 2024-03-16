using Azure.AI.OpenAI;
using Azure;

namespace DotNetOfficeAzureApp.Services
{
    public interface IAzureAISearchService
    {
        bool UpdateIndexer();
        AzureCustomResult SearchResultByAIService(string input);

        Task<Response<ChatCompletions>> SearchResultByOpenAI(string chatInput);
    }
}
