# Image Upload Using Azure Blob Storage

## What Azure Blob Storage ?
[Azure Blob storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction) is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data.

Blob storage is designed for:

* **Serving images** or **documents** directly to a browser.
* **Storing files** for distributed access.
* **Streaming** video and audio.
* **Writing** to log files.
* **Storing data** for **backup** and **restore, disaster recovery**, and **archiving**.
* **Storing data** for analysis by an on-premises or Azure-hosted service.

## What Azure Function ?
[Azure functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) is a serverless concept of cloud native design that allows a piece of code deployed and execute without any need of server infrastructure, web server, or any configurations.

There are a number of templates available in Microsoft Azure Functions, few are as below :

* **HTTP** - Users can run the code based on HTTP requests.
* **Timer** - Users can schedule the code to run at the predefined time.
* **Blob Storage** - You can create a function that will trigger when files are uploaded to or updated in a Blob storage container.
* **Queue Storage** - It is used to respond to the Azure Storage queue messages.
 
## About this exercise

**Frontend Code Base:**

Previously, we scaffolded a new Angular application in which we have

* FontAwesome library for icons.
* Bootstrap library for styling.



## In this exercise

 * We will create an **Storage Account** on Azure Portal.
 * We will configure **containers**  for storage account.  
 * We will create an **HTTP Azure function** in Asp.Net Core.
 * We will implement Image upload functionality in Angular.

-----------
 Here are the steps to begin with :

# Cloud Side Configuration

## Step 1: Create Azure Storage Account
To create a new storage account go to [Azure Portal](https://portal.azure.com/#home) and click **Create a resource**. search `Storage Account' and click **Create** button. 

Enter the relevant details and click **Review + Create** button as below :
![1](https://user-images.githubusercontent.com/100709775/180192138-43566992-44eb-4da1-8849-6d0eb8217d45.PNG)

--------

## Step 2:  Create Container in Storage Account
To create a new container in storage account click on **Containers** option from left menu and then click **Add container** button. Enter the name and select **Blob (anonymous read access for blobs only)** option in Public access level as below : 

-------------

![2](https://user-images.githubusercontent.com/100709775/180192855-d6d26267-0685-4bee-84fe-b4525d71e8b8.PNG)

## Step 3:  Get Storage Account Connection String

Our Azure Function will require the connection string of storage account to upload the images to get the connection string click on the **Access Keys** option from left menu and **copy** the connection string.

![3](https://user-images.githubusercontent.com/100709775/180194276-00e35d11-affe-4a4a-8327-3961e61bc9cc.PNG)

----------------
# Server Side Implementation

## Step 1: Create new Azure Function Application
To create a new Azure function create a new **Azure Function** project in Visual Studio as below : 

![4](https://user-images.githubusercontent.com/100709775/180195435-3af3a799-812f-4fc0-adcf-a39ef1bfbfa5.PNG)

Click next and then select **Http Trigger** template and click create button as below. This trigger gets fired when the HTTP request comes.

![5](https://user-images.githubusercontent.com/100709775/180195445-90aaccf9-3ff7-4082-b57a-7a6efd4fec72.PNG)


Install the required **Azure nuget** using commands in package console manager

```powershell
    Install-Package Azure.Storage.Blobs -Version 12.13.0
    Install-Package Microsoft.Azure.Functions.Extensions -Version 1.1.0
    Install-Package Microsoft.Azure.WebJobs.Extensions.Storage -Version 5.0.1
```
After getting installed. It will look like as below :  

![6](https://user-images.githubusercontent.com/100709775/180198608-43ff33b5-a5e9-4ad5-a939-f29bbe069f73.PNG)

---------------


## Step 2: Setup Blob Service Interface
Create a new interface `IBlobService` which will have two functions `GetContainerClient` which will return the `BlobContainerClient` against block container name and second function `UploadFileBlobAsync` which will upload the image stream on blob storage. 

The `IBlobService` looks likes as below :

```cs
  public interface IBlobService
    {
        Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName);
        BlobContainerClient GetContainerClient(string blobContainerName);
    }
```
------------

## Step 3: Implement Blob Service Interface

Create a new class `BlobService` and inherit it with `IBlobService` interface.
Implement `GetContainerClient` method which will get the blob container client by blob container name. We will also implement  `UploadFileBlobAsync` method that will call **GetContainerClient** and will create blob client for this container.
After creating the `blobclient` it will upload the image stream and returns the Image URI as below :  

```cs
 public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName)
        {
            // Get reference of the container 
            var containerClient = GetContainerClient(blobContainerName);
            // create a space for a file in the container.
            var blobClient = containerClient.GetBlobClient(fileName);
            // upload the bytes of the file in that space
            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
            // returns the URI  of the file create.
            return blobClient.Uri;
        }

        public BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);
            return containerClient;
        }
    }
```

## Step 4: Setup Connection String
Go to `local.settings.json` file and add the connection string copied from azure portal under values object, Also we will setup container name in this settings as well 

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
      "FUNCTIONS_WORKER_RUNTIME": "dotnet",
      "blobConnection": "ConnectionString",
      "ContainerName": "profilepic"
    }
}
```

## Step 5: Create Startup.cs and Dependency Inject Blob Service
Create a new class `Startup` and inherit it with `FunctionsStartup` class so that we can dependency inject the **BlobServiceClient** and **IBlobService**.
Also we will read the connection string from configuration file.

```cs
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
            // Dependency injection custom BlobService class that handles blob interactions. 
            builder.Services.AddScoped<IBlobService, BlobService>();
        }
    }
}
```


## Step 6: Implement Image Upload functionality in Azure Function
Go to `Function1.cs ` file and rename it to `UploadImageAndGetUrl` and renamed the function name to `UploadImageAndGetUrl`.
This class will dependency inject the `IConfiguration` for getting the container name and also will inject the `IBlobService` to upload the image stream.
In function we will get the received file from `HttpRequest` object and then will create its stream and sent it to `UploadFileBlobAsync` method to get the image uri.

Here is the code as below : 

```cs
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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,ILogger log)
        {
            string fileName = String.Empty;
            Uri uri;
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                // picking up the first file sent to the function and is accessed through request's Form object
                var file = req.Form.Files[0];
                // creating a random file name
                fileName = Guid.NewGuid().ToString() + ".jpg";
                using (var ms = new MemoryStream())
                {
                    // copying incoming bytes into stream
                    file.CopyTo(ms);
                    ms.Position = 0;
                    // sending a stream for uploading. 
                    uri = await blobService.UploadFileBlobAsync(configuration.GetValue<string>("ContainerName"), ms, "", fileName);
                }
            }
             catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
            // returning the url 
            return new OkObjectResult(new FullPathResponse() {  FullPath = uri.AbsoluteUri });

        }
    }
