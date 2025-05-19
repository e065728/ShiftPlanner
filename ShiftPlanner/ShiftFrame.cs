
using System;

namespace ShiftPlanner
{
    public class ShiftFrame
    {
        public DateTime Date { get; set; }
        public string ShiftType { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public TimeSpan ShiftEnd { get; set; }
        public int RequiredNumber { get; set; }
    }
}
