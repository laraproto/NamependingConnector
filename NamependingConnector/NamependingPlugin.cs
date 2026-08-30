using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace NamependingConnector
{
    public sealed class NamependingPlugin : Plugin<NamependingConfig>
    {
        public static NamependingPlugin Instance { get; private set; }
        
        public static NamependingConfig Cfg => Instance?.Config;
        
        public PlayerEventsHandler PlayerEvents { get; } = new();
        public ServerEventHandlers ServerEvents { get; } = new ();
        
        public override string Name => "Namepending";
        public override string Description => "Syncs various things to web panel";
        public override string Author => "Lara The Protogen";
        public override Version Version => GetType().Assembly.GetName().Version;
        public override Version RequiredApiVersion { get; } = new (LabApiProperties.CompiledVersion);
        public override bool IsTransparent => true;
        
        public override void Enable()
        {
            Instance = this;
            if (Cfg.ApiKey != null && Cfg.ApiUrl != null)
            {
                WebClient.Init();
            }
            CustomHandlersManager.RegisterEventsHandler(PlayerEvents);
            CustomHandlersManager.RegisterEventsHandler(ServerEvents);
            Logger.Info("Namepending Connector started.");
        }

        public override void Disable() => Logger.Info("Namepending Connector stopped.");
    }
}