using System;
using Harmony;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Object = StardewValley.Object;

namespace RadioactiveGeodes
{
    public class ModEntry : Mod
    {
        public static Config Config;

        public static IMonitor Logger;
        internal ITranslationHelper i18n => Helper.Translation;

        private readonly HarmonyInstance _harmony = HarmonyInstance.Create("jas.RadioactiveGeodes");

        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("RadioactiveGeodes.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            Monitor.Log(startingMessage, LogLevel.Trace);
            Logger = Monitor;
            

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), "getTreasureFromGeode"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(GeodePatch), "getTreasureFromGeodePostFix"))
            );

            helper.Events.GameLoop.GameLaunched += onLaunched;
        }


        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {
            Config = Helper.ReadConfig<Config>();
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
            {
                var api = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

                api.RegisterModConfig(ModManifest, () => Config = new Config(), () => Helper.WriteConfig(Config));
                api.SetDefaultIngameOptinValue(ModManifest, true);
                api.RegisterClampedOption(ModManifest, "Chance", "Chance of replacing Iridium Ore with Radioactive Ore", () => Config.Chance, (int val) => Config.Chance = val, 0, 100);
            }
            
        }


    }

    [HarmonyPatch(typeof(Utility), "getTreasureFromGeode")]
    public static class GeodePatch
    {

        static void getTreasureFromGeodePostFix(ref Item __result)
        {
            // ModEntry.Logger.Log("Postfix Activated with: " + __result.Name + " of stack size " + __result.Stack);
            if (!Game1.player.team.mineShrineActivated.Value)
            {
                return;
            }
            int stack = __result.Stack;
            if (__result.ParentSheetIndex != 386) return;
            Random r = new Random(DateTime.Now.Millisecond);
            if (r.Next(0,ModEntry.Config.Chance) == 0)
            {
                __result = new Object(909, stack);
            }
        }
    }
}
