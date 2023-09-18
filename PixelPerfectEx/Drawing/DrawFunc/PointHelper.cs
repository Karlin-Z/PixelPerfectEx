using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.DrawFunc
{
    internal class PointHelper
    {
        internal static List<Vector2> GetPionts(Vector3 start, Vector3 goal)
        {
            var points = new List<Vector2>();
            if (float.IsNaN(start.X) || float.IsNaN(start.Y) || float.IsNaN(start.Z) || float.IsNaN(goal.X) || float.IsNaN(goal.Y) || float.IsNaN(goal.Z)) return points;
            var nor = Vector3.Normalize(goal - start);
            float dis = MathF.Floor(Vector3.Distance(start, goal));
            for (int i = 0; i <= dis; i++)
            {
                var pin = start + nor * i;
                PPexProjection.WorldToScreen(pin, out Vector2 p, out Vector3 p1r);
                // Dont add points that may be projected to weird positions
                if (p1r.Z >= -.1f)
                {
                    points.Add(p);
                }
            }
            PPexProjection.WorldToScreen(goal, out Vector2 pe, out Vector3 p2r);
            // Dont add points that may be projected to weird positions
            if (p2r.Z >= -.1f)
            {
                points.Add(pe);
            }
            return points;
        }
    }
}
