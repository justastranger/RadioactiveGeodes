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
        internal ITranslationHelper i18n => Helper.Translation;

        internal readonly Harmony _harmony = new(ModID);

        public override void Entry(IModHelper helper)
        {
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

                api.RegisterModConfig(ModManifest, () => Config = new Config(), () => Helper.WriteConfig(Config));
                api.SetDefaultIngameOptinValue(ModManifest, true);
                api.RegisterClampedOption(ModManifest, "Chance", "Chance of replacing Iridium Ore with Radioactive Ore. Higher numbers lower the chance.", () => Config.Chance, (int val) => Config.Chance = val, 0, 100);
                api.RegisterSimpleOption(ModManifest, "Debug Mode", "Enabled extra logging information.", () => Config.Debug, (bool val) => Config.Debug = val);
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
            if (ModEntry.Config.Debug) ModEntry.Logger.Log("Postfix Activated with: " + __result.Name + " of stack size " + __result.Stack, LogLevel.Info);
            if (__result == null)
            {
                ModEntry.Logger.Log("Null Result from Geode.", LogLevel.Error);
                return;
            }
            if (!Game1.player.team.mineShrineActivated.Value)
            {
                if (ModEntry.Config.Debug) ModEntry.Logger.Log("Hard Mode Shrine not activated.", LogLevel.Info);
                return;
            }
            // if (__result.ParentSheetIndex != SObject.iridium) return;
            if (Utility.IsNormalObjectAtParentSheetIndex(__result, SObject.iridium))
            {
                var stack = __result.Stack;
                if (ModEntry.Config.Debug) ModEntry.Logger.Log("Iridium Ore Detected.", LogLevel.Info);
                Random r = new(DateTime.Now.Millisecond);
                if (r.Next(0, ModEntry.Config.Chance) == 0)
                {
                    if (ModEntry.Config.Debug) ModEntry.Logger.Log("Radiation Dispensed.", LogLevel.Info);
                    __result = new SObject(909, stack);
                }
            }
        }
    }
}
