using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using DVeLsCommonLibrary.UserInterface;
using UnityEngine;
using Verse;

namespace DVeLsCommonLibrary.HarmonyPatches.StatsReportUtility
{
    [HarmonyPatch(typeof(RimWorld.StatsReportUtility), "DrawStatsWorker")]
    public static class DrawStatsWorkerPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ReplaceDrawStatTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, ilGenerator);

            /*
            // First we must find the "statDrawEntry" local variable as LocalBuilder
            // It is used in 
            // IL_0288: ldloc.s statDrawEntry
            // IL_028a: ldfld        bool RimWorld.StatDrawEntry::hasOptionalReq
            // where hasOptionalReq call is unique.
            codeMatcher.MatchStartForward(
                CodeMatch.IsLdloc(),
                CodeMatch.LoadsField(typeof(StatDrawEntry).GetField("hasOptionalReq"))
            //,new CodeMatch(OpCodes.Ldloc_S, "statDrawEntry")
            );
            codeMatcher.ThrowIfInvalid("DrawStatsWorkerPatch could not find statDrawEntry");
            LocalBuilder statDrawEntry = codeMatcher.Instruction.operand as LocalBuilder;
            */

            // We need explanationText just to find other variables.
            // It is easy to find because GetExplanationText is called once.
            // IL_02b0: ldloc.s statDrawEntry
            // IL_02b2: ldloc.s statRequest
            // IL_02b4: callvirt     instance string RimWorld.StatDrawEntry::GetExplanationText(valuetype RimWorld.StatRequest)
            // IL_02b9: stloc.s      explanationText
            codeMatcher.MatchEndForward(
                CodeMatch.Calls(() => default(StatDrawEntry).GetExplanationText(default)),
                CodeMatch.IsStloc()
                );
            codeMatcher.ThrowIfInvalid("DrawStatsWorkerPatch could not find explanationText");
            LocalBuilder explanationText = codeMatcher.Instruction.operand as LocalBuilder;
            codeMatcher.Advance(-2);
            LocalBuilder statRequest = codeMatcher.Instruction.operand as LocalBuilder;
            codeMatcher.Advance(-1);
            LocalBuilder statDrawEntry = codeMatcher.Instruction.operand as LocalBuilder;

            // Using explanationText as anchor to find rect4
            // They are used together in
            // Widgets.Label(rect4, explanationText);
            // IL_02e7: ldloc.s rect4
            // IL_02e9: ldloc.s explanationText
            // IL_02eb: call         void Verse.Widgets::Label(valuetype[UnityEngine.CoreModule]UnityEngine.Rect, string)
            codeMatcher.MatchStartForward(
                CodeMatch.IsLdloc(),
                CodeMatch.IsLdloc(explanationText),
                CodeMatch.Calls(() => Widgets.Label(default(Rect), (string)null/*default(string)*/))
                );
            codeMatcher.ThrowIfInvalid("DrawStatsWorkerPatch could not find rect4");
            LocalBuilder rect4 = codeMatcher.Instruction.operand as LocalBuilder;

            /*
             Now We are right before Widgets.Label() call
            We add 
            float customDrawHeight = DrawStatsWorkerPatch.CustomDrawStats()
            before it
            We can use rect4 and then modify it for use in Widgets.Label()
            */

            // IL_02e7: ldloc.s rect4 <-- We are here
            // IL_02e9: ldloc.s explanationText
            // IL_02eb: call         void Verse.Widgets::Label(valuetype[UnityEngine.CoreModule]UnityEngine.Rect, string)
            LocalBuilder customDrawHeight;
            codeMatcher.DeclareLocal(typeof(float), out customDrawHeight);
            codeMatcher.InsertAfterAndAdvance(
                //new CodeInstruction(OpCodes.Dup),
                CodeInstruction.LoadLocal(statDrawEntry.LocalIndex),
                CodeInstruction.LoadLocal(statRequest.LocalIndex),
                CodeInstruction.Call(() => DrawStatsWorkerPatch.CustomDrawStats(default(Rect), default(StatDrawEntry), default(StatRequest))),
                CodeInstruction.StoreLocal(customDrawHeight.LocalIndex)
                );

            // IL_02e7: ldloc.s rect4
            //        : ldloc.s statDrawEntry
            //        : call CustomDrawStats()
            //        : stloc.s customDrawHeight <-- We are here
            // IL_02e9: ldloc.s explanationText
            // IL_02eb: call         void Verse.Widgets::Label(valuetype[UnityEngine.CoreModule]UnityEngine.Rect, string)
            // We have done CustomDrawStats, but Widgets.Label() does not have a Rect parameter
            // We will create it using
            // rect4 = rect4.BottomPartPixels(rect4.height - myHeight)
            codeMatcher.InsertAfterAndAdvance(
                CodeInstruction.LoadLocal(rect4.LocalIndex),
                CodeInstruction.LoadLocal(rect4.LocalIndex, true),
                CodeInstruction.Call(typeof(Rect), "get_height"),
                CodeInstruction.LoadLocal(customDrawHeight.LocalIndex),
                new CodeInstruction(OpCodes.Sub),
                CodeInstruction.Call(() => GenUI.BottomPartPixels(default(Rect), default(float))), // Extension for Rect
                new CodeInstruction(OpCodes.Dup), // one to rewrite rect4, another as parameter for Widgets.Label()
                CodeInstruction.StoreLocal(rect4.LocalIndex)
                );

            // That should do with drawing.
            // Now we must adjust num2 to represent the increased height
            // we will find it by searching for CalcHeight() + 10;
            // IL_02f9: call float32 Verse.Text::CalcHeight(string, float32)
            // IL_02fe: ldc.r4       10
            // IL_0303: add
            // IL_0304: stloc.s num2
            codeMatcher.MatchEndForward(
                CodeMatch.Calls(() => Text.CalcHeight(default, default)),
                CodeMatch.LoadsConstant(10f),
                new CodeMatch(OpCodes.Add),
                CodeMatch.IsStloc()
                );

            // IL_02f9: call float32 Verse.Text::CalcHeight(string, float32)
            // IL_02fe: ldc.r4       10
            // IL_0303: add
            // IL_0304: stloc.s num2  <-- We are here
            codeMatcher.Insert(
                CodeInstruction.LoadLocal(customDrawHeight.LocalIndex),
                new CodeInstruction(OpCodes.Add)
                );

            return codeMatcher.Instructions();
        }

        /// <returns>Returns height in pixels of the custom drawing</returns>
        public static float CustomDrawStats(Rect rect, StatDrawEntry statDrawEntry, StatRequest request)
        {
            if (!(statDrawEntry?.stat?.Worker is ICustomDrawStat worker)) return 0;
            return worker.CustomDraw(rect, request);
        }

        /*
        Copy-pasted parts of original code with needed adjustments
        private static void DrawStatsWorkerCopyPaste(Thing optionalThing, Rect outRect, StatDrawEntry statDrawEntry, ref float ___rightPanelHeight, ref Vector2 scrollPositionRightPanel)
        {
            // 1. Do custom drawing
            // 2. Move rec4 down by customDrawHeight
            // 3. Increase num2 by customDrawHeight

            Rect viewRect2 = new Rect(0.0f, 0.0f, outRect.width - 16f, ___rightPanelHeight);
            StatRequest statRequest = !statDrawEntry.hasOptionalReq ? (optionalThing == null ? StatRequest.ForEmpty() : StatRequest.For(optionalThing)) : statDrawEntry.optionalReq;
            string explanationText = statDrawEntry.GetExplanationText(statRequest);
            float num1 = 0.0f;
            Widgets.BeginScrollView(outRect, ref scrollPositionRightPanel, viewRect2);
            Rect rect4 = viewRect2;
            rect4.width -= 4f;
            float customDrawHeight = CustomDrawStats(rect4, statDrawEntry); // Here we do the drawing before Widgets.Label(rect4, explanationText);
            rect4 = rect4.BottomPartPixels(rect4.height - customDrawHeight); // Recreate new rect4 for futher drawing
            Widgets.Label(rect4, explanationText);
            float num2 = Text.CalcHeight(explanationText, rect4.width) + 10f;
            num2 += customDrawHeight; // Add customDrawHeight
            float num3 = num1 + num2;
            
            IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = statDrawEntry.GetHyperlinks(statRequest);
            if (hyperlinks != null)
            {
                Rect rect5 = new Rect(rect4.x, rect4.y + num2, rect4.width, rect4.height - num2); 
                Color color1 = GUI.color;
                GUI.color = Widgets.NormalOptionColor;
                foreach (Dialog_InfoCard.Hyperlink hyperlink1 in hyperlinks)
                {
                    float height = Text.CalcHeight(hyperlink1.Label, rect5.width);
                    Rect rect6 = new Rect(rect5.x, rect5.y, rect5.width, height);
                    string str2 = hyperlink1.HasGeneOwnerThing ? hyperlink1.Label : "ViewHyperlink".Translate((NamedArgument)hyperlink1.Label).ToString();
                    Dialog_InfoCard.Hyperlink hyperlink2 = hyperlink1;
                    string text = str2;
                    Color? color2 = new Color?();
                    Widgets.HyperlinkWithIcon(rect6, hyperlink2, text, color: color2);
                    rect5.y += height;
                    rect5.height -= height;
                    num3 += height;
                }
                GUI.color = color1;
            }
            ___rightPanelHeight = num3;
            Widgets.EndScrollView();
        }
        */

    }
}
