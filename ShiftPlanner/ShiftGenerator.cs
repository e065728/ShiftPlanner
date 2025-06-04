using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftPlanner
{
    public static class ShiftGenerator
    {
        /// <summary>
        /// シフト枠に必要な人数を満たすようメンバーを割り当てます。
        /// </summary>
        /// <remarks>
        /// 勤務可能日の条件を満たすメンバーが不足している場合、
        /// 全メンバーをローテーションして割り当てます。
        /// </remarks>
        public static List<ShiftAssignment> GenerateBaseShift(List<ShiftFrame> frames, List<Member> members)
        {
            var assignments = new List<ShiftAssignment>();
            if (frames == null || members == null)
            {
                return assignments; // nullチェック
            }

            int rotationIndex = 0; // メンバー選択用インデックス

            foreach (var frame in frames.OrderBy(f => f.Date))
            {
                var eligible = members.Where(m =>
                    (m.AvailableDays == null || m.AvailableDays.Contains(frame.Date.DayOfWeek)) &&
                    m.AvailableFrom <= frame.ShiftStart &&
                    m.AvailableTo >= frame.ShiftEnd &&
                    (m.DesiredHolidays == null || !m.DesiredHolidays.Contains(frame.Date.Date)))
                    .ToList();

                if (eligible.Count == 0)
                {
                    // 条件に合うメンバーがいない場合は全メンバー対象
                    eligible = members.ToList();
                }

                var assigned = new List<Member>();
                for (int i = 0; i < frame.RequiredNumber; i++)
                {
                    var member = eligible[(rotationIndex + i) % eligible.Count];
                    assigned.Add(member);
                }

                rotationIndex++;

                assignments.Add(new ShiftAssignment
                {
                    Date = frame.Date,
                    ShiftType = frame.ShiftType,
                    RequiredNumber = frame.RequiredNumber,
                    AssignedMembers = assigned
                });
            }

            return assignments;
        }
    }
}

