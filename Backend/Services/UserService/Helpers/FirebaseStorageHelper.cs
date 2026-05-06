using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Net.Http;

namespace UserService.Helpers
{
    public class FirebaseStorageHelper
    {
        //private readonly string _bucketName = "your-project-id.appspot.com"; // Tên bucket của bạn
        //private readonly string _credentialPath = "path/to/firebase-auth.json"; // Đường dẫn file JSON ở Bước 1
        private readonly IConfiguration _configuration;

        public FirebaseStorageHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateUploadUrl(string objectName, string contentType)
        {
            var credential = GoogleCredential.GetApplicationDefault();

            var urlSigner = UrlSigner.FromCredential(credential);

            var options = UrlSigner.Options.FromDuration(TimeSpan.FromMinutes(15));

            var requestTemplate = UrlSigner.RequestTemplate
                .FromBucket(_configuration.GetValue<string>("BUCKET_NAME"))
                .WithObjectName(objectName)
                .WithHttpMethod(HttpMethod.Put)
                .WithContentHeaders(new Dictionary<string, IEnumerable<string>>
                {
                    { "Content-Type", new[] { contentType } }
                });

            return urlSigner.Sign(requestTemplate, options);
        }
    }
}
