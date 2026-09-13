using HarmonyLib;
using Verse;

namespace DVeLsCommonLibrary.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {

        static HarmonyInstance()
        {
            if (!Harmony.HasAnyPatches("DVeL.CommonLibrary"))
            {
                var harmony = new Harmony("DVeL.CommonLibrary");
                harmony.PatchAll();
            }
        }
    }
}