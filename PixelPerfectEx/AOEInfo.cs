using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx
{
    
    public class AOEInfo
    {
        public enum AoeTypeEnum
        {
            None=0,
            Round=1,
            Ring=2,
            Rect=3,
            Sector=4,
            Repel=5,
            Back=6,
            TP=7,
            Link=8
        }
        public enum PostionTypeEnum
        {
            ActorId=1,
            ActorName=2,
            PostionValue=3,
        }
        public enum TrackEnum
        {
            FixedAngle=1,
            IdTrack=2,
            NameTrack = 3,
            Nearest=4,
            Farest=5
        }

        public AoeTypeEnum AoeType { get; set; } = AoeTypeEnum.None;
        public PostionTypeEnum PostionType { get; set; }
        public uint ActorId { get; set; }
        public string ActorName { get; set; }
        public Vector3 Postion { get; set; }

        public PostionTypeEnum PostionType2 { get; set; }
        public uint ActorId2 { get; set; }
        public string ActorName2 { get; set; }
        public Vector3 Postion2 { get; set; }
        public float Rotation { get; set; }
        public TrackEnum TrackMode { get; set; }
        public uint TrackId { get; set; }
        public string TrackName { get; set; }
        public float OuterRadius { get; set; }
        public float InnerRadius { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Thickness { get; set; }
        public float SectorAngle { get; set; }
        public uint Color { get; set; }
        public uint CorrectColor { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public float Delay { get; set; }
        public float During { get; set; }



    }
}
