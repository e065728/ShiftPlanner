using System;
using System.Collections.Generic;
using System.Globalization;
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

            /// <summary>
            /// 前回割り当てた勤務名。
            /// </summary>
            public string LastShiftName { get; set; } = string.Empty;

            /// <summary>
            /// 週番号ごとの総労働時間。
            /// </summary>
            public Dictionary<int, double> WeeklyHours { get; } = new Dictionary<int, double>();

            public MemberState(Member member)
            {
                Member = member;
            }
        }

        /// <summary>
        /// 候補メンバーへの割り当てコストを計算します。
        /// </summary>
        private static double CalculateCost(
            Member member,
            MemberState state,
            DateTime date,
            string shiftName,
            string requiredSkillGroup,
            Dictionary<string, ShiftTime> shiftTimeMap)
        {
            double cost = 0;

            // スキル不一致ペナルティ
            if (!string.Equals(member.SkillGroup, requiredSkillGroup, StringComparison.OrdinalIgnoreCase))
            {
                cost += 50; // 大きめのペナルティ
            }

            if (!string.IsNullOrEmpty(shiftName) && shiftTimeMap.TryGetValue(shiftName, out var shift))
            {
                // 残業超過ペナルティ
                var cal = CultureInfo.CurrentCulture.Calendar;
                var week = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                state.WeeklyHours.TryGetValue(week, out var worked);
                var hours = (shift.End - shift.Start).TotalHours;
                var max = member.Constraints?.MaxWeeklyHours ?? double.MaxValue;
                if (worked + hours > max)
                {
                    cost += (worked + hours - max) * 10;
                }

                // 前後シフト切替ペナルティ
                if (!string.IsNullOrEmpty(state.LastShiftName) && shiftTimeMap.TryGetValue(state.LastShiftName, out var prev))
                {
                    var rest = (shift.Start - prev.End).TotalHours;
                    if (rest < 10) // 休息不足
                    {
                        cost += 20;
                    }
                    if (shiftName != state.LastShiftName)
                    {
                        cost += 1; // シフト種別が変わる小さなペナルティ
                    }
                }
            }

            // 負荷平準化: 割り当て数が多いほどペナルティ
            cost += state.AssignedCount * 0.5;

            return cost;
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

            var shiftTimeMap = shiftTimes
                .Where(st => st != null && !string.IsNullOrEmpty(st.Name))
                .GroupBy(st => st.Name)
                .Select(g => g.First())
                .ToDictionary(st => st.Name, st => st);

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
                SimpleLogger.Error("[Generate] 柔軟度計算中にエラーが発生しました", ex);
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
                requests.Count(r => r.MemberId == m.Id && r.種別 != 申請種別.勤務希望 && r.Date.Year == baseDate.Year && r.Date.Month == baseDate.Month));

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
                    if (req != null && req.種別 != 申請種別.勤務希望)
                    {
                        string name = req.種別 == 申請種別.希望休 ? "希休" : req.種別 == 申請種別.有休 ? "有休" : "健診";
                        result[m.Id][date] = name;
                        states[m.Id].WorkStreak = 0;
                        states[m.Id].LastShiftName = string.Empty;
                        states[m.Id].LastShiftName = string.Empty;
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
                        states[m.Id].LastShiftName = string.Empty;
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

                // 候補者数が少ないスキルグループから処理する
                var skillGroupOrder = skillGroups
                    .Select(sg => new {
                        Group = sg,
                        Count = candidates.Count(m => m.SkillGroup == sg.Name)
                    })
                    .OrderBy(x => x.Count)
                    .Select(x => x.Group)
                    .ToList();
                // スキルグループの割り当て順をログに出力
                SimpleLogger.Info(
                    $"[Generate] {date:yyyy-MM-dd} スキルグループ優先順: " +
                    string.Join(",", skillGroupOrder.Select(s => s.Name)));

                // Phase-B: スキルグループ要件の充足
                foreach (var sg in skillGroupOrder)
                {
                    int need = daySkillReq.ContainsKey(sg.Name) ? daySkillReq[sg.Name] : 0;
                    while (need > 0)
                    {
                        var target = candidates
                            .Where(m => m.SkillGroup == sg.Name && !dayAssignments.ContainsKey(m.Id))
                            .OrderBy(m => CalculateCost(m, states[m.Id], date, string.Empty, sg.Name, shiftTimeMap))
                            .ThenBy(m => 柔軟度マップ.ContainsKey(m.Id) ? 柔軟度マップ[m.Id] : int.MaxValue)
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
                        states[target.Id].LastShiftName = shiftName;
                        try
                        {
                            if (shiftTimeMap.TryGetValue(shiftName, out var stInfo))
                            {
                                var cal = CultureInfo.CurrentCulture.Calendar;
                                var weekIdx = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                                if (!states[target.Id].WeeklyHours.ContainsKey(weekIdx))
                                {
                                    states[target.Id].WeeklyHours[weekIdx] = 0;
                                }
                                states[target.Id].WeeklyHours[weekIdx] += (stInfo.End - stInfo.Start).TotalHours;
                            }
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.Error("[Generate] 労働時間更新エラー", ex);
                        }
                        need--;
                    }
                }

                // 候補者数が少ない勤務時間から処理する
                var shiftTimeOrder = shiftTimes
                    .Select(st => new {
                        Time = st,
                        Count = candidates.Count(m => m.AvailableShiftNames.Contains(st.Name))
                    })
                    .OrderBy(x => x.Count)
                    .Select(x => x.Time)
                    .ToList();
                // 勤務時間帯の割り当て順をログに出力
                SimpleLogger.Info(
                    $"[Generate] {date:yyyy-MM-dd} 勤務時間優先順: " +
                    string.Join(",", shiftTimeOrder.Select(t => t.Name)));

                // Phase-C: 残りの勤務枠を割り当て
                foreach (var st in shiftTimeOrder)
                {
                    int need = dayShiftReq.ContainsKey(st.Name) ? dayShiftReq[st.Name] : 0;
                    while (need > 0)
                    {
                        var target = candidates
                            .Where(m => !dayAssignments.ContainsKey(m.Id) && m.AvailableShiftNames.Contains(st.Name))
                            .OrderBy(m => CalculateCost(m, states[m.Id], date, st.Name, m.SkillGroup, shiftTimeMap))
                            .ThenBy(m => 柔軟度マップ.ContainsKey(m.Id) ? 柔軟度マップ[m.Id] : int.MaxValue)
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
                        states[target.Id].LastShiftName = st.Name;
                        try
                        {
                            if (shiftTimeMap.TryGetValue(st.Name, out var stInfo))
                            {
                                var cal = CultureInfo.CurrentCulture.Calendar;
                                var weekIdx = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                                if (!states[target.Id].WeeklyHours.ContainsKey(weekIdx))
                                {
                                    states[target.Id].WeeklyHours[weekIdx] = 0;
                                }
                                states[target.Id].WeeklyHours[weekIdx] += (stInfo.End - stInfo.Start).TotalHours;
                            }
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.Error("[Generate] 労働時間更新エラー", ex);
                        }
                        need--;
                    }
                }

                // Phase-D: (未実装)

                // 結果の書き込み
                foreach (var m in members)
                {
                    if (dayAssignments.TryGetValue(m.Id, out var shiftName))
                    {
                        bool preferWork = requests.Any(r => r.MemberId == m.Id && r.Date.Date == date.Date && r.種別 == 申請種別.勤務希望);
                        result[m.Id][date] = preferWork ? $"希{shiftName}" : shiftName;
                    }
                    else if (string.IsNullOrEmpty(result[m.Id][date]))
                    {
                        result[m.Id][date] = "休";
                        states[m.Id].WorkStreak = 0;
                        states[m.Id].LastShiftName = string.Empty;
                    }
                }
            }

            return result;
        }
    }
}
