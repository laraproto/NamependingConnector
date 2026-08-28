namespace NamependingConnector.Models;

public class GetServerResponse
{
    public ServerContent Server;
    
    public class ServerContent
    {
        public string Id;
        public string Description;
    }
}