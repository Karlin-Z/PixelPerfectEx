
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.DrawFunc
{
    internal class DrawCircle
    {
        internal static void Draw(ImDrawListPtr drawList, Vector3 centre, float radius, uint col)
        {
            var points = new Vector2[Service.Configuration.Segment];
            var seg = 2 * MathF.PI / Service.Configuration.Segment;
            for (int i = 0; i < Service.Configuration.Segment; i++)
            {
                Vector3 v3 = new Vector3(
                        centre.X + radius * MathF.Sin(i * seg),
                        centre.Y,
                        centre.Z + radius * MathF.Cos(i * seg));
                PPexProjection.WorldToScreen(v3, out Vector2 p, out Vector3 pr);

                // Dont add points that may be projected to weird positions
                if (pr.Z < -.1f) points[i] = new(float.NaN, float.NaN);
                else
                {
                    points[i] = p;
                    drawList.PathLineTo(p);
                }
            }
            drawList.PathFillConvex(col);
            if (Service.Configuration.Strock)
            {
                foreach (var p in points)
                    if (!float.IsNaN(p.X)) drawList.PathLineTo(p);
                var colStock = ImGui.ColorConvertU32ToFloat4(col);
                colStock.W = 1;
                drawList.PathStroke(ImGui.ColorConvertFloat4ToU32(colStock), ImDrawFlags.Closed, Service.Configuration.StrockWidth);
            }
            drawList.PathClear();
        }
    }
}
