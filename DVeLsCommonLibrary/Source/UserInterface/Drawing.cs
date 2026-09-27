using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace DVeLsCommonLibrary.UserInterface
{
    public static class Drawing
    {
        /// <summary>
        /// Draws a table. Clamps parts of image that do not fit into provided Rect
        /// </summary>
        /// <param name="rect">Space available for drawing</param>
        /// <returns>Space actually used to draw table</returns>
        public static Rect DrawTable<T>(Rect rect, List<T> dataSource, List<TableDataGetter<T>> columns)
        {
            // based on DebugTables.MakeTablesDialog();
            int rowsTotal = dataSource.Count + 1; // + headers
            int columnsTotal = columns.Count;
            string[,] table = new string[columnsTotal, rowsTotal];
            int colN = 0;
            foreach (TableDataGetter<T> getter in columns)
            {
                table[colN, 0] = getter.label;
                ++colN;
            }
            int rowN = 1;
            foreach (T item in dataSource)
            {
                for (colN = 0; colN < columnsTotal; ++colN)
                    table[colN, rowN] = columns[colN].getter(item);
                ++rowN;
            }

            // based on Window_DebugTable.DoWindowContents()
            List<float> rowHeights = new List<float>();
            for (rowN = 0; rowN < table.GetLength(1); ++rowN)
            {
                float height = 0.0f;
                for (colN = 0; colN < table.GetLength(0); ++colN)
                {
                    float y = Text.CalcSize(table[colN, rowN]).y;
                    if (y > height)
                        height = y;
                }
                rowHeights.Add(height + 2f);
            }
            float rowsTotalHeight = rowHeights.Sum();

            List<float> colWidths = new List<float>();
            for (colN = 0; colN < table.GetLength(0); ++colN)
            {
                float width = 0.0f;
                for (rowN = 0; rowN < table.GetLength(1); ++rowN)
                {
                    float x = Text.CalcSize(table[colN, rowN]).x;
                    if (x > width)
                        width = x;
                }
                colWidths.Add(width + 4f);
            }
            float columnsTotalWidth = colWidths.Sum();

            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Tiny;
            bool even = false;
            float th = rect.y; // total height
            for (int i = 0; i < table.GetLength(1); ++i)
            {
                float x = rect.x;
                for (int j = 0; j < table.GetLength(0); ++j)
                {
                    Rect field = new Rect(x, th, colWidths[j], rowHeights[i]);
                    if (even) DevGUI.DrawLightHighlight(field);
                    Widgets.Label(field, table[j, i]);
                    x += colWidths[j];
                }
                th += rowHeights[i];
                even = !even;
            }
            Text.Font = oldFont;

            return new Rect(rect.x, rect.y, columnsTotalWidth, rowsTotalHeight);
        }
    }
}
