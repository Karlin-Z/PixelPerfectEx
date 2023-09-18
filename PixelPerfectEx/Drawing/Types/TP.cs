using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.Drawing.Types
{
    internal class TP : DrawBase
    {
        public float Length { get; set; }
        public float Rotation { get; set; }
        public float Thikness { get; set; }
        public TP(JObject jo) : base(jo)
        {
            Length = jo.TryGetValue(nameof(Length), out var _l) ? _l.ToObject<float>() : 0;
            Rotation = jo.TryGetValue(nameof(Rotation), out var _r) ? _r.ToObject<float>() : 0;
            Thikness = jo.TryGetValue(nameof(Thikness), out var _le) ? _le.ToObject<float>() : Service.Configuration.DefaultThickness;
        }

        public override void Draw(ImDrawListPtr drawList)
        {
        }
    }
}
