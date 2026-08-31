namespace NamependingConnector.Models;

public class GetRolesResponse
{
    public RoleContent[] Roles;
    
    public class RoleContent
    {
        public string Id;
        public string Name;
        public Permissions Permissions;
        
        public GameGroupContent GameGroup;
        
        public class GameGroupContent
        {
            public string Id;
            public string Name;
            public string Color;
            public string[] Permissions;
        }
    }
}