using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.DrawFunc
{

    internal class DrawLine
    {
        internal static void Draw(ImDrawListPtr drawList, Vector3 start, Vector3 end, float thikness, uint col)
        {
            PointHelper.GetPionts(start, end).ForEach(p =>
            {
                drawList.PathLineTo(p);
            });
            drawList.PathStroke(col, ImDrawFlags.None, thikness);
            drawList.PathClear();

        }
    }
}