```

Server side is complete, run the azure function as see its working. 

------------

# Client Side Implementation

## Step 1: Setup Image Tag in HTML
Go to `app.component.html` and add `img` tag. Add a click event which will call the `fileInput.click()` of hidden input filed which will call `save` function **onChange** event.

Here is the code as below : 

```html
<div class="container-fluid">
  <app-toolbar></app-toolbar>
</div>
  <div class="row">
    <div class="col-12">
      <div class="card card-user">
        <div class="card-body">
          <div class="author">
            <div class="block block-one"></div>
            <div class="block block-two"></div>
            <div class="block block-three"></div>
            <div class="block block-four"></div>
            <input hidden #fileInput type="file" id="file" (change)="save(fileInput.files)" />
            <div class="avatar-cont">
              <img id="photo" alt="..." class="avatar"  src="{{profilePicUrl}}" />
              <a href="javascript:void(0)"(click)="fileInput.click()" class="btn-change-avatar"><i class="fas fa-camera"></i></a>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
<router-outlet></router-outlet>
```

## Step 2: Setup Environment Variable
Go to `environment.ts` and create a new variable `azureFunctionsBaseUrl` which will contain the Azure function URL.

```ts
export const environment = {
  production: false,
  apiUrlBase: 'http://localhost:5070/api/',
  azureFunctionsBaseUrl: 'http://localhost:7071/api/'
};
```

## Step 2: Setup Azure Access Service
Create a new file named `azureAccess.service.ts` and add  `uploadImageAndGetUrl` function which will send the POST request to Azure Function.

Here is the code as below :

```ts
@Injectable({
  providedIn: 'root',
})
export default class AzureAccessService {
  constructor(private httpClient: HttpClient) { }

  uploadImageAndGetUrl(imageData: FormData): Observable<UploadImageResponse> {
    return this.httpClient.post<UploadImageResponse>(`${environment.azureFunctionsBaseUrl}UploadImageAndGetUrl`, imageData);
  }
}
```

## Step 3: Implementing Image Upload 
Go to `app.component.ts` and inject the `AzureAccessService` in constructor.
Create `save` function will be triggered on `Change` event of Input field.
This function will call the `uploadImageAndGetUrl` method of AzureAccessService to upload the image set the response in `profilePicUrl` local variable.

Here is the code as below : 

```ts
constructor(private transactionService: TransactionService,private azureAccessService: AzureAccessService) {}

