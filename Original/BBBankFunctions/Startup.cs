using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(BBBankFunctions.Startup))]
namespace BBBankFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped(x => new BlobServiceClient(Environment.GetEnvironmentVariable("BlobConnection")));
            builder.Services.AddScoped<IBlobService, BlobService>();
        }
    }
}
