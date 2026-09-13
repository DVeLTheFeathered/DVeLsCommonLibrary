using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LudeonTK;

namespace DVeLsCommonLibrary.Source
{
    public static class Drawing
    {
        /// <summary>
        /// Draws a table. Clamps parts of image that do not fit into provided Rect
        /// </summary>
        /// <param name="rect">Space available for drawing</param>
        /// <returns>Space actually used to draw table</returns>
        public static Rect DrawTable(Rect rect)
        {
            return new Rect(0,0,0,0);
        }
    }
}
