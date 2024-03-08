using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PythonNETExtensions
{
    public static class DownloadHelpers
    {
        private const int BUFFER_SIZE = 81920;
        
        public static async Task DownloadWithProgressAsync(this HttpClient client, string url, Stream destination)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            
            response.EnsureSuccessStatusCode();

            await using var download = await response.Content.ReadAsStreamAsync();

            await download.CopyToAsync(destination, BUFFER_SIZE);
        }
    }
}