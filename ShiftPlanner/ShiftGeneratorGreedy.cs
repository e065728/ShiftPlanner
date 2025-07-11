using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftPlanner
{
    /// <summary>
    /// 貪欲法を用いてシフト割り当てを行うクラス。
    /// </summary>
    public static class ShiftGeneratorGreedy
    {
        private static readonly Random _rand = new Random();

        /// <summary>
        /// シフト割り当て処理で使用する内部クラス。
        /// </summary>
        private class MemberState
        {
            public Member Member { get; }
            public int AssignedCount { get; set; }
            public int WorkStreak { get; set; }

            public MemberState(Member member)
            {
                Member = member;
            }
        }

        /// <summary>
        /// 勤務枠情報を表す構造体。
        /// 必要に応じて SkillGroup 名を保持します。
        /// </summary>
        private struct Slot
        {
            public DateTime Date { get; set; }
            public string ShiftName { get; set; }
            public string? RequiredSkill { get; set; }
        }

        /// <summary>
        /// 指定した条件からシフト表を生成します。
        /// </summary>
        /// <param name="members">メンバー一覧</param>
        /// <param name="baseDate">対象月の開始日</param>
        /// <param name="days">対象月の日数</param>
        /// <param name="skillRequirements">日付ごとのスキルグループ必要人数</param>
        /// <param name="shiftRequirements">日付ごとの勤務時間帯必要人数</param>
        /// <param name="requests">シフト希望</param>
        /// <param name="skillGroups">スキルグループ一覧</param>
        /// <param name="shiftTimes">勤務時間一覧</param>
        /// <param name="minHolidayCount">最低休日日数</param>
        /// <returns>メンバーIDと日付をキーとした割当結果</returns>
        public static Dictionary<int, Dictionary<DateTime, string>> Generate(
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

            if (members == null || members.Count == 0)
            {
                return result;
            }

            skillRequirements ??= new Dictionary<DateTime, Dictionary<string, int>>();
            shiftRequirements ??= new Dictionary<DateTime, Dictionary<string, int>>();
            requests ??= new List<ShiftRequest>();
            skillGroups ??= new List<SkillGroup>();
            shiftTimes ??= new List<ShiftTime>();

            // メンバー状態の初期化
            var states = members.ToDictionary(m => m.Id, m => new MemberState(m));

            // メンバーごとの勤務可能枠数(柔軟度)を計算
            var 柔軟度マップ = new Dictionary<int, int>();
            try
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var m = members[i];
                    int 枠数 = 0;
                    for (int d = 0; d < days; d++)
                    {
                        var 日付 = baseDate.AddDays(d);
                        bool 出勤可能 = m.AvailableDays.Contains(日付.DayOfWeek) &&
                            (日付.DayOfWeek != DayOfWeek.Saturday || m.WorksOnSaturday) &&
                            (日付.DayOfWeek != DayOfWeek.Sunday || m.WorksOnSunday);

                        if (出勤可能)
                        {
                            枠数 += m.AvailableShiftNames?.Count ?? 0;
                        }
                    }
                    柔軟度マップ[m.Id] = 枠数;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Generate] 柔軟度計算中にエラーが発生しました: {ex.Message}");
            }

            // 各日の休み許容量を計算
            var dailyHolidayCapacity = new Dictionary<int, int>();
            for (int d = 0; d < days; d++)
            {
                var date = baseDate.AddDays(d);
                int required = 0;
                if (shiftRequirements.TryGetValue(date, out var sr) && sr != null)
                {
                    required = sr.Values.Sum();
                }
                int capacity = members.Count - required;
                if (capacity < 0)
                {
                    capacity = 0;
                }
                dailyHolidayCapacity[d] = capacity;
            }

            // メンバーごとの現在休日数をカウント
            var holidayCounts = members.ToDictionary(m => m.Id, m =>
                requests.Count(r => r.MemberId == m.Id && r.IsHolidayRequest && r.Date.Year == baseDate.Year && r.Date.Month == baseDate.Month));

            // 最低休日日数を満たすための追加休み候補を生成
            var extraHolidays = new Dictionary<int, HashSet<int>>();
            foreach (var m in members)
            {
                int need = Math.Max(0, minHolidayCount - holidayCounts[m.Id]);
                var candidateDays = Enumerable.Range(0, days)
                    .Where(d => !requests.Any(r => r.MemberId == m.Id && r.Date.Date == baseDate.AddDays(d).Date))
                    .Where(d => dailyHolidayCapacity.ContainsKey(d) && dailyHolidayCapacity[d] > 0)
                    .ToList();
                var set = new HashSet<int>();
                while (need > 0 && candidateDays.Count > 0)
                {
                    int idx = _rand.Next(candidateDays.Count);
                    int dayIndex = candidateDays[idx];
                    set.Add(dayIndex);
                    dailyHolidayCapacity[dayIndex]--;
                    holidayCounts[m.Id]++;
                    candidateDays.RemoveAt(idx);
                    candidateDays = candidateDays.Where(c => dailyHolidayCapacity[c] > 0 && !set.Contains(c)).ToList();
                    need--;
                }
                extraHolidays[m.Id] = set;
            }

            // 各メンバーの日付別結果を初期化
            foreach (var m in members)
            {
                var dict = new Dictionary<DateTime, string>();
                for (int i = 0; i < days; i++)
                {
                    dict[baseDate.AddDays(i)] = string.Empty;
                }
                result[m.Id] = dict;
            }

            // メイン処理 (Phase A-C)
            for (int d = 0; d < days; d++)
            {
                var date = baseDate.AddDays(d);
                var daySkillReq = skillRequirements.ContainsKey(date) ? new Dictionary<string, int>(skillRequirements[date]) : new Dictionary<string, int>();
                var dayShiftReq = shiftRequirements.ContainsKey(date) ? new Dictionary<string, int>(shiftRequirements[date]) : new Dictionary<string, int>();

                var dayAssignments = new Dictionary<int, string>();
                var candidates = new List<Member>();

                // Phase-A: 勤務可能メンバーの抽出と休日処理
                foreach (var m in members)
                {
                    var req = requests.FirstOrDefault(r => r.MemberId == m.Id && r.Date.Date == date.Date);
                    if (req != null && req.IsHolidayRequest)
                    {
                        result[m.Id][date] = "希休";
                        states[m.Id].WorkStreak = 0;
                        if (dailyHolidayCapacity.ContainsKey(d))
                        {
                            dailyHolidayCapacity[d]--;
                        }
                        holidayCounts[m.Id]++;
                        continue;
                    }

                    if (extraHolidays.TryGetValue(m.Id, out var set) && set.Contains(d))
                    {
                        result[m.Id][date] = "休";
                        states[m.Id].WorkStreak = 0;
                        continue;
                    }

                    bool canWork = m.AvailableDays.Contains(date.DayOfWeek) &&
                        (date.DayOfWeek != DayOfWeek.Saturday || m.WorksOnSaturday) &&
                        (date.DayOfWeek != DayOfWeek.Sunday || m.WorksOnSunday);

                    int maxConsecutive = m.Constraints?.MaxConsecutiveDays ?? 5;
                    if (states[m.Id].WorkStreak >= maxConsecutive)
                    {
                        canWork = false;
                    }

                    if (!canWork)
                    {
                        result[m.Id][date] = "休";
                        states[m.Id].WorkStreak = 0;
                        if (dailyHolidayCapacity.ContainsKey(d))
                        {
                            dailyHolidayCapacity[d]--;
                        }
                        holidayCounts[m.Id]++;
                        continue;
                    }

                    // とりあえず候補として保持
                    candidates.Add(m);
                }

                // Phase-B: スキルグループ要件の充足
                foreach (var sg in skillGroups)
                {
                    int need = daySkillReq.ContainsKey(sg.Name) ? daySkillReq[sg.Name] : 0;
                    while (need > 0)
                    {
                        var target = candidates
                            .Where(m => m.SkillGroup == sg.Name && !dayAssignments.ContainsKey(m.Id))
                            .OrderBy(m => 柔軟度マップ.ContainsKey(m.Id) ? 柔軟度マップ[m.Id] : int.MaxValue)
                            .ThenBy(m => states[m.Id].AssignedCount)
                            .ThenBy(_ => _rand.Next())
                            .FirstOrDefault();

                        if (target == null)
                        {
                            break;
                        }

                        var possibleShifts = target.AvailableShiftNames
                            .Where(n => dayShiftReq.ContainsKey(n) && dayShiftReq[n] > 0)
                            .ToList();

                        if (possibleShifts.Count == 0)
                        {
                            break;
                        }

                        string shiftName = possibleShifts[_rand.Next(possibleShifts.Count)];
                        dayShiftReq[shiftName]--;
                        dayAssignments[target.Id] = shiftName;
                        states[target.Id].AssignedCount++;
                        states[target.Id].WorkStreak++;
                        need--;
                    }
                }

                // Phase-C: 残りの勤務枠を割り当て
                foreach (var st in shiftTimes)
                {
                    int need = dayShiftReq.ContainsKey(st.Name) ? dayShiftReq[st.Name] : 0;
                    while (need > 0)
                    {
                        var target = candidates
                            .Where(m => !dayAssignments.ContainsKey(m.Id) && m.AvailableShiftNames.Contains(st.Name))
                            .OrderBy(m => 柔軟度マップ.ContainsKey(m.Id) ? 柔軟度マップ[m.Id] : int.MaxValue)
                            .ThenBy(m => states[m.Id].AssignedCount)
                            .ThenBy(_ => _rand.Next())
                            .FirstOrDefault();

                        if (target == null)
                        {
                            break;
                        }

                        dayAssignments[target.Id] = st.Name;
                        states[target.Id].AssignedCount++;
                        states[target.Id].WorkStreak++;
                        need--;
                    }
                }

                // Phase-D: (未実装)

                // 結果の書き込み
                foreach (var m in members)
                {
                    if (dayAssignments.TryGetValue(m.Id, out var shiftName))
                    {
                        bool preferWork = requests.Any(r => r.MemberId == m.Id && r.Date.Date == date.Date && !r.IsHolidayRequest);
                        result[m.Id][date] = preferWork ? $"希{shiftName}" : shiftName;
                    }
                    else if (string.IsNullOrEmpty(result[m.Id][date]))
                    {
                        result[m.Id][date] = "休";
                        states[m.Id].WorkStreak = 0;
                    }
                }
            }

            return result;
        }
    }
}
