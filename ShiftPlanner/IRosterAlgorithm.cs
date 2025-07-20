namespace ShiftPlanner
{
    /// <summary>
    /// シフト自動生成アルゴリズムを表すインターフェース
    /// </summary>
    public interface IRosterAlgorithm
    {
        /// <summary>
        /// 指定された条件からシフト割り当てを生成します。
        /// </summary>
        /// <param name="members">メンバー一覧</param>
        /// <param name="baseDate">対象月の開始日</param>
        /// <param name="days">日数</param>
        /// <param name="skillRequirements">スキルグループ必要人数</param>
        /// <param name="shiftRequirements">勤務時間帯必要人数</param>
        /// <param name="requests">シフト希望</param>
        /// <param name="skillGroups">スキルグループ一覧</param>
        /// <param name="shiftTimes">勤務時間一覧</param>
        /// <param name="minHolidayCount">最低休日日数</param>
        /// <returns>メンバーIDと日付をキーにした割当結果</returns>
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<System.DateTime, string>> Generate(
            System.Collections.Generic.List<Member>? members,
            System.DateTime baseDate,
            int days,
            System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<string, int>>? skillRequirements,
            System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<string, int>>? shiftRequirements,
            System.Collections.Generic.List<ShiftRequest>? requests,
            System.Collections.Generic.List<SkillGroup>? skillGroups,
            System.Collections.Generic.List<ShiftTime>? shiftTimes,
            int minHolidayCount);
    }
}
