using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace DVeLsCommonLibrary
{

    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {
        static HarmonyInstance()
        {
            if (Harmony.HasAnyPatches("DVeL.CommonLibrary")) return;
            var harmony = new Harmony("DVeL.CommonLibrary");
            
            if (DCLFeatures.FeatureRequested(DCLFeature.CustomDrawStat))
            {
                try
                {
                    harmony.PatchCategory(nameof(DCLFeature.CustomDrawStat));
                }
                catch (Exception e)
                {
                    Log.Error($"DVeL.CommonLibrary. Applying Harmony patches for {nameof(DCLFeature.CustomDrawStat)} failed with exception.\r\nPossible reasons: Rimworld update or mod conflict\r\n{e}");
                    throw;
                }
                Log.Message($"DVeL.CommonLibrary. Successfully applied Harmony patches for {nameof(DCLFeature.CustomDrawStat)}");
            }
        }
    }

}