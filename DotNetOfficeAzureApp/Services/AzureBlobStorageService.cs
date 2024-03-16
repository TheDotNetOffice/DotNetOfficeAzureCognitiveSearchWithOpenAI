using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace DotNetOfficeAzureApp.Services
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {

        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _configStorage;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            this._configuration = configuration;
            _configStorage = _configuration.GetSection("Storage");
        }

        public List<string> GetBlobFileNames()
        {
            List<string> lstFileNames = new List<string>();

            try
            {

                string containerName = _configStorage.GetValue<string>("Container");
                string connectionString = _configStorage.GetValue<string>("connectionString");

                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobs = blobContainerClient.GetBlobs();
                foreach (var blob in blobs)
                {
                    lstFileNames.Add(blob.Name);
                }
            }
            catch (Exception ex) { }
            return lstFileNames;
        }

        public string UploadFile(IFormFile formFile)
        {
            string containerName = _configStorage.GetValue<string>("Container");
            //var blobServiceClient = GetBlobServiceClient();

            string connectionString = _configStorage.GetValue<string>("connectionString");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(formFile.FileName);

            using (Stream stream = formFile.OpenReadStream())
            {
                blobClient.Upload(stream, true);
            }

            return formFile.FileName;
        }
        public bool deleteBlobName(string blobName)
        {
            try
            {

                string containerName = _configStorage.GetValue<string>("Container");
                string connectionString = _configStorage.GetValue<string>("connectionString");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString); //GetBlobServiceClient();

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                blobContainerClient.DeleteBlobIfExistsAsync(blobName);
                return true;
            }
            catch (Exception ex) { return false; }
        }

    }
}
