using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace ShiftPlanner.Tests
{
    /// <summary>
    /// ShiftAnalyzer の主要メソッドをテストします。
    /// 仕様書の「月別の労働時間集計」「シフトタイプ分布の取得」に対応。
    /// </summary>
    public class ShiftAnalyzerTests
    {
        [Fact]
        public void CalculateMonthlyHours_ReturnsSumOfHoursForSpecifiedMonth()
        {
            var shifts = new List<ShiftPlanner.ShiftFrame>
            {
                new ShiftPlanner.ShiftFrame
                {
                    Date = new DateTime(2024, 5, 1),
                    ShiftStart = TimeSpan.FromHours(9),
                    ShiftEnd = TimeSpan.FromHours(17)
                },
                new ShiftPlanner.ShiftFrame
                {
                    Date = new DateTime(2024, 5, 2),
                    ShiftStart = TimeSpan.FromHours(8),
                    ShiftEnd = TimeSpan.FromHours(16)
                },
                // 別月のシフトは集計対象外
                new ShiftPlanner.ShiftFrame
                {
                    Date = new DateTime(2024, 4, 30),
                    ShiftStart = TimeSpan.FromHours(9),
                    ShiftEnd = TimeSpan.FromHours(17)
                }
            };

            var result = ShiftPlanner.ShiftAnalyzer.CalculateMonthlyHours(shifts, 2024, 5);

            result.Should().Be(TimeSpan.FromHours(16));
        }

        [Fact]
        public void GetShiftTypeDistribution_ReturnsCountsByType()
        {
            var shifts = new List<ShiftPlanner.ShiftFrame>
            {
                new ShiftPlanner.ShiftFrame { Date = new DateTime(2024, 5, 1), ShiftType = "Day" },
                new ShiftPlanner.ShiftFrame { Date = new DateTime(2024, 5, 2), ShiftType = "Day" },
                new ShiftPlanner.ShiftFrame { Date = new DateTime(2024, 5, 3), ShiftType = "Night" },
                // 別月は含めない
                new ShiftPlanner.ShiftFrame { Date = new DateTime(2024, 4, 30), ShiftType = "Night" }
            };

            var result = ShiftPlanner.ShiftAnalyzer.GetShiftTypeDistribution(shifts, 2024, 5);

            result.Should().ContainKey("Day").WhichValue.Should().Be(2);
            result.Should().ContainKey("Night").WhichValue.Should().Be(1);
            result.Should().NotContainKey("");
        }
    }
}
