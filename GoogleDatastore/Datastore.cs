using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace GoogleCloudSamples
{
    public class Datastore
    {
        private const string projectId = "Update_projectid";
        private const string bucketName = "Update_bucketname";

        public IConfigurableHttpClientInitializer GetApplicationDefaultCredentials()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());

            var docPath = HttpContext.Current.Server.MapPath("Update_credential_path");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", docPath, EnvironmentVariableTarget.Process);

            try
            {
                GoogleCredential credential =
                    GoogleCredential.GetApplicationDefaultAsync().Result;
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(new[] {
                    StorageService.Scope.DevstorageReadWrite
                });
                }
                return credential;
            }
            catch (AggregateException exception)
            {
                throw new Exception(String.Join("\n", exception.Flatten().InnerExceptions.Select(t => t.Message)));
            }
        }

        public async Task<bool> UploadImageAsync(string filename, string imageUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    byte[] data = webClient.DownloadData(imageUrl);
                    MemoryStream mem = new MemoryStream(data);
                    return await UploadAsync(filename, mem, "image/jpeg", "images");
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public async Task<bool> UploadUserContactsAsync(string filename, string data)
        {
            MemoryStream mem = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(mem))
            {
                writer.Write(data);
                writer.Flush();
                mem.Position = 0;
                return await UploadAsync(filename, mem, "text/plain", "contacts");
            }
        }

        public async Task<bool> UploadStorageInfoAsync(string filename, string data)
        {
            MemoryStream mem = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(mem))
            {
                writer.Write(data);
                writer.Flush();
                mem.Position = 0;
                return await UploadAsync(filename, mem, "text/plain", "storage");
            }
        }

        public async Task<bool> UploadAsync(string filename, MemoryStream mem, string datatype, string dirName)
        {
            IConfigurableHttpClientInitializer credentials = GetApplicationDefaultCredentials();
            StorageService service = new StorageService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = "YellowBook",
                });

            // To make public
            var acl = new List<ObjectAccessControl>
                {
                    new ObjectAccessControl
                    {
                        Role = "OWNER",
                        Entity = "allUsers"
                    }
                };

            filename = String.Format("{0}/{1}", dirName, filename);
            var fileobj = new Google.Apis.Storage.v1.Data.Object() { Name = filename, Acl = acl };
            await service.Objects.Insert(fileobj, bucketName, mem, datatype).UploadAsync();
            return true;
        }

        public async Task<bool> FileExistsAsync(string filename, string dirName)
        {
            try
            {
                IConfigurableHttpClientInitializer credentials = GetApplicationDefaultCredentials();
                StorageService storageService = new StorageService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = "YellowBook",
                });
                var request = storageService.Objects.List(bucketName);
                var children = await request.ExecuteAsync();
                var objectName = String.Format("{0}/{1}", dirName, filename);
                return children.Items.Any(c => c.Name == objectName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }
}
