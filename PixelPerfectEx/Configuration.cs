using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx
{
    internal partial class Configuration: IPluginConfiguration
    {

        public int Version { get; set; } = 0;

        public bool ConfigVisible = false;


        #region Position Assist

        
        public bool OwnerHitboxVisible = false;
        public float OwnerHitboxSize = 2f;
        public bool OwnerHitboxOuterRingVisible = false;
        public bool OwnerHitboxCombatOnly = false;
        public bool OwnerHitboxInstanceOnly = false;
        public bool OwnerRingVisible = false;
        public float OwnerRingSize = 2f;
        public float OwnerRingThickness = 2f;
        public Vector4 OwnerRingColour = new Vector4(1, 0, 0, 1);
        public Vector4 OwnerHitboxColour = new Vector4(1, 0, 0, 1);
        public Vector4 OwnerHitboxOuterRingColour = new Vector4(1, 0, 0, 1);

        public bool TargetHitboxVisible = false;
        public float TargetHitboxSize = 2f;
        public bool TargetHitboxOuterRingVisible = false;
        public bool TargetHitboxCombatOnly = false;
        public bool TargetHitboxInstanceOnly = false;
        public bool TargetRingVisible = false;
        public float TargetRingSize = 2f;
        public float TargetRingThickness = 2f;
        public Vector4 TargetRingColour = new Vector4(1, 0, 0, 1);
        public bool TargetRing2Visible = false;
        public float TargetRing2Size = 2f;
        public float TargetRing2Thickness = 2f;
        public Vector4 TargetRing2Colour = new Vector4(1, 0, 0, 1);

        public bool TargetBodyPosVisible = false;
        public float TargetBodyPosSize = 2f;
        public float TargetBodyPosThickness = 2f;
        public float TargetBodyPosOffset = 0f;
        public Vector4 TargetBodyPosColour = new Vector4(1, 0, 0, 1);

        public Vector4 TargetHitboxColour = new Vector4(1, 0, 0, 1);
        public Vector4 TargetHitboxOuterRingColour = new Vector4(1, 0, 0, 1);
        public bool TargetPlayerLineVisible = false;
        public Vector4 TargetPlayerLineColour = new Vector4(1, 0, 0, 1);
        public bool RelativePositionLineVisible = false;
        public float RelativePositionLineLength = 2f;
        public Vector4 RelativePositionLineColour = new Vector4(1, 0, 0, 1);

        public bool DistanceShowVisible = false;
        public bool DistanceShowFollowPlayer = false;
        public bool DistanceShowCombatOnly = false;
        public bool DistanceShowInstanceOnly = false;
        public string DistanceShowFormat = "00.00";
        public float DistanceShowLarge = 5;
        public float DistanceBgAlpha = 0;
        public Vector2 DistanceShowOffset = new Vector2(0, 0);
        public Vector4 DistanceShowColour = new Vector4(1, 0, 0, 1);
        #endregion

        #region Draw

        public bool DrawDebug = false;
        public bool DrawAoeAssist = false;
        public bool ShowReceive = false;
        public int Segment = 20;
        public int HttpPort = 9588;
        public int DrawSize = 4;
        public bool Strock = false;
        public float StrockWidth = 1;
        public float DefaultThickness = 10;

        #endregion


        public static Configuration Load(DirectoryInfo configDirectory)
        {
            var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, $"PixelPerfectEx.json"));

            if (!pluginConfigPath.Exists)
                return new Configuration();

            var data = File.ReadAllText(pluginConfigPath.FullName);
            var conf = JsonConvert.DeserializeObject<Configuration>(data);
            return conf ?? new Configuration();
        }

        internal void Upgrade()
        {
        }

        public void Save() => Service.Interface.SavePluginConfig(this);
        
    }
}
