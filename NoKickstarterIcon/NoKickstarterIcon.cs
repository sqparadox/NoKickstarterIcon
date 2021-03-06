using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using Newtonsoft.Json;
using SVGImporter;
using System;
using System.IO;
using System.Reflection;

namespace NoKickstarterIcon
{
    public class NoKickstarterIcon
    {
        internal static ModSettings settings;
        internal static string ModDirectory;

        public static void Init(string directory, string modSettings)
        {
            ModDirectory = directory;
            try
            {
                settings = JsonConvert.DeserializeObject<ModSettings>(modSettings);
            }
            catch (Exception e)
            {
                Logger.NewLog();
                Logger.LogError(e);
            }
            var harmony = HarmonyInstance.Create("sqparadox.NoKickstarterIcon");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(SimGameState), "GetPilotRoninIcon")]
        public static class NoKickstarterIcon_GetPilotRoninIcon_Patch
        {
            public static bool Prepare()
            {
                return settings.ChangeIconToRonin;
            }
            public static void Postfix(SimGameState __instance, Pilot p, ref SVGAsset __result, Pilot ___commander)
            {
                try
                {
                    BattleTech.Data.SVGCache cache = (BattleTech.Data.SVGCache)Traverse.Create(__instance.DataManager).Method("get_SVGCache").GetValue();
                    if (p.Description.Id == ___commander.Description.Id)
                        __result = cache.GetAsset("uixSvgIcon_mwrank_Commander");
                    if (p.pilotDef.IsVanguard || p.pilotDef.IsRonin)
                        __result = cache.GetAsset("uixSvgIcon_mwrank_Ronin");
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(SimGameState), "GetPilotTypeColor")]
        public static class NoKickstarterIcon_GetPilotTypeColor_Patch
        {
            public static bool Prepare()
            {
                return settings.ChangeColorToRonin;
            }
            public static void Postfix(SimGameState __instance, Pilot p, ref UIColor __result, Pilot ___commander)
            {
                try
                {
                    if (p.pilotDef.Description.Id == ___commander.Description.Id)
                        __result = UIColor.SimPilotCommander;
                    if (p.pilotDef.IsRonin || p.pilotDef.IsVanguard)
                        __result = UIColor.SimPilotRonin;
                    else
                        __result = UIColor.SimPilotStandard;
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }
        [HarmonyPatch(typeof(SimGameState), "SetupRoninTooltip")]
        public static class NoKickstarterIcon_SetupRoninTooltip_Patch
        {
            public static bool Prepare()
            {
                return settings.ChangeTooltipToRonin;
            }
            public static void Postfix(HBSTooltip RoninTooltip, Pilot pilot)
            {
                if (RoninTooltip != null)
                {
                    string text;
                    if (pilot.pilotDef.IsVanguard || pilot.pilotDef.IsRonin)
                        text = "UnitMechWarriorSpecial";
                    else
                    {
                        if (pilot != UnityGameInstance.BattleTechGame.Simulation.Commander)
                            return;
                        text = "UnitMechWarriorCommander";
                    }
                    if (!string.IsNullOrEmpty(text) && UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Exists(text))
                    {
                        BaseDescriptionDef def = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get(text);
                        RoninTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(def));
                    }
                }
            }
        }

        public class Logger
        {
            static readonly string filePath = $"{ModDirectory}\\Log.txt";
            public static void NewLog()
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.WriteLine("");
                }
            }
            public static void LogError(Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            public static void LogLine(String line)
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, true))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString() + Environment.NewLine + line + Environment.NewLine);
                }
            }
        }
        internal class ModSettings
        {
            public bool ChangeIconToRonin = true;
            public bool ChangeColorToRonin = false;
            public bool ChangeTooltipToRonin = false;
        }
    }
}
