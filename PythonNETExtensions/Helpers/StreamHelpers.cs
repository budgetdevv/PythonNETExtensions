using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace PythonNETExtensions.Helpers
{
    public static class StreamHelpers
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);
            ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);
            
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
        
        public static async Task UnzipAsync(this Stream source, string destinationDirectory)
        {
            await Task.Yield();
            
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destinationDirectory);
            
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            
            ZipFile.ExtractToDirectory(source, destinationDirectory);
        }
    }
}