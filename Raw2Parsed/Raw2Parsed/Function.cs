using System.Collections.Generic;
using System.Net;
using System.IO;


using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Amazon.Runtime;
using Amazon;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Raw2Parsed
{
    public class Functions
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {

        }

        public List<Entry> ExtractText(JObject rawJson)
        {
            JArray rawText = (JArray)rawJson["formImage"]["Pages"][0]["Texts"];
            int length = rawText.Count;
            List<Entry> text = new List<Entry>();
            for (int i = 0; i < length; i++)
            {
                Entry entry = new Entry(WebUtility.UrlDecode(rawText[i]["R"][0]["T"].ToString()),
                    (float)rawText[i]["x"],
                    (float)rawText[i]["y"],
                    (float)rawText[i]["w"],
                    (float)rawText[i]["R"][0]["TS"][1]);
                text.Add(entry);
            }
            return text;
        }

        public Parser SelectCustomer(string JSONstring)
        {
            if (JSONstring.Contains("CollectiveHealth%2C%20Inc.%20Summary%20Capitalization%20Table"))
            {
                return new ESharesParser();
            }
            else if (JSONstring.Contains("Breather%20Products%20Inc."))
            {
                return new BreatherParser();
            }
            return new ESharesParser();
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {

            context?.Logger.LogLine("Get Request\n");
            
            string JSONstring = "";
            //var credentials = new StoredProfileAWSCredentials("LinkLiang");
            //using (var client = new AmazonLambdaClient(credentials,RegionEndpoint.USEast1))
            using (var client = new AmazonLambdaClient())
            {
                var load = JsonConvert.SerializeObject(request.Body);
                var req = new InvokeRequest()
                {
                    FunctionName = "PDF2Raw",
                    Payload = "\"" + request.Body + "\""
                };

                context?.Logger.LogLine(req.ToString());
                var res = client.InvokeAsync(req).GetAwaiter().GetResult();

                using (var sr = new StreamReader(res.Payload))
                {
                    JSONstring = sr.ReadToEnd();
                }
            }

            Parser parse = SelectCustomer(JSONstring);
            string result = parse.Parse(JSONstring);

            context?.Logger.LogLine(result);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = result,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}
