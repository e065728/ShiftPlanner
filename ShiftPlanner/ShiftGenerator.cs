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
        /// 勤務可能日の条件を満たすメンバーが不足している場合は、
        /// 全メンバーからランダムに選択して割り当てます。
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

            // ランダム選択用のインスタンス
            var random = new Random();

            foreach (var frame in frames.OrderBy(f => f.Date))
            {
                var eligible = members.Where(m =>
                    (m.AvailableDays == null || m.AvailableDays.Contains(frame.Date.DayOfWeek)) &&
                    (frame.Date.DayOfWeek != DayOfWeek.Saturday || m.WorksOnSaturday) &&
                    (frame.Date.DayOfWeek != DayOfWeek.Sunday || m.WorksOnSunday))
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

                // 希望者を優先的に割り当て
                foreach (var m in priorityMembers)
                {
                    if (assigned.Count >= frame.RequiredNumber)
                    {
                        break;
                    }
                    assigned.Add(m);
                }

                // 残りの枠はランダムに選択
                if (others.Count > 0)
                {
                    var shuffled = others.OrderBy(_ => random.Next()).ToList();
                    foreach (var m in shuffled)
                    {
                        if (assigned.Count >= frame.RequiredNumber)
                        {
                            break;
                        }
                        assigned.Add(m);
                    }
                }

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

