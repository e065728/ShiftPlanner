using System;

namespace ShiftPlanner
{
    public class ChangeRecord
    {
        public DateTime Time { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Time:yyyy-MM-dd HH:mm:ss} {Description}";
        }
    }
}
