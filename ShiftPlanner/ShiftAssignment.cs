using System;
using System.Collections.Generic;

namespace ShiftPlanner
{
    public class ShiftAssignment
    {
        public DateTime Date { get; set; }
        public string ShiftType { get; set; }
        public int RequiredNumber { get; set; }
        public List<Member> AssignedMembers { get; set; } = new List<Member>();
        public bool Shortage => AssignedMembers.Count < RequiredNumber;
        public bool Excess => AssignedMembers.Count > RequiredNumber;
    }
}

