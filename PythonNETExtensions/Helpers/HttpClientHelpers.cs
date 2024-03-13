using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PythonNETExtensions.Helpers
{
    public static class HttpClientHelpers
    {
        public static async Task DownloadFileAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            // Get the http headers first to examine the content length
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            var contentLength = response.Content.Headers.ContentLength;

            await using var download = await response.Content.ReadAsStreamAsync(cancellationToken);
            // Ignore progress reporting when no progress reporter was 
            // passed or when the content length is unknown
            if (progress == null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
            }

            else
            {
                const int BUFFER_SIZE = 81920;
                
                // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                var relativeProgress = new Progress<long>(totalBytes => progress.Report((float) totalBytes / contentLength.Value));
                // Use extension method to report progress while downloading
                await download.CopyToAsync(destination, BUFFER_SIZE, relativeProgress, cancellationToken);
                progress.Report(1);
            }
        }
    }
}