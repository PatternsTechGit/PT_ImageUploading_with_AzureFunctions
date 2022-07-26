﻿using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// marking this file as a startup file
[assembly: FunctionsStartup(typeof(BBBankFunctions.Startup))]
namespace BBBankFunctions
{
    //FunctionsStartup is part of Dependency Injection  Nuget
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Dependency Injecting BlobServiceClient
            builder.Services.AddScoped(x => new BlobServiceClient(Environment.GetEnvironmentVariable("BlobConnection")));
            // Dependency injection custom BlobService class that handels blob interactions. 
            builder.Services.AddScoped<IBlobService, BlobService>();
        }
    }
}
