using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace DeliveryService.Helpers
{
    public class FirebaseStorageHelper
    {
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