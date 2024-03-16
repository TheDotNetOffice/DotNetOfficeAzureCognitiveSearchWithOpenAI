namespace DotNetOfficeAzureApp.Services
{
    public interface IAzureBlobStorageService
    {
        List<string> GetBlobFileNames();
        string UploadFile(IFormFile formFile);
        bool deleteBlobName(string blobName);
    }
}
