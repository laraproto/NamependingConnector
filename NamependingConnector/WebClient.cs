using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace NamependingConnector;

public static class WebClient
{
    public static GraphQLHttpClient Client { get; private set; }

    public static void Init()
    {
        Client = new GraphQLHttpClient(
            NamependingPlugin.Cfg.ApiUrl, 
            new SystemTextJsonSerializer());
        Client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Server {NamependingPlugin.Cfg.ApiKey}");
    }
}