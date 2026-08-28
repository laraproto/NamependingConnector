using System.Linq;
using System.Threading.Tasks;
using CommandSystem;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using NamependingConnector.Models;

namespace NamependingConnector.Command;

[CommandHandler(typeof(GameConsoleCommandHandler))]
public class Setup: ICommand
{
    internal GraphQLRequest Request = new GraphQLRequest
    {
        Query = """
                {
                    server {
                        id
                        description
                    }
                }
                """,
    };
    
    public string Command { get; } = "setupnamepending";

    public string[] Aliases { get; } = [];
    
    public string Description { get; } = "Configures the Namepending plugin with the provided API key and URL.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Missing arguments. Usage: setupnamepending <api_key> <api_url>";
            return false;
        }

        
        string key = arguments.At(0);
        string url = arguments.At(1);
        
        TestConnection(key, url).ConfigureAwait(false);
        
        response = "Running connection test...";
        return true;
    }
    
    protected async Task TestConnection(string apiKey, string apiUrl)
    {
        try
        {
            using var client = new GraphQLHttpClient(apiUrl, new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Server {apiKey}");
            var queryResponse = await client.SendQueryAsync<GetServerResponse>(Request);

            if (queryResponse.Errors != null)
            {
                Logger.Error(
                    $"Error occurred while testing connection: {string.Join(", ", queryResponse.Errors.Select(e => e.Message))}");
            }

            Logger.Info($"Successfully connected to {apiUrl}, Server ID: {queryResponse.Data.Server.Id}, Description: {queryResponse.Data.Server.Description}");

            NamependingPlugin.Cfg.ApiKey = apiKey;
            NamependingPlugin.Cfg.ApiUrl = apiUrl;
            NamependingPlugin.Instance.SaveConfig();
            WebClient.Init();
        }
        catch (TaskCanceledException ex)
        {
            Logger.Error($"Connection test timed out: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error occurred while testing connection: {ex.Message}");
        }
    }
}