using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition
{
    class Program
    {

        public const string credentialFile = "ImageRecognition-0d0e795f6b75.json";
        public const string projectId = "imagerecognition-181213";

        static void Main(string[] args)
        {
			System.IO.Directory.CreateDirectory(@"c:\image\");

			FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"c:\image\";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(FileAdded);
            watcher.EnableRaisingEvents = true;


            while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        }

        private static void FileAdded(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(" *** New Image ***");
            //Authentication stuff...
            GoogleCredential credential = null;
            using (var jsonStream = new FileStream(credentialFile, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                credential = GoogleCredential.FromStream(jsonStream);
            }
            var channel = new Grpc.Core.Channel(ImageAnnotatorClient.DefaultEndpoint.Host, credential.ToChannelCredentials());


            //Create client
            var client = ImageAnnotatorClient.Create(channel);

            var image = Image.FromFile(e.FullPath);

            //Labeldetection
            var response = client.DetectLabels(image);
            
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                    Console.WriteLine(string.Format("{0:0.00}", annotation.Score) + "% " + annotation.Description);
            }
            Console.WriteLine(" *****************");
        }


        


    }
}
