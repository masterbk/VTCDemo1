using FFMpegCore;
using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Helper
{
    public static class MediaExtensions
    {
        public static string GetFileNameWithoutExtension(this string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        public static bool IsImage(this string mimeType)
        {
            return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVideo(this string mimeType)
        {
            return mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAudio(this string mimeType)
        {
            return mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDocument(this string mimeType)
        {
            return mimeType.StartsWith("application/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsImageWebp(this string mimeType)
        {
            return mimeType.IsImage() && mimeType.EndsWith("webp", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVideoMp4(this string mimeType)
        {
            return mimeType.IsVideo() && mimeType.EndsWith("mp4", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<MemoryStream> ConvertImageToWebpAsync(Stream stream)
        {
            var memoryStream = new MemoryStream();

            var arguments = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(stream))
                .OutputToPipe(new StreamPipeSink(memoryStream), options => options
                    .WithVideoCodec("libwebp")
                    .ForceFormat("webp")
                    .WithFastStart());

            await arguments.ProcessAsynchronously();

            return memoryStream;
        }

        public static async Task<MemoryStream> ConvertVideoToMp4Async(Stream stream)
        {
            var memoryStream = new MemoryStream();

            var arguments = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(stream))
                .OutputToPipe(new StreamPipeSink(memoryStream), options => options
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac")
                    .ForceFormat("mp4")
                    .WithFastStart());

            await arguments.ProcessAsynchronously();

            return memoryStream;
        }
    }
}
