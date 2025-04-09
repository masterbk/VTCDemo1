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
            stream.Position = 0;
            var memoryStream = new MemoryStream();

            var arguments = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(stream))
                .OutputToPipe(new StreamPipeSink(memoryStream), options => options
                    .WithVideoCodec("libwebp")
                    .ForceFormat("webp")
                    .WithFastStart());

            await arguments.ProcessAsynchronously();
            memoryStream.Position = 0;

            return memoryStream;
        }

        public static async Task<MemoryStream> ConvertVideoToMp4Async(Stream stream)
        {
            stream.Position = 0;

            //var arguments = FFMpegArguments
            //    .FromPipeInput(new StreamPipeSource(stream))
            //    .OutputToPipe(new StreamPipeSink(memoryStream), options => options
            //        .WithVideoCodec("libx264")
            //        .WithAudioCodec("aac")
            //        .ForceFormat("mp4"));

            if (!Directory.Exists("./tmp"))
            {
                Directory.CreateDirectory("./tmp");
            }

            var outputFileName = $"./tmp/{Guid.NewGuid().ToString()}.mp4";

            var arguments = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(stream))
                .OutputToFile(outputFileName, true, options => options
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac")
                    .ForceFormat("mp4"));

            await arguments.ProcessAsynchronously();

            var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(outputFileName));
            File.Delete(outputFileName);

            return memoryStream;
        }
    }
}
