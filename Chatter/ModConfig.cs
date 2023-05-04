using StardewModdingAPI;

namespace Chatter
{
    public class ModConfig
    {
        public bool enableIndicators = false;
        public SButton enableIndicatorsButton = SButton.O;
        public bool enableDebugOutput = false;
        public bool useDebugOffsetsForAllNPCs = false;
        public float indicatorXOffset = 16f;
        public float indicatorYOffset = -100f;
    }

    public class ModConfigField
    {
        public const string enableIndicators = "enableIndicators";
        public const string enableIndicatorsKeybind = "enableIndicatorsKeybind";
        public const string enableDebugOutput = "enableDebugOutput";
        public const string useDebugOffsetsForAllNPCs = "useDebugOffsetsForAllNPCs";
        public const string indicatorXOffset = "indicatorXOffset";
        public const string indicatorYOffset = "indicatorYOffset";
    }
}
