
using Demo1.Dto.Enums;
using Demo1.Dto.Options;
using Demo1.Service;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Storage.V1;
using FFMpegCore;
using FFMpegCore.Pipes;
using System.Runtime.InteropServices;

namespace Demo1.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            const string root = "Configs";
            builder.Configuration.AddJsonFile($"{root}/appsettings.json", false, true);
            builder.Configuration.AddJsonFile($"{root}/appsettings.{enviroment}.json", true, true);
            builder.Configuration.AddEnvironmentVariables();

            var gcpOption = new GCPOption();
            builder.Configuration.GetSection(nameof(GCPOption)).Bind(gcpOption);
            builder.Services.AddSingleton(gcpOption);

            var googleCredential = GoogleCredential.FromFile(gcpOption.CredentialFile);

            builder.Services.AddSingleton(provider =>
            {
                var credential = googleCredential
                    .CreateScoped(new[]
                    {
                        GoogleScopes.SheetsReadOnly,
                        GoogleScopes.DriveReadOnly,
                        GoogleScopes.StorageFullControl,
                        GoogleScopes.CloudPlatform
                    });

                return credential;
            });

            builder.Services.AddSingleton(provider =>
            {
                var credential = provider.GetRequiredService<GoogleCredential>();

                return new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MyApp Google Sheets"
                });
            });

            builder.Services.AddSingleton(provider =>
            {
                var credential = provider.GetRequiredService<GoogleCredential>();

                return new DriveService(new Google.Apis.Services.BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "DriveToGCSUploader"
                });
            });

            FirebaseApp.Create(new AppOptions
            {
                Credential = googleCredential
            });

            var firestoreClientBuilder = new FirestoreClientBuilder
            {
                Credential = googleCredential
            };
            var firestoreClient = firestoreClientBuilder.Build();
            var firestoreDb = FirestoreDb.Create(gcpOption.FirebaseProjectID, firestoreClient);
            builder.Services.AddSingleton(firestoreDb);

            var storageClient = StorageClient.Create(googleCredential);
            builder.Services.AddSingleton(storageClient);

            builder.Services.AddSingleton<GoogleSheetService>();
            builder.Services.AddSingleton<GoogleDriveService>();
            builder.Services.AddSingleton<GoogleStorageService>();
            builder.Services.AddSingleton<FirebaseService>();

            // Cấu hình đường dẫn đến ffmpeg nếu cần (nếu không nằm trong PATH)
            var binaryPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
             ? "./ffmpeg/bin"
             : "/app/tools/ffmpeg";

            GlobalFFOptions.Configure(new FFOptions
            {
                BinaryFolder = binaryPath
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
