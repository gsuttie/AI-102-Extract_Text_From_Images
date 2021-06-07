using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extract_Text_From_Images
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string endpoint = configuration["CognitiveServicesEndpoint"];
            string subscriptionKey = configuration["CognitiveServiceKey"];

            // Set console encoding to unicode
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

            var READ_TEXT_URL_IMAGE = "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/ComputerVision/Images/printed_text.jpg";

            ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();
        }

        /*
          * AUTHENTICATE
          * Creates a Computer Vision client used by each example.
          */
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
            return client;
        }

        /*
         * READ FILE - URL 
         * Extracts text. 
         */
        public static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("READ FILE FROM URL");
            Console.WriteLine();

            // Read text from URL
            var textHeaders = await client.ReadAsync(urlFile);
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
            Console.WriteLine();
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            // Display the found text.
            Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }
    }
}
