using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBBankFunctions
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName)
        {
            // Get refrence of the container 
            var containerClient = GetContainerClient(blobContainerName);
            // create a space for a file in the container.
            var blobClient = containerClient.GetBlobClient(fileName);
            // upload the bytes of the file in that space
            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
            // retutrns the URI  of the file create.
            return blobClient.Uri;
        }

        public BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);
            return containerClient;
        }
    }
}
