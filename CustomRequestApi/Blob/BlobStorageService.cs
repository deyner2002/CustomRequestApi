using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace CustomRequestApi.Blob
{
    public interface IBlobStorageService
    {
        string AddBlob(MemoryStream stream, string attachName, BlobContainerClient containerClient);
        BlobContainerClient GetContainer(string containerName);
    }
    public class BlobStorageService : IBlobStorageService
    {
        public readonly ConnectionStringsBlob _ConnectionStringsBlob;

        public BlobStorageService(IOptions<ConnectionStringsBlob> connectionStringsBlob)
        {
            _ConnectionStringsBlob = connectionStringsBlob.Value;
        }


        public BlobContainerClient GetContainer(string containerName)
        {
            string connectionString = _ConnectionStringsBlob.AzureBlobConnection;

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            if (!containerClient.Exists())
            {
                containerClient.CreateIfNotExists(publicAccessType: PublicAccessType.BlobContainer);
            }

            return containerClient;
        }

        public string AddBlob(MemoryStream stream, string attachName, BlobContainerClient containerClient)
        {
            BlobClient blobClientCertificadoExistencia = containerClient.GetBlobClient(attachName);

            blobClientCertificadoExistencia.Upload(stream, true);

            return blobClientCertificadoExistencia.Uri.ToString();
        }
    }
}
