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

        var wrapper = JsonSerializer.Deserialize<RequestBodyWrapper>(apigProxyEvent.Body, LambdaFunctionJsonSerializerContext.Default.RequestBodyWrapper);
        var mfaTokenRequest = wrapper?.MFATokenRequest;

        // Example: Use the deserialized MFATokenRequest object
        var responseMessage = $"Received MFA token request from {mfaTokenRequest?.SentBy} to {mfaTokenRequest?.ReceivedBy} at {mfaTokenRequest?.RequestedAt}";

        var location = await GetCallingIP();
        var body = new Dictionary<string, string>
        {
            { "message", responseMessage },
            { "location", location }
        };

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(body, typeof(Dictionary<string, string>), LambdaFunctionJsonSerializerContext.Default),
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
