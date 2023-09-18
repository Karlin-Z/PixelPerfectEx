using Dalamud.Utility.Numerics;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.DrawFunc
{
    internal class DrawSector
    {
        internal static void Draw(ImDrawListPtr drawList, Vector3 centre, float Radius, float Angle, float rotation, uint col)
        {
            var radian = (Angle / 180f) * MathF.PI;
            int diffnum = Service.Configuration.Segment;
            var diffAngle = radian / diffnum;
            var beginRadian = rotation - radian / 2;
            var points = new List<Vector2>();
            for (int i = 1; i < diffnum - 1; i++)
            {
                var p = new Vector3(
                    centre.X + (Radius * (float)MathF.Sin(diffAngle * i + beginRadian)),
                    centre.Y,
                    centre.Z + (Radius * (float)MathF.Cos(diffAngle * i + beginRadian)));
                PPexProjection.WorldToScreen(p, out var pIncreen, out var pr);
                if (pr.Z > -1) points.Add(pIncreen);
            }
            var lep = new Vector3(
                    centre.X + (Radius * (float)MathF.Sin(beginRadian)),
                    centre.Y,
                    centre.Z + (Radius * (float)MathF.Cos(beginRadian)));
            var side1LinePoints = PointHelper.GetPionts(centre, lep);

            var rep = new Vector3(
                    centre.X + (Radius * (float)MathF.Sin(beginRadian + radian)),
                    centre.Y,
                    centre.Z + (Radius * (float)MathF.Cos(beginRadian + radian)));
            var side2LinePoints = PointHelper.GetPionts(rep, centre);
            side2LinePoints.RemoveAt(side2LinePoints.Count - 1);


            side1LinePoints.ForEach(p => drawList.PathLineTo(p));
            points.ForEach(p => drawList.PathLineTo(p));
            side2LinePoints.ForEach(p => drawList.PathLineTo(p));
            drawList.PathFillConvex(col);


            if (Service.Configuration.Strock)
            {
                var colStock = ImGui.ColorConvertU32ToFloat4(col).WithW(1);
                side1LinePoints.ForEach(p => drawList.PathLineTo(p));
                points.ForEach(p => drawList.PathLineTo(p));
                side2LinePoints.ForEach(p => drawList.PathLineTo(p));
                drawList.PathStroke(ImGui.ColorConvertFloat4ToU32(colStock), ImDrawFlags.Closed, Service.Configuration.StrockWidth);
            }
        }
    }
}
