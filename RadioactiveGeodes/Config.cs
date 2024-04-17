using StardewModdingAPI;

namespace RadioactiveGeodes
{
    public class Config
    {
        // public SButton debugKey { get; set; }

        public int Chance { get; set; }
        public bool Debug { get; set; }
        public bool RequireBothShrines { get; set; }

        public Config()
        {
            Chance = 4;
            Debug = false;
            RequireBothShrines = false;
        }
    }
}
