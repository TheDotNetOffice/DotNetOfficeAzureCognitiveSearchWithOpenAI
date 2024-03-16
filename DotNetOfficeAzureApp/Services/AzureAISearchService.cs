using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System.Text;
using Azure.AI.OpenAI;

namespace DotNetOfficeAzureApp.Services
{
    public class AzureAISearchService : IAzureAISearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAISearchService> _logger;

        private readonly string _azureOpenAIApiBase = string.Empty;
        private readonly string _azureOpenAIKey = string.Empty;
        private readonly string _azuresearchServiceEndpoint = string.Empty;
        private readonly string _azuresearchIndexName = string.Empty;
        private readonly string _azuresearchApiKey = string.Empty;
        private readonly string _azureOpenAIDeploymentId = string.Empty;
        private readonly string _azurequeryKey = string.Empty;

        private readonly OpenAIClient _client;
        private ChatCompletionsOptions _options;
        public AzureAISearchService(IConfiguration configuration, ILogger<AzureAISearchService> logger)
        {
            this._configuration = configuration;
            this._logger = logger;


            _azuresearchServiceEndpoint = _configuration.GetValue<string>("AISearchServiceEndpoint");
            _azuresearchIndexName = _configuration.GetValue<string>("AISearchIndexName");
            _azuresearchApiKey = _configuration.GetValue<string>("AISearchApiKey");
            _azurequeryKey = _configuration.GetValue<string>("QueryKey");

            _azureOpenAIApiBase = _configuration.GetValue<string>("AzOpenAIApiBase");
            _azureOpenAIKey = _configuration.GetValue<string>("AzOpenAIKey");
            _azureOpenAIDeploymentId = _configuration.GetValue<string>("AzOpenAIDeploymentId");

            _client = new OpenAIClient(new Uri(_azureOpenAIApiBase), new AzureKeyCredential(_azureOpenAIKey));
            CreateChatCompletionOptions();
        }

        public bool UpdateIndexer()
        {
            try
            {
                string iName = _configuration["AISearchIndexerName"];

                SearchIndexerClient indexerClient = new SearchIndexerClient(
                                    new Uri(_configuration["AISearchServiceEndpoint"]),
                                    new AzureKeyCredential(_configuration["AISearchApiKey"]));

                Response response = indexerClient.RunIndexer(iName);
                if (response != null)
                {
                    if (response.Status == 202)
                    {
                        Thread.Sleep(5000);

                        if (IndexerStatus(indexerClient, iName))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: ex.InnerException?.Message);
                throw;
            }
        }

        private bool IndexerStatus(SearchIndexerClient client, string iName)
        {
            bool isIndexerUpdated = false;
            SearchIndexerStatus execInfo = client.GetIndexerStatus(iName);
            IndexerExecutionResult result = execInfo.LastResult;

            if (result.ErrorMessage != null)
            {
                _logger.LogError($"Error occured while updating Indexer. Error = {result.ErrorMessage}");
            }
            else
            {
                _logger.LogInformation("Indexer updated Success");
                isIndexerUpdated = true;
            }
            return isIndexerUpdated;
        }

        public AzureCustomResult SearchResultByAIService(string input)
        {
            AzureCustomResult output = new AzureCustomResult();
            AzureKeyCredential cred = new AzureKeyCredential(_azurequeryKey);
            var client = new SearchIndexClient(new Uri(_azuresearchServiceEndpoint), cred);
            var searchClient = client.GetSearchClient(_azuresearchIndexName);

            var response = searchClient.Search<AzureCustomResult>(input).Value;
            StringBuilder sb = new StringBuilder();
            foreach (SearchResult<AzureCustomResult> result in response.GetResults())
            {
                output.content = result.Document.content;
                output.metadata_storage_path = result.Document.metadata_storage_path;

                output.people = result.Document.people;
                output.organizations = result.Document.organizations;
                output.locations = result.Document.locations;



            }

            return output;
        }

        public async Task<Response<ChatCompletions>> SearchResultByOpenAI(string chatInput)
        {

            List<ChatMessage> messages = new List<ChatMessage>()
            {
                new ChatMessage(ChatRole.User, chatInput)
            };

            InitializeMessages(messages);
            var result = await _client.GetChatCompletionsAsync(_azureOpenAIDeploymentId, _options);
            return result;
        }

        private void CreateChatCompletionOptions()
        {
            _options = new ChatCompletionsOptions()
            {
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                    {
                        new AzureCognitiveSearchChatExtensionConfiguration()
                        {
                            SearchEndpoint = new Uri(_azuresearchServiceEndpoint),
                            IndexName = _azuresearchIndexName,
                            SearchKey = new AzureKeyCredential(_azuresearchApiKey),

                        }
                    },

                },

            };

        }

        private void InitializeMessages(List<ChatMessage> chatMessages)
        {
            foreach (var chatMessage in chatMessages)
            {
                _options.Messages.Add(chatMessage);
            }
        }
    }

    public class AzureCustomResult
    {
        public string content { get; set; }
        public string metadata_storage_path { get; set; }

        public List<string> people { get; set; }

        public List<string> organizations { get; set; }

        public List<string> locations { get; set; }
        public string language { get; set; }

    }
}
