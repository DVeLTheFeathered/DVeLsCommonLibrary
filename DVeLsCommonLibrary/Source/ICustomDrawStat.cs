using RimWorld;
using UnityEngine;

namespace DVeLsCommonLibrary
{
    public interface ICustomDrawStat
    {
        /// <summary>
        /// Implement this on a StatWorker, and it will get an option to draw something before usual text
        /// </summary>
        /// <returns>Returns height in pixels of custom drawing</returns>
        float CustomDraw(Rect rect, StatRequest request);
    }
}
