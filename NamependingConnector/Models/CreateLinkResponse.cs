namespace NamependingConnector.Models;

public class CreateLinkResponse
{
    public AccountLinkContent CreateAccountLink;
    
    public class AccountLinkContent
    {
        public string Key;
        public DateTimeOffset Expires;
    }
}