
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.DrawFunc
{
    internal class DrawDonut
    {
        internal static void Draw(ImDrawListPtr drawList, Vector3 centre, float radius, float innerRadius, uint col)
        {
            var seg = 2 * MathF.PI / Service.Configuration.Segment;
            var outerWorldPoint = new Vector3[Service.Configuration.Segment];
            var innerWorldPoint = new Vector3[Service.Configuration.Segment];

            var outerPoints = new Vector2[Service.Configuration.Segment];
            var innerPoints = new Vector2[Service.Configuration.Segment];


            for (int i = 0; i < Service.Configuration.Segment; i++)
            {
                outerWorldPoint[i] = 
                    new(centre.X + radius * MathF.Sin(i * seg),
                        centre.Y,
                        centre.Z + radius * MathF.Cos(i * seg));
                innerWorldPoint[i] =
                     new(centre.X + innerRadius * MathF.Sin(i * seg),
                        centre.Y,
                        centre.Z + innerRadius * MathF.Cos(i * seg));
            }
            
            for (int i = 0;i < Service.Configuration.Segment; i++)
            {
                var p1 =PointHelper.GetPionts(innerWorldPoint[i], outerWorldPoint[i]);
                var p2= PointHelper.GetPionts(outerWorldPoint[(i+1)% Service.Configuration.Segment], innerWorldPoint[(i + 1) % Service.Configuration.Segment]);

                p1.ForEach(p => drawList.PathLineTo(p));
                p2.ForEach(p => drawList.PathLineTo(p));
                drawList.PathFillConvex(col);

                innerPoints[i]=p1.FirstOrDefault();
                outerPoints[i]= p1.LastOrDefault();
            }

            if (Service.Configuration.Strock)
            {
                var colStock = ImGui.ColorConvertU32ToFloat4(col);
                colStock.W = 1;
                // outer
                for (int i = 0; i < Service.Configuration.Segment; i++)
                    if (!float.IsNaN(outerPoints[i].X)) drawList.PathLineTo(outerPoints[i]);
                drawList.PathStroke(ImGui.ColorConvertFloat4ToU32(colStock), ImDrawFlags.Closed, Service.Configuration.StrockWidth);

                // inner
                for (int i = 0; i < Service.Configuration.Segment; i++)
                    if (!float.IsNaN(innerPoints[i].X)) drawList.PathLineTo(innerPoints[i]);
                drawList.PathStroke(ImGui.ColorConvertFloat4ToU32(colStock), ImDrawFlags.Closed, Service.Configuration.StrockWidth);
            }
        }
        
    }
}
