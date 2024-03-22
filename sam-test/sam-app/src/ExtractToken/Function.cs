using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Text.RegularExpressions;

namespace MFAService;

public partial class Function
{
    private static readonly HttpClient client = new HttpClient();
    
    /// <summary>
    /// The main entry point for the Lambda function. The main function is called once during the Lambda init phase. It
    /// initializes the .NET Lambda runtime client passing in the function handler to invoke for each Lambda event and
    /// the JSON serializer to use for converting Lambda JSON format to the .NET types. 
    /// </summary>
    private static async Task Main()
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task<string> GetCallingIP()
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

        var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

        return msg.Replace("\n","");
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {

        // Get message body from request
        string messageBody = apigProxyEvent.Body;  // Assuming request body is a string

        // Check if body is empty or null
        if (string.IsNullOrEmpty(messageBody))
        {
            messageBody = "No message provided in request body";
        }

        var location = await GetCallingIP();
        var body = new Dictionary<string, string>
        {
            { "message", messageBody },
            { "location", location }
        };

        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(body, typeof(Dictionary<string, string>), LambdaFunctionJsonSerializerContext.Default),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    // public async Task<string> ExtractToken(string message)
    // {
    //     var patternToMatch = await GetRegex();  // Assuming GetRegex() returns the desired pattern

    //     // Use Match method to find the first match
    //     var match = Regex.Match(message, patternToMatch);

    //     // Check if there's a match
    //     if (match.Success)
    //     {
    //         // Extract the matched group (assuming there's only one capturing group)
    //         return match.Groups[1].Value;
    //     }
    //     else
    //     {
    //         // Handle the case where no match is found (optional)
    //         // You can return an empty string, a default value, or throw an exception
    //         return "";  // Example: return an empty string if no match
    //     }
    // }

    public async Task<Regex> GetRegex()
    {
        return MyRegex();
    }

    [GeneratedRegex(@"\s\d{6}")]
    private static partial Regex MyRegex();
}

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}