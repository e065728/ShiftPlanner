using System;

namespace ShiftPlanner
{
    /// <summary>
    /// 各従業員に適用される勤務条件を表します。
    /// </summary>
    public class ShiftConstraints
    {
        /// <summary>
        /// 1週間あたりの最小勤務時間(時間単位)。
        /// </summary>
        public double MinWeeklyHours { get; set; }

        /// <summary>
        /// 1週間あたりの最大勤務時間(時間単位)。
        /// </summary>
        public double MaxWeeklyHours { get; set; }

        /// <summary>
        /// 連続勤務可能日数の上限。
        /// </summary>
        public int MaxConsecutiveDays { get; set; }
    }
}
