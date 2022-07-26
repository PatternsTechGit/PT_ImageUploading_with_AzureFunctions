using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BBBankFunctions
{
    public class UploadImageAndGetUrl
    {
        IBlobService blobService;
        IConfiguration configuration;
        public UploadImageAndGetUrl(IConfiguration configuration, IBlobService blobService)
        {
            this.configuration = configuration;
            this.blobService = blobService;
        }
        [FunctionName("UploadImageAndGetUrl")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            //[Blob("profilepics/{rand-guid}.json", FileAccess.ReadWrite, Connection = "BlobConnection")] CloudBlockBlob outputBlob,
            //string filename,
            ILogger log)
        {
            string fileName = String.Empty;
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                var file = req.Form.Files[0];
                fileName = Guid.NewGuid().ToString() + ".jpg";
                using (var ms = new MemoryStream())
                {

                    file.CopyTo(ms);
                    ms.Position = 0;
                    await blobService.UploadFileBlobAsync(configuration.GetValue<string>("ContainerName"), ms, "", fileName);
                }
            }
             catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
            String fullPath = "https://bbbank.blob.core.windows.net/profilepics/" + fileName;
            return new OkObjectResult(new FullPathResponse() {  FullPath = fullPath });

        }
    }
}
