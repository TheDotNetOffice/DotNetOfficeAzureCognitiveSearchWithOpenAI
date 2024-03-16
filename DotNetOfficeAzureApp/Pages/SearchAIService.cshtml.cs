using DotNetOfficeAzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace DotNetOfficeAzureApp.Pages
{
    public class SearchAIService : PageModel
    {
        private readonly IAzureAISearchService _service;
        private readonly ILogger<SearchAIService> _searchLogger;

        public string Output { get; set; } = "";

        public SearchAIService(IAzureAISearchService service, ILogger<SearchAIService> searchLogger)
        {
            _service = service;
            this._searchLogger = searchLogger;
        }

        public void OnGet()
        {
        }

        public async void OnPost(string Searchinput)
        {
            try
            {

                //var response = _service.SearchResultByAIService(Searchinput);
                var response = _service.SearchResultByOpenAI(Searchinput);
                Output = response.Result.Value.Choices[0].Message.Content;

            }
            catch(Exception ex) 
            { 
            }
        }
    }
}
