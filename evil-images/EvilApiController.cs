using System;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.Azure; 
using Microsoft.WindowsAzure.Storage;

namespace evil_images.Controllers
{
    [RoutePrefix("")]
    public class EvilApiController : ApiController
    {
        [Route("")]
        [HttpGet]
        public async Task<string> Welcome()
        {
            return "You are heading for the evilest API!";
        }

        [Route("image")]
        [HttpPost]
        public async Task<string> UploadImage()
        {
            var token = CloudConfigurationManager.GetSetting("Token");
            if (Request.Headers.Authorization == null || !Request.Headers.Authorization.ToString().Contains(token))
                return null;

            var connection = CloudConfigurationManager.GetSetting("BlobConnectionString");
            var buffer = await Request.Content.ReadAsByteArrayAsync();

            var storageAccount = CloudStorageAccount.Parse(connection);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("images");
            container.CreateIfNotExists();

            var filename = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            var blockBlob = container.GetBlockBlobReference(filename);
            blockBlob.UploadFromByteArray(buffer, 0, buffer.Length);
            
            return filename;
        }

        [Route("metadata/{filename}")]
        [HttpPost]
        public async Task<bool> UploadMetadata(string filename)
        {
            var token = CloudConfigurationManager.GetSetting("Token");
            if (Request.Headers.Authorization == null || !Request.Headers.Authorization.ToString().Contains(token))
                return false;

            var connection = CloudConfigurationManager.GetSetting("BlobConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connection);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("metadata");
            container.CreateIfNotExists();

            var buffer = await Request.Content.ReadAsByteArrayAsync();
            var blockBlob = container.GetBlockBlobReference(filename);
            blockBlob.UploadFromByteArray(buffer, 0, buffer.Length);

            return true;
        }
    }
}
