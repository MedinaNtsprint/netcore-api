using System;
using System.IO;
using System.Threading.Tasks;
using APICore.Services;

namespace APICore.Services.Impls
{
    /// <summary>
    /// No-op fallback storage service used when no Azure connection string is configured.
    /// This prevents runtime exceptions in environments where blob storage is optional.
    /// </summary>
    public class NoOpStorageService : IStorageService
    {
        public Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName)
        {
            // Return a dummy URI so callers can continue to operate in dev/test environments
            return Task.FromResult(new Uri("about:blank"));
        }

        public Task DeleteFileBlobAsync(string blobContainerName, string fileName)
        {
            // Nothing to delete in the no-op implementation
            return Task.CompletedTask;
        }
    }
}
