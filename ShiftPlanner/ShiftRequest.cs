
using System;

namespace ShiftPlanner
{
    public class ShiftRequest
    {
        public int MemberId { get; set; }
        public DateTime Date { get; set; }
        public bool IsHolidayRequest { get; set; }
    }
}
