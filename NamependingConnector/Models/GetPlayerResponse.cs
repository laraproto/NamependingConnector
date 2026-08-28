using JetBrains.Annotations;

namespace NamependingConnector.Models;

public class GetPlayerResponse
{

    public PlayerContent Player;
    
    public class PlayerContent
    {
        public DateTimeOffset Created;
        public DateTimeOffset Updated;
        public DateTimeOffset ConnectTime;
        public string Id;
        [CanBeNull] public string UserId;
        [CanBeNull] public UserContent User;
        public BansContent[] Bans;

        public class BansContent
        {
            public bool Active;
        }

        public class UserContent
        {
            public string Name;
            public GroupContent Group;
            
            public class GroupContent
            {
                public string Name;

                public GameGroupContent GameGroup;
        
                public class GameGroupContent
                {
                    public string Name;
                    public string Id;
                    public string[] Permissions;
                }
            }
        }
    }
}