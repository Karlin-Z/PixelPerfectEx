using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.ConfigModule;
using static PixelPerfectEx.Drawing.IDrawData;

namespace PixelPerfectEx.Drawing.Types
{
    internal abstract class DrawBase:IDrawData
    {
        public string  Name{ get;}


        public AoeTypeEnum AoeType { get; set; }


        public CentreTypeEnum CentreType { get; set; }


        public object CentreValue { get; set; }

        [JsonIgnore]public Vector3 Centre
        {
            get{
                switch (CentreType)
                {
                    case CentreTypeEnum.ActorId:
                        var id = (uint)CentreValue;
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.ObjectId == id)
                                return ac.Position;
                        }
                        break;
                    case CentreTypeEnum.ActorName:
                        var name = (string)CentreValue;
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.Name.ToString() == name)
                                return ac.Position;
                        }
                        break;
                    case CentreTypeEnum.PostionValue:
                        return (Vector3)CentreValue;
                    default:
                        break;
                }
                return new(float.NaN, float.NaN, float.NaN);
            }
        }


        public TrackTypeEnum TrackType { get; set; }
        public object TrackValue { get; set; }




        public uint Color { get; set; }


        public float Delay { get; set; }

        public float During { get; set; }

        [JsonIgnore]public DateTime StartTime { get; set; }

        [JsonIgnore]public DateTime EndTime { get; set; }
        protected DrawBase(JObject jo) 
        {
            Name = jo.TryGetValue(nameof(Name), out var _name) ? _name.ToObject<string>() :"";
            CentreType= jo.TryGetValue(nameof(CentreType), out var _ct) ? _ct.ToObject<CentreTypeEnum>() : CentreTypeEnum.ActorId;
            jo.TryGetValue(nameof(CentreValue), out var _cv);
            switch (CentreType)
            {
                case CentreTypeEnum.ActorId:
                    CentreValue= _cv.ToObject<uint>();
                    break;
                case CentreTypeEnum.ActorName:
                    CentreValue = _cv.ToObject<string>();
                    break;
                case CentreTypeEnum.PostionValue:
                    CentreValue = _cv.ToObject<Vector3>();
                    break;
            }
            TrackType= jo.TryGetValue(nameof(TrackType), out var _tt) ? _tt.ToObject<TrackTypeEnum>() : TrackTypeEnum.None;
            jo.TryGetValue(nameof(TrackValue), out var _tv);
            switch (TrackType)
            {
                case TrackTypeEnum.None:
                    TrackValue = 0;
                    break;
                case TrackTypeEnum.IdTrack:
                    TrackValue= _tv.ToObject<uint>();
                    break;
                case TrackTypeEnum.NameTrack:
                    TrackValue = _tv.ToObject<string>();
                    break;
                case TrackTypeEnum.Nearest:
                    TrackValue = _tv.ToObject<uint>();
                    break;
                case TrackTypeEnum.Farest:
                    TrackValue = _tv.ToObject<uint>();
                    break;
            }
            Color= jo.TryGetValue(nameof(Color), out var _col) ? _col.ToObject<uint>() : 0;
            Delay= jo.TryGetValue(nameof(Delay), out var _de) ? _de.ToObject<float>() : 0;
            During= jo.TryGetValue(nameof(During), out var _du) ? _du.ToObject<float>() : 0;

            StartTime = DateTime.Now.AddSeconds(Delay);
            EndTime = StartTime.AddSeconds(During);
        }

        public abstract void Draw(ImDrawListPtr drawList);
        

    }
}
