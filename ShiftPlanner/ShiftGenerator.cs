using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ShiftPlanner
{
    public static class ShiftGenerator
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
                Console.WriteLine($"[Shuffle] シャフル中にエラーが発生しました: {ex.Message}");
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
                Console.WriteLine($"[GenerateBaseShift] 出力中にエラー: {ex.Message}");
            }

            return assignments;
        }
    }
}

