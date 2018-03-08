using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace json_test
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var credentials = new StoredProfileAWSCredentials("LinkLiang");
            using (var client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast1))
            {
                var request = new InvokeRequest
                {
                    FunctionName = "PDF2Raw"
                };
                var response = await client.InvokeAsync(request);

                string result;
                using (var sr = new StreamReader(response.Payload))
                {
                    //Put all tests below
                    result = sr.ReadToEnd();
                }
            }
            Console.WriteLine("Hello World!");
        }
    }
}
