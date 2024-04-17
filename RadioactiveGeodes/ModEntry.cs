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

        internal static readonly Random random = new(DateTime.Now.Millisecond);

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
            if (ModEntry.Config.Debug) ModEntry.Logger.Log("Postfix Activated with: " + __result.Name + " of stack size " + __result.Stack, LogLevel.Info);
            if (__result == null)
            {
                ModEntry.Logger.Log("Null Result from Geode.", LogLevel.Error);
                return;
            }
            // if neither hard mode shrine is active, do not continue
            if (!Game1.player.team.mineShrineActivated.Value && !Game1.player.team.skullShrineActivated.Value)
            {
                if (ModEntry.Config.Debug) ModEntry.Logger.Log("Hard Mode Shrines not activated.", LogLevel.Info);
                return;
            }
            // if only one shrine is active but both are needed
            else if (ModEntry.Config.RequireBothShrines && !(Game1.player.team.mineShrineActivated.Value && Game1.player.team.skullShrineActivated.Value))
            {
                if (ModEntry.Config.Debug) ModEntry.Logger.Log("Not enough Hard Mode Shrines activated.", LogLevel.Info);
                return;
            }
            // check for iridium ore using new method
            // no more checking tilesheet indexes
            // only complaint is that not all of the vanilla items have constants for their QIDs
            if (__result.QualifiedItemId == SObject.iridiumQID)
            {
                var stack = __result.Stack;
                if (ModEntry.Config.Debug) ModEntry.Logger.Log("Iridium Ore Detected.", LogLevel.Info);
                if (ModEntry.random.Next(0, ModEntry.Config.Chance) == 0)
                {
                    if (ModEntry.Config.Debug) ModEntry.Logger.Log("Radiation Dispensed.", LogLevel.Info);
                    // replace the result, creating the stack in the new way
                    // uses the Qualified Item ID for radioactive ore (bars are 910)
                    __result = ItemRegistry.Create("(O)909", stack);
                }
            }
        }
    }
}