save(files:any) {
    const formData = new FormData();

    if (files[0]) {
      formData.append(files[0].name, files[0]);
    }
    this.azureAccessService
    .uploadImageAndGetUrl(formData)
    .subscribe({
      next: (data) => {
        this.profilePicUrl = data['fullPath'];
      },
      error: (error) => {
        console.log(error);
      },
    });
  }
```

## Step 4: Setup styling 
Go to `app.component.css` and add the following **css** for styling.

```css
.card {
  position: relative;
  display: flex;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #ffffff;
  background-clip: border-box;
  border: 0.0625rem solid rgba(34, 42, 66, 0.05);
  border-radius: 0.2857rem;
}
.card {
  background: #27293d;
  border: 0;
  position: relative;
  width: 100%;
  margin-bottom: 30px;
  box-shadow: 0 1px 20px 0px rgba(0, 0, 0, 0.1);
}
.card .card-body {
  padding: 15px;
}
.card .card-body .card-description {
  color: rgba(255, 255, 255, 0.6);
}
.card .avatar {
  width: 30px;
  height: 30px;
  overflow: hidden;
  border-radius: 50%;
  margin-bottom: 15px;
}
.card-user {
  overflow: hidden;
}
.card-user .author {
  text-align: center;
  text-transform: none;
  margin-top: 25px;
}
.card-user .author a+p.description {
  margin-top: -7px;
}
.card-user .author .block {
  position: absolute;
  height: 100px;
  width: 250px;
}
.card-user .author .block.block-one {
  background: rgba(225, 78, 202, 0.6);
  background: -webkit-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -o-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -moz-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  filter: progid:DXImageTransform.Microsoft.BasicImage(rotation=10);
  -webkit-transform: rotate(150deg);
  -moz-transform: rotate(150deg);
  -ms-transform: rotate(150deg);
  -o-transform: rotate(150deg);
  transform: rotate(150deg);
  margin-top: -90px;
  margin-left: -50px;
}
.card-user .author .block.block-two {
  background: rgba(225, 78, 202, 0.6);
  background: -webkit-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -o-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -moz-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  filter: progid:DXImageTransform.Microsoft.BasicImage(rotation=10);
  -webkit-transform: rotate(30deg);
  -moz-transform: rotate(30deg);
  -ms-transform: rotate(30deg);
  -o-transform: rotate(30deg);
  transform: rotate(30deg);
  margin-top: -40px;
  margin-left: -100px;
}
.card-user .author .block.block-three {
  background: rgba(225, 78, 202, 0.6);
  background: -webkit-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -o-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -moz-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  filter: progid:DXImageTransform.Microsoft.BasicImage(rotation=10);
  -webkit-transform: rotate(170deg);
  -moz-transform: rotate(170deg);
  -ms-transform: rotate(170deg);
  -o-transform: rotate(170deg);
  transform: rotate(170deg);
  margin-top: -70px;
  right: -45px;
}
.card-user .author .block.block-four {
  background: rgba(225, 78, 202, 0.6);
  background: -webkit-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -o-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: -moz-linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  background: linear-gradient(to right, rgba(225, 78, 202, 0.6) 0%, rgba(225, 78, 202, 0) 100%);
  filter: progid:DXImageTransform.Microsoft.BasicImage(rotation=10);
  -webkit-transform: rotate(150deg);
  -moz-transform: rotate(150deg);
  -ms-transform: rotate(150deg);
  -o-transform: rotate(150deg);
  transform: rotate(150deg);
  margin-top: -25px;
  right: -45px;
}
.card-user .avatar {
  width: 124px;
  height: 124px;
  border: 5px solid #2b3553;
  border-bottom-color: transparent;
  background-color: transparent;
  position: relative;
}
.card-user .card-body {
  min-height: 240px;
}
.card-user .card-description {
  margin-top: 30px;
}
.card .amount-cont {
  color: rgba(255, 255, 255, 0.6);
  font-size: .875rem;
  font-weight: 300;
}
.form-control {
  background-color: #27293d;
  font-size: 1.125rem;
  font-weight: 300;
  color: #fff;
  border: 1px solid #e14eca;
  margin-bottom: 1rem;
}


.avatar-cont {
  position: relative;
  width: 124px;
  height: 124px;
  margin: 0 auto;
  margin-bottom: 1px;
}
a.btn-change-avatar {
  display: block;
  width: 40px;
  height: 40px;
  border-radius: 5px;
  color: #fff;
  background: #e14eca;
  font-size: 1.25rem;
  line-height: 40px;
  position: absolute;
  bottom: 0;
  right: 0;
  transition: all .3s ease 0s;
}
```

# Final Output
Run the application and see its working as below :

![Image-Upload-video](https://user-images.githubusercontent.com/100709775/182832665-bc2fe831-83f7-4738-815c-f063651bc4b2.gif)
