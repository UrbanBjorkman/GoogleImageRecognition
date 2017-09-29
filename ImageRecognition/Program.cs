using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using System;
using System.IO;
using System.Linq;

namespace ImageRecognition
{
	class Program
    {

        public const string credentialFile = "ImageRecognition-0d0e795f6b75.json";
        public const string projectId = "imagerecognition-181213";

        static void Main(string[] args)
        {
			// Create directory
			Directory.CreateDirectory(@"c:\image\");


			// Setup watcher for image directory
			FileSystemWatcher watcher = new FileSystemWatcher()
			{
				Path = @"c:\image\",
				NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName,
				Filter = "*.*"
			};
			watcher.Created += new FileSystemEventHandler(FileAdded);
            watcher.EnableRaisingEvents = true;


			// Keep looping until escapekey
            while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        }

        private static void FileAdded(object sender, FileSystemEventArgs e)
        {
			Console.Clear();
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
            var labelResponse = client.DetectLabels(image);
            Console.WriteLine(" *** Labels ***");

			foreach (var annotation in labelResponse)
            {
                if (annotation.Description != null)
                    Console.WriteLine(string.Format("{0:0.00}", annotation.Score) + "% " + annotation.Description);
            }
            Console.WriteLine(" *****************");
            

			//Textdetection
			var textResponse = client.DetectText(image);

			if (textResponse.Any())
			{
				Console.WriteLine(" ");
				Console.WriteLine(" *** Texts ***");
				foreach (var annotation in textResponse)
				{
					if (annotation.Description != null)
						Console.WriteLine(annotation.Description);
				}
				Console.WriteLine(" *****************");
			}
			
			Console.WriteLine(" ");



			//Logodetection
			var logoResponse = client.DetectLogos(image);

			if (logoResponse.Any())
			{
				Console.WriteLine(" ");
				Console.WriteLine(" *** Logos ***");
				foreach (var annotation in logoResponse)
				{
					if (annotation.Description != null)
					{
						Console.WriteLine(annotation.Description);
						Console.WriteLine("   Bounding polygon: "  + annotation.BoundingPoly);
					}
				}
				Console.WriteLine(" *****************");
			}

			Console.WriteLine(" ");



			//Facedetectiono
			var faceResponse = client.DetectFaces(image);

			if (faceResponse.Any())
			{
				Console.WriteLine(" ");
				Console.WriteLine(" *** Faces ***");
				var count = 1;
				foreach (var faceAnnotation in faceResponse)
				{

						Console.WriteLine("Face {0}:", count++);
						Console.WriteLine("  Joy: {0}", faceAnnotation.JoyLikelihood);
						Console.WriteLine("  Anger: {0}", faceAnnotation.AngerLikelihood);
						Console.WriteLine("  Sorrow: {0}", faceAnnotation.SorrowLikelihood);
						Console.WriteLine("  Surprise: {0}", faceAnnotation.SurpriseLikelihood);

				}
				Console.WriteLine(" *****************");
			}

			Console.WriteLine(" ");


		}


        


    }
}
