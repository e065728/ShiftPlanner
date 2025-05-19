using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftPlanner
{
    public static class ShiftGenerator
    {
        public static List<ShiftAssignment> GenerateBaseShift(List<ShiftFrame> frames, List<Member> members)
        {
            var assignments = new List<ShiftAssignment>();
            foreach (var frame in frames)
            {
                var eligible = members.Where(m =>
                    m.AvailableDays != null && m.AvailableDays.Contains(frame.Date.DayOfWeek) &&
                    m.AvailableFrom <= frame.ShiftStart &&
                    m.AvailableTo >= frame.ShiftEnd).ToList();

                var assignment = new ShiftAssignment
                {
                    Date = frame.Date,
                    ShiftType = frame.ShiftType,
                    RequiredNumber = frame.RequiredNumber,
                    AssignedMembers = eligible.Take(frame.RequiredNumber).ToList()
                };
                // If more than required, keep all to show excess
                if (eligible.Count > frame.RequiredNumber)
                {
                    assignment.AssignedMembers = eligible;
                }

                assignments.Add(assignment);
            }

            return assignments;
        }
    }
}

