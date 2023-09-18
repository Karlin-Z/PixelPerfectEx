using ImGuiNET;
using System;

namespace PixelPerfectEx.Drawing
{
    internal interface IDrawData
    {
        public string Name { get; }
        public AoeTypeEnum AoeType { get;}
        public uint Color { get;}
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsExpired => EndTime < DateTime.Now;
        public bool IsVisible => (EndTime > DateTime.Now && DateTime.Now > StartTime);
        public void Draw(ImDrawListPtr drawList);


        public enum AoeTypeEnum
        {
            None = 0,
            Circle = 1,
            Donut = 2,
            Rect = 3,
            Sector = 4,
            Repel = 5,
            Back = 6,
            TP = 7,
            Link = 8,
            Cros=9,
        }
        public enum CentreTypeEnum
        {
            ActorId = 1,
            ActorName = 2,
            PostionValue = 3,
        }
        public enum TrackTypeEnum
        {
            None = 1,
            IdTrack = 2,
            NameTrack = 3,
            Nearest = 4,
            Farest = 5
        }
    }
}
