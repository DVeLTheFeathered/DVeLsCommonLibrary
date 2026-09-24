using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace DVeLsCommonLibrary.UserInterface
{
    public static class Listing_StandardExtensions
    {
        public static string TextEntryLabeled(this Listing_Standard listing, string label, string text, float minTextAreaSizePct)
        {
            Vector2 labelSize = Verse.Text.CalcSize(label);
            minTextAreaSizePct = Math.Min(Math.Max(minTextAreaSizePct, 0), 1);

            Rect fullRect = listing.GetRect(Verse.Text.LineHeight);
            Rect labelRect = fullRect.LeftPartPixels(Math.Min((1 - minTextAreaSizePct) * listing.ColumnWidth, labelSize.x));
            Rect textRect = new Rect(labelRect.xMax, fullRect.y, fullRect.width - labelRect.width, fullRect.height);
            
            int anchor = (int)Verse.Text.Anchor;
            Verse.Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            string res = Widgets.TextField(textRect, text);
            Verse.Text.Anchor = (TextAnchor)anchor;

            listing.Gap(listing.verticalSpacing);
            return res;
        }

    }
}
