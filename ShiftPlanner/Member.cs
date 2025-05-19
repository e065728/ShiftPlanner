
using System;
using System.Collections.Generic;

namespace ShiftPlanner
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DayOfWeek> AvailableDays { get; set; }
        public TimeSpan AvailableFrom { get; set; }
        public TimeSpan AvailableTo { get; set; }
        public List<string> Skills { get; set; }
    }
}
