using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ShiftPlanner
{
    /// <summary>
    /// ランダム方式による簡易シフト自動生成クラス。
    /// </summary>
    public class ShiftGenerator : IRosterAlgorithm
    {
        // 乱数生成用の共有インスタンス
        private static readonly Random _rand = new Random();

        /// <summary>
        /// Fisher–Yates アルゴリズムでリストをシャフルします。
        /// </summary>
        /// <typeparam name="T">リストの要素型</typeparam>
        /// <param name="list">シャフル対象のリスト</param>
        public static void Shuffle<T>(IList<T>? list)
        {
            if (list == null || list.Count <= 1)
            {
                return;
            }

            try
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = _rand.Next(i + 1);
                    T temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("[Shuffle] シャフル中にエラーが発生しました", ex);
            }
        }
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
            List<ShiftRequest>? requests)
        {
            var assignments = new List<ShiftAssignment>();
            if (frames == null || members == null)
            {
                return assignments; // nullチェック
            }

            requests ??= new List<ShiftRequest>();

            // 乱数生成用インスタンスはクラスで共有

            try
            {
                foreach (var frame in frames.OrderBy(f => f.Date))
                {
                var eligible = members.Where(m =>
                    (m.AvailableDays == null || m.AvailableDays.Contains(frame.Date.DayOfWeek)) &&
                    (frame.Date.DayOfWeek != DayOfWeek.Saturday || m.WorksOnSaturday) &&
                    (frame.Date.DayOfWeek != DayOfWeek.Sunday || m.WorksOnSunday))
                    .ToList();

                // 休み希望(勤務希望以外)があれば割当対象から除外
                if (requests != null)
                {
                    eligible = eligible
                        .Where(m => !requests.Any(r => r.MemberId == m.Id && r.Date.Date == frame.Date.Date && r.種別 != 申請種別.勤務希望))
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
                        .Where(m => requests.Any(r => r.MemberId == m.Id && r.Date.Date == frame.Date.Date && r.種別 == 申請種別.勤務希望))
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
                    Shuffle(others);
                    foreach (var m in others)
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
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("[GenerateBaseShift] 出力中にエラー", ex);
            }

            return assignments;
        }

        /// <summary>
        /// <see cref="IRosterAlgorithm"/> 実装。指定された条件からシフトを生成します。
        /// </summary>
        public Dictionary<int, Dictionary<DateTime, string>> Generate(
            List<Member>? members,
            DateTime baseDate,
            int days,
            Dictionary<DateTime, Dictionary<string, int>>? skillRequirements,
            Dictionary<DateTime, Dictionary<string, int>>? shiftRequirements,
            List<ShiftRequest>? requests,
            List<SkillGroup>? skillGroups,
            List<ShiftTime>? shiftTimes,
            int minHolidayCount)
        {
            var result = new Dictionary<int, Dictionary<DateTime, string>>();
            if (members == null || members.Count == 0 || shiftRequirements == null)
            {
                return result;
            }

            var shiftTimeMap = shiftTimes?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, ShiftTime>();
            var frames = new List<ShiftFrame>();

            foreach (var kv in shiftRequirements)
            {
                foreach (var sv in kv.Value)
                {
                    shiftTimeMap.TryGetValue(sv.Key, out var st);
                    frames.Add(new ShiftFrame
                    {
                        Date = kv.Key,
                        ShiftType = sv.Key,
                        ShiftStart = st?.Start ?? TimeSpan.Zero,
                        ShiftEnd = st?.End ?? TimeSpan.Zero,
                        RequiredNumber = sv.Value
                    });
                }
            }

            var assignments = GenerateBaseShift(frames, members, requests);

            foreach (var m in members)
            {
                var dict = new Dictionary<DateTime, string>();
                for (int d = 0; d < days; d++)
                {
                    dict[baseDate.AddDays(d)] = string.Empty;
                }
                result[m.Id] = dict;
            }

            foreach (var asg in assignments)
            {
                foreach (var m in asg.AssignedMembers)
                {
                    if (!result.ContainsKey(m.Id))
                    {
                        continue;
                    }
                    result[m.Id][asg.Date] = asg.ShiftType;
                }
            }

            return result;
        }
    }
}

