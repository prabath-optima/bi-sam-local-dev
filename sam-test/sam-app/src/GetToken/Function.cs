using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;


namespace MFAService;

public class Function
{
    private static readonly HttpClient client = new HttpClient();
    
    /// <summary>
    /// The main entry point for the Lambda function.
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
        // Ensure query string parameters are not null
        if (apigProxyEvent.QueryStringParameters == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Query string parameters are missing",
                StatusCode = 400, // Bad Request
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        // Extract query parameters safely
        apigProxyEvent.QueryStringParameters.TryGetValue("ReceivedBy", out var receivedBy);
        apigProxyEvent.QueryStringParameters.TryGetValue("SentBy", out var sentBy);
        apigProxyEvent.QueryStringParameters.TryGetValue("RequestedAt", out var requestedAt);

        // Example: Use the extracted query parameters
        var responseMessage = $"Received MFA token request from {sentBy} to {receivedBy} at {requestedAt}";

        var body = new Dictionary<string, string>
    {
        { "message", responseMessage }
    };

        // Adjusted serialization call using the source-generated context
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(body, LambdaFunctionJsonSerializerContext.Default.DictionaryStringString),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

}

// Define the MFATokenRequest class to match the expected JSON structure
public class MFATokenRequest
{
    public string ReceivedBy { get; set; }
    public string SentBy { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class RequestBodyWrapper
{
    public MFATokenRequest MFATokenRequest { get; set; }
}

// Adjust the JsonSerializerContext to include the MFATokenRequest type
[JsonSerializable(typeof(RequestBodyWrapper))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(MFATokenRequest))] // Include MFATokenRequest type for serialization
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}
