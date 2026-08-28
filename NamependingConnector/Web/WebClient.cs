using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using NamependingConnector.Models;

namespace NamependingConnector;

public static class WebClient
{
    public static GraphQLHttpClient Client { get; private set; }

    public static void Init()
    {
        Client = new GraphQLHttpClient(
            NamependingPlugin.Cfg.ApiUrl, 
            new NewtonsoftJsonSerializer());
        Client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Server {NamependingPlugin.Cfg.ApiKey}");
        Logger.Info("WebClient initialized.");
    }

    public static void Destroy()
    {
        Client.Dispose();
        Client = null;
        Logger.Info("WebClient destroyed.");
    }
    
    public static async Task<GetPlayerResponse> GetPlayer(string platformId)
    {
        
        if (Client == null)
        {
            Logger.Error("WebClient is not initialized, cannot run GetPlayer.");
            return null;
        }
        
        var request = new GraphQLRequest
        {
            Query = """
                    query GetPlayer($id: String!) {
                        player(platformId: $id) {
                            id
                            created
                            updated
                            userId
                            bans {
                                active
                            }
                            user {
                                name
                                group {
                                    id
                                    gameGroup {
                                        id
                                        name
                                        permissions
                                    }
                                }
                            }
                        }
                    }
                    """,
            OperationName = "GetPlayer",
            Variables = new { id = platformId }
        };

        var response = await Client.SendQueryAsync<GetPlayerResponse>(request);

        if (response.Errors is null)
        {
            response.Data.Player.ConnectTime = DateTimeOffset.Now;
            return response.Data;
        }

        Logger.Error($"Get Player Failed: {platformId}: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        return null;
    }

    public static async Task<CreateLinkResponse> CreateLink(string platformId)
    {
        if (Client == null)
        {
            Logger.Error("WebClient is not initialized, cannot run CreateLink.");
            return null;
        }
        
        var request = new GraphQLRequest
        {
            Query = """
                    mutation CreateLink($id: String!) {
                        createAccountLink(platformId: $id) {
                            key
                            expires
                        }
                    }
                    """,
            OperationName = "CreateLink",
            Variables = new { id = platformId }
        };

        var response = await Client.SendMutationAsync<CreateLinkResponse>(request);

        if (response.Errors is null)
        {
            return response.Data;
        }

        Logger.Error($"Create Link Failed: {platformId}: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        return null;
    }

    public static async Task<UpdatePlayerResponse> UpdatePlayer(string platformId, bool doNotTrack, string name, int timeSpent)
    {
        if (Client == null)
        {
            Logger.Error("WebClient is not initialized, cannot run UpdatePlayer.");
            return null;
        }

        var request = new GraphQLRequest
        {
            Query = """
                    mutation UpdatePlayer($platformId: String!, $name: String!, $timeSpent: Int!, $doNotTrack: Boolean!) {
                      updatePlayer(
                        platformId: $platformId
                        name: $name
                        timeSpent: $timeSpent
                        doNotTrack: $doNotTrack
                      )
                    }
                    """,
            OperationName = "UpdatePlayer",
            Variables = new { platformId, name, timeSpent, doNotTrack },
        };

        var response = await Client.SendMutationAsync<UpdatePlayerResponse>(request);
        
        if (response.Errors is null)
        {
            return response.Data;
        }
        
        Logger.Error($"Update Player Failed: {platformId}: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        return null;
    }
}