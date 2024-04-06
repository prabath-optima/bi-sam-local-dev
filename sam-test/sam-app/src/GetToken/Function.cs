extern alias jsonnet;
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
using MongoDB.Driver;
using MongoDB.Bson;

// Use the jsonnet alias for Newtonsoft.Json types
using JsonConverter = jsonnet.Newtonsoft.Json.JsonConverter;
using JsonReader = jsonnet.Newtonsoft.Json.JsonReader;
using JsonWriter = jsonnet.Newtonsoft.Json.JsonWriter;
using JsonSerializer = jsonnet.Newtonsoft.Json.JsonSerializer;
using MongoDB.Bson.Serialization.Attributes;

namespace MFAService;

public class Function
{
    private static readonly HttpClient client = new HttpClient();
    private static IMongoDatabase database = null;

    private static readonly string mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI");

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

        var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext: false);

        return msg.Replace("\n", "");
    }

    // Method to establish MongoDB connection
    private static IMongoDatabase GetDatabase()
    {
        if (database == null)
        {
            try
            {
                // Replace the below connection string with your actual connection string.
                var connectionString = "mongodb://username:password@mongodb:27017/MfaDataStore?authSource=admin";

                // Create a MongoClient object using the connection string
                var client = new MongoClient(connectionString);
                Console.WriteLine($"-----------------------------Attempting to connect using {connectionString}-------------------");

                // Get the database using its name
                database = client.GetDatabase("MfaDataStore");
            }
            catch (Exception ex)
            {
                if (ex is MongoConnectionException)
                {
                    Console.WriteLine("Error connecting to MongoDB server:");
                    Console.WriteLine(ex.ToString());
                }
                else if (ex is NotSupportedException)
                {
                    Console.WriteLine("Serializer compatibility issue:");
                    Console.WriteLine(ex.ToString()); 
                }
                else
                {
                    Console.WriteLine("Unexpected error occurred:");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        return database;
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {

        var db = GetDatabase();
        //var dbName = db.DatabaseNamespace;

        var collection = db.GetCollection<BsonDocument>("MfaOtp");
        // Define your filter based on the query parameters or some criteria
        var filter = Builders<BsonDocument>.Filter.Empty; // Example: Empty filter to fetch all documents

        var results = await collection.Find(filter).ToListAsync();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = jsonnet.Newtonsoft.Json.JsonConvert.SerializeObject(results),
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
