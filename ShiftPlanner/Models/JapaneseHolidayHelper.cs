using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftPlanner
{
    /// <summary>
    /// 日本の祝日判定を行う補助クラス
    /// </summary>
    public static class JapaneseHolidayHelper
    {
        // 追加祝日リスト
        private static List<CustomHoliday> customHolidays = new List<CustomHoliday>();

        /// <summary>
        /// 追加祝日リストを設定します。
        /// </summary>
        public static void SetCustomHolidays(List<CustomHoliday>? holidays)
        {
            customHolidays = holidays ?? new List<CustomHoliday>();
        }

        /// <summary>
        /// 指定した日付が祝日かどうか判定します。
        /// </summary>
        /// <param name="date">判定する日付</param>
        /// <returns>祝日の場合 true</returns>
        public static bool IsHoliday(DateTime date)
        {
            // 日付部分のみで比較
            date = date.Date;

            // ユーザー定義の祝日を先に判定
            if (customHolidays.Any(h => h.Date.Date == date))
            {
                return true;
            }

            if (IsFixedHoliday(date))
            {
                return true;
            }

            if (IsHappyMondayHoliday(date))
            {
                return true;
            }

            if (IsEquinoxHoliday(date))
            {
                return true;
            }

            // 振替休日 (祝日が日曜日の場合、翌月曜日が休み)
            if (date.DayOfWeek == DayOfWeek.Monday)
            {
                var prev = date.AddDays(-1);
                if ((IsFixedHoliday(prev) || IsHappyMondayHoliday(prev) || IsEquinoxHoliday(prev))
                    && prev.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }
            }

            // 国民の休日 (祝日に挟まれた平日)
            var before = date.AddDays(-1);
            var after = date.AddDays(1);
            if (!IsFixedHoliday(date) && !IsHappyMondayHoliday(date) && !IsEquinoxHoliday(date))
            {
                if ((IsFixedHoliday(before) || IsHappyMondayHoliday(before) || IsEquinoxHoliday(before))
                    && (IsFixedHoliday(after) || IsHappyMondayHoliday(after) || IsEquinoxHoliday(after)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 毎年同じ日付の固定祝日か判定します。
        /// </summary>
        private static bool IsFixedHoliday(DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                    if (date.Day == 1) return true; // 元日
                    break;
                case 2:
                    if (date.Day == 11) return true; // 建国記念の日
                    if (date.Day == 23 && date.Year >= 2020) return true; // 天皇誕生日
                    break;
                case 4:
                    if (date.Day == 29) return true; // 昭和の日
                    break;
                case 5:
                    if (date.Day == 3 || date.Day == 4 || date.Day == 5) return true; // 憲法記念日、みどりの日、こどもの日
                    break;
                case 8:
                    if (date.Day == 11) return true; // 山の日
                    break;
                case 11:
                    if (date.Day == 3) return true; // 文化の日
                    if (date.Day == 23) return true; // 勤労感謝の日
                    break;
            }
            return false;
        }

        /// <summary>
        /// ハッピーマンデー制の祝日か判定します。
        /// </summary>
        private static bool IsHappyMondayHoliday(DateTime date)
        {
            if (date.DayOfWeek != DayOfWeek.Monday)
            {
                return false;
            }

            int week = (date.Day - 1) / 7 + 1; // 月内での週番号
            switch (date.Month)
            {
                case 1:
                    if (week == 2) return true; // 成人の日
                    break;
                case 7:
                    if (week == 3) return true; // 海の日
                    break;
                case 9:
                    if (week == 3) return true; // 敬老の日
                    break;
                case 10:
                    if (week == 2) return true; // スポーツの日（旧 体育の日）
                    break;
            }
            return false;
        }

        /// <summary>
        /// 春分の日・秋分の日か判定します。
        /// </summary>
        private static bool IsEquinoxHoliday(DateTime date)
        {
            int year = date.Year;

            int springDay = (int)Math.Floor(20.8431 + 0.242194 * (year - 2000) - Math.Floor((year - 2000) / 4.0));
            int autumnDay = (int)Math.Floor(23.2488 + 0.242194 * (year - 2000) - Math.Floor((year - 2000) / 4.0));

            if (date.Month == 3 && date.Day == springDay) return true;
            if (date.Month == 9 && date.Day == autumnDay) return true;

            return false;
        }
    }
}
