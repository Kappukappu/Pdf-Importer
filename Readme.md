Project structure:

1. The whole project is a generic PDF extraction tool for cap tables. It uses Pdf2json library : https://github.com/modesty/pdf2json
2. There are two lambda functions: Raw2Parsed(C#) and PDF2Raw(NodeJS wrapper to Pdf2json).
Raw2Parsed is the entry point for uploded PDF file. It passes the file to the second lambda function PDF2Raw. PDF2Raw returns generic Json representing PDF file, Raw2Parsed then format the generic Json into standarized format, and returns to caller.
3. Under Raw2Parsed:
Function.cs is the function that invokes PDF2Raw, get response from PDF2Raw, calls Parser for main logic and responses to the caller of lambda function.
Parser.cs is where the main logic locates.
SampleEshareRawJson.txt is a sample output from PDF2Raw
IParsingParameters.cs and EsharesParsingParameters.cs are for future use of costomized parameters.
4. Under PDF2Raw:
app.js has the handler for AWS and wrapper for Pdf2json library. The receiving payload is event and the return method is callback(null,content_to_return)

Run locally:

1. PDF2Raw.TestHarness is for invoking NodeJS wrapper lambda function (PDF2Raw) locally.
2. Raw2Parsed.TestHarness (under Raw2Parse folder) is for invoking C# lambda function (Raw2Parsed) locally. request.Body should be a PDF file in Base64 string.
To use Raw2Parsed.TestHarness, comment out line 45 in Function.cs and uncomment line 43 and 44. To publish to Lambda, comment out line 43 and 44 and uncomment line 45.


AWS Lambda function (in Visual Studio 2017):
1. Install AWS Toolkit for Visual Studio 2017 : https://aws.amazon.com/visualstudio/
2. Open the project in visual studio. Make sure AWSSDK.Lambda is installed in Nuget packages.
3. Right click on the project, and select "Publish to AWS Lambda". The Stack name should be the proper name for the lambda function, and S3 Bucket is where the lambda function is stored on AWS.
The instruction of creating S3 bucket is here: http://docs.aws.amazon.com/AmazonS3/latest/gsg/CreatingABucket.html
Make sure the region is US East
4. For Raw2Parsed, modify the policy of its role so that it has permission to call other lambda functions. A sample policy is below:
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:PutLogEvents",
                "lambda:*"
            ],
            "Resource": "*"
        }
    ]
}
Note that "lambda:*" gives the lambda function permission to call other lambda functions.
To modify the policy of a role, go to AWS IAM management console. Select the role in "Roles" tab. Click "Attach policy" and copy paste the policy above.

The steps of setting up AWS API gateway:
1. Go to AWS API gateway and create an API with proper name and description.
2. In Resources tab of the API, click Actions and then create method. Select POST.
3. Integration type is Lambda Function, Use Lambda Proxy integration is checked, Lambda Function is Raw2Parsed and Lambda Region is us-east-1.
4. Click Actions-Deploy API, and select a deployment stage or create one. Click Deploy. In Stages tab, find the method and Invoke URL is there for use of Postman and other testers.


Upload PDF file through Postman:

1.Make sure header has Content-Type : application/octet-stream
2.Make sure body type is binary
3.Make sure action is POST
4.The URL is the Invoke URL of AWS API gateway