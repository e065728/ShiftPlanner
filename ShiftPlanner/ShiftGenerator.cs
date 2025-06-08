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
        public static List<ShiftAssignment> GenerateBaseShift(
            List<ShiftFrame> frames,
            List<Member> members,
            List<ShiftRequest> requests)
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
                    m.AvailableTo >= frame.ShiftEnd)
                    .ToList();

                // 休み希望があれば割当対象から除外
                if (requests != null)
                {
                    eligible = eligible
                        .Where(m => !requests.Any(r => r.MemberId == m.Id && r.Date.Date == frame.Date.Date && r.IsHolidayRequest))
                        .ToList();
                }

                if (eligible.Count == 0)
                {
                    // 条件に合うメンバーがいない場合は全メンバー対象
                    eligible = members.ToList();
                }

                // 勤務希望者を優先的に割り当て
                var priorityMembers = new List<Member>();
                if (requests != null)
                {
                    priorityMembers = eligible
                        .Where(m => requests.Any(r => r.MemberId == m.Id && r.Date.Date == frame.Date.Date && !r.IsHolidayRequest))
                        .ToList();
                }

                var others = eligible.Except(priorityMembers).ToList();

                var assigned = new List<Member>();

                foreach (var m in priorityMembers)
                {
                    if (assigned.Count >= frame.RequiredNumber)
                    {
                        break;
                    }
                    assigned.Add(m);
                }

                for (int i = 0; assigned.Count < frame.RequiredNumber && others.Count > 0 && i < others.Count; i++)
                {
                    var member = others[(rotationIndex + i) % others.Count];
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

