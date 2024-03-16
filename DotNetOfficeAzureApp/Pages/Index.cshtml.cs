using DotNetOfficeAzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotNetOfficeAzureApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        IAzureBlobStorageService _service;
        private readonly IAzureAISearchService _azureAIService;
        public List<string> blobFileNames { get; set; } = new List<string>();
        public IndexModel(ILogger<IndexModel> logger, IAzureBlobStorageService service, IAzureAISearchService azureAIService)
        {
            _logger = logger;
            _service = service;
            _azureAIService = azureAIService;
        }

        public void OnGet()
        {
            blobFileNames = _service.GetBlobFileNames();
        }

        public IActionResult OnPost(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return RedirectToPage();
                }

                string uploadedFileName = _service.UploadFile(file);
                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    bool isIndexerSuccess = _azureAIService.UpdateIndexer();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: ex.Message);
            }
            return RedirectToPage();

        }
        public IActionResult OnPostDelete(string blobName)
        {
            _service.deleteBlobName(blobName);
            return RedirectToPage();
        }
    }
}