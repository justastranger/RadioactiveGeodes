using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace RadioactiveGeodes
{
    public class ModEntry : Mod
    {
        internal static string ModID = "jas.RadioactiveGeodes";

        internal static Config Config;

        internal static IMonitor Logger;
        internal static ITranslationHelper i18n;

        internal readonly Harmony _harmony = new(ModID);

        public override void Entry(IModHelper helper)
        {
            i18n = Helper.Translation;
            Monitor.Log(i18n.Get("RadioactiveGeodes.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath }), LogLevel.Trace);
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

                api.Register(ModManifest, () => Config = new Config(), () => Helper.WriteConfig(Config), false);
                api.AddNumberOption(ModManifest, () => Config.Chance, (int val) => Config.Chance = val, () => i18n.Get("RadioactiveGeodes.config.chance.name"), () => i18n.Get("RadioactiveGeodes.config.chance.description"), 0, 100, 1);
                api.AddBoolOption(ModManifest, () => Config.Debug, (bool val) => Config.Debug = val, () => i18n.Get("RadioactiveGeodes.config.debug.name"), () => i18n.Get("RadioactiveGeodes.config.debug.description"));
                api.AddBoolOption(ModManifest, () => Config.RequireBothShrines, (bool val) => Config.RequireBothShrines = val, () => i18n.Get("RadioactiveGeodes.config.requirebothshrines.name"), () => i18n.Get("RadioactiveGeodes.config.requirebothshrines.description"));
            }
            
        }

        public new void Dispose()
        {
            _harmony.UnpatchAll(ModID);
        }


    }

    [HarmonyPatch(typeof(Utility), "getTreasureFromGeode")]
    public static class GeodePatch
    {

        static void getTreasureFromGeodePostFix(ref Item __result)
        {
            if (ModEntry.Config.Debug) ModEntry.Logger.Log(ModEntry.i18n.Get("RadioactiveGeodes.debug.postfix", new { __result.Name, __result.Stack }), LogLevel.Info);
            // 
            if (__result == null)
            {
                ModEntry.Logger.Log("Null Result from Geode.", LogLevel.Error);
                return;
            }
            // if neither hard mode shrine is active, do not continue
            if (!Game1.player.team.mineShrineActivated.Value && !Game1.player.team.skullShrineActivated.Value)
            {
                if (ModEntry.Config.Debug) ModEntry.Logger.Log(ModEntry.i18n.Get("RadioactiveGeodes.debug.postfix.noshrines"), LogLevel.Info);
                return;
            }
            // if only one shrine is active but both are needed
            else if (ModEntry.Config.RequireBothShrines && !(Game1.player.team.mineShrineActivated.Value && Game1.player.team.skullShrineActivated.Value))
            {
                if (ModEntry.Config.Debug) ModEntry.Logger.Log(ModEntry.i18n.Get("RadioactiveGeodes.debug.postfix.requirebothshrines"), LogLevel.Info);
                return;
            }
            // check for iridium ore
            if (__result.QualifiedItemId == SObject.iridiumQID)
            {
                var stack = __result.Stack;
                if (ModEntry.Config.Debug) ModEntry.Logger.Log(ModEntry.i18n.Get("RadioactiveGeodes.debug.postfix.iridium"), LogLevel.Info);
                
                if (Game1.random.Next(0, ModEntry.Config.Chance) == 0)
                {
                    if (ModEntry.Config.Debug) ModEntry.Logger.Log(ModEntry.i18n.Get("RadioactiveGeodes.debug.postfix.jackpot"), LogLevel.Info);
                    // replace the stack with radioactive ore in the same quantity
                    __result = ItemRegistry.Create("(O)909", stack);
                }
            }
        }
    }
}
