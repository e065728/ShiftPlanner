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

            // 週毎の勤務時間と連続勤務日数を記録する辞書
            var weeklyHours = new Dictionary<int, Dictionary<DateTime, double>>();
            var lastWorkDate = new Dictionary<int, DateTime?>();
            var consecutiveDays = new Dictionary<int, int>();

            foreach (var m in members)
            {
                weeklyHours[m.Id] = new Dictionary<DateTime, double>();
                lastWorkDate[m.Id] = null;
                consecutiveDays[m.Id] = 0;
            }

            int rotationIndex = 0; // メンバー選択用インデックス

            foreach (var frame in frames.OrderBy(f => f.Date))
            {
                var frameHours = (frame.ShiftEnd - frame.ShiftStart).TotalHours;
                var weekStart = GetWeekStart(frame.Date);

                var eligible = members.Where(m =>
                    (m.AvailableDays == null || m.AvailableDays.Contains(frame.Date.DayOfWeek)) &&
                    m.AvailableFrom <= frame.ShiftStart &&
                    m.AvailableTo >= frame.ShiftEnd &&
                    !ExceedsWeeklyHours(m, weekStart, frameHours, weeklyHours) &&
                    !ExceedsConsecutiveDays(m, frame.Date, lastWorkDate, consecutiveDays))
                    .ToList();

                if (eligible.Count == 0)
                {
                    // 条件に合うメンバーがいない場合は制約を無視して全メンバー対象
                    eligible = members.ToList();
                }

                var assigned = new List<Member>();
                for (int i = 0; i < frame.RequiredNumber; i++)
                {
                    var member = eligible[(rotationIndex + i) % eligible.Count];
                    assigned.Add(member);

                    // 勤務時間・連続勤務情報を更新
                    UpdateWeeklyHours(member, weekStart, frameHours, weeklyHours);
                    UpdateConsecutiveDays(member, frame.Date, lastWorkDate, consecutiveDays);
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

        /// <summary>
        /// 指定した日付が属する週の開始日(月曜日)を返します。
        /// </summary>
        private static DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }

        private static bool ExceedsWeeklyHours(Member member, DateTime weekStart, double addHours,
            Dictionary<int, Dictionary<DateTime, double>> weeklyHours)
        {
            if (member?.Constraints == null)
            {
                return false;
            }

            double max = member.Constraints.MaxWeeklyHours;
            if (max <= 0)
            {
                return false; // 制限なし
            }

            double current = 0;
            if (weeklyHours.TryGetValue(member.Id, out var weekDict))
            {
                weekDict.TryGetValue(weekStart, out current);
            }

            return current + addHours > max;
        }

        private static bool ExceedsConsecutiveDays(Member member, DateTime date,
            Dictionary<int, DateTime?> lastWorkDate, Dictionary<int, int> consecutiveDays)
        {
            if (member?.Constraints == null)
            {
                return false;
            }

            int max = member.Constraints.MaxConsecutiveDays;
            if (max <= 0)
            {
                return false; // 制限なし
            }

            if (!lastWorkDate.TryGetValue(member.Id, out var last) || !last.HasValue)
            {
                return false;
            }

            var diff = (date.Date - last.Value.Date).TotalDays;
            if (diff == 1)
            {
                return consecutiveDays[member.Id] + 1 > max;
            }

            return false;
        }

        private static void UpdateWeeklyHours(Member member, DateTime weekStart, double addHours,
            Dictionary<int, Dictionary<DateTime, double>> weeklyHours)
        {
            if (!weeklyHours.TryGetValue(member.Id, out var weekDict))
            {
                weekDict = new Dictionary<DateTime, double>();
                weeklyHours[member.Id] = weekDict;
            }

            if (!weekDict.ContainsKey(weekStart))
            {
                weekDict[weekStart] = 0;
            }

            weekDict[weekStart] += addHours;
        }

        private static void UpdateConsecutiveDays(Member member, DateTime date,
            Dictionary<int, DateTime?> lastWorkDate, Dictionary<int, int> consecutiveDays)
        {
            if (!lastWorkDate.ContainsKey(member.Id))
            {
                lastWorkDate[member.Id] = null;
                consecutiveDays[member.Id] = 0;
            }

            var lastDate = lastWorkDate[member.Id];
            if (lastDate.HasValue)
            {
                var diff = (date.Date - lastDate.Value.Date).TotalDays;
                if (diff == 1)
                {
                    consecutiveDays[member.Id] = consecutiveDays[member.Id] + 1;
                }
                else if (diff > 1)
                {
                    consecutiveDays[member.Id] = 1;
                }
                // 同じ日の場合はカウントを変えない
            }
            else
            {
                consecutiveDays[member.Id] = 1;
            }

            lastWorkDate[member.Id] = date.Date;
        }
    }
}

