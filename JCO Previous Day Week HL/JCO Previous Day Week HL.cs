// =====================================================
// Previous Day & Week High Low Indicator
// =====================================================
// Version: 2.3
// Date: 2026-01-20
//
// Changelog:
// v2.3 (2026-01-20)
//   - Added NY Midnight opening price line
//   - Added labels on all lines (PDH, PDM, PDL, PWH, PWL, 0 NY)
//   - Added configurable font size for labels per group
//
// v2.1 (2025-01-14)
//   - Added visibility toggles for day lines, week lines, and vertical lines
//   - Dashboard now always displays values independently of line visibility
//
// v2.0 (2025-01-14)
//   - Added Previous Week High & Low levels
//   - Reorganized parameters by section
//   - Removed mid line option for week levels
//
// v1.0 (Initial)
//   - Previous Day High/Low with mid line
//   - Configurable colors and display options
// =====================================================

using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System;
using System.Linq;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PreviousDayHighLow : Indicator
    {
        // ===== PREVIOUS DAY PARAMETERS =====
        [Parameter("Show Day Lines", DefaultValue = true, Group = "Previous Day")]
        public bool ShowDayLines { get; set; }

        [Parameter("Starting Hour", DefaultValue = 0, Group = "Previous Day")]
        public int PDHLStartingHour { get; set; }

        [Parameter("High Line Color", DefaultValue = "Green", Group = "Previous Day")]
        public string PDHLHighLineColor { get; set; }

        [Parameter("Mid Line Color", DefaultValue = "Gray", Group = "Previous Day")]
        public string PDHLMidLineColor { get; set; }

        [Parameter("Low Line Color", DefaultValue = "Green", Group = "Previous Day")]
        public string PDHLLowLineColor { get; set; }
        
        [Parameter("Show Mid Line", DefaultValue = true, Group = "Previous Day")]
        public bool PDHLShowMidLine { get; set; }       

        [Parameter("Line Thickness", DefaultValue = 2, Group = "Previous Day")]
        public int PDHLLineThickness { get; set; }

        [Parameter("Extend Lines (Candles)", DefaultValue = 10, Group = "Previous Day")]
        public int ExtendPDHLLines { get; set; }

        [Parameter("Vertical Line Color", DefaultValue = "Blue", Group = "Previous Day")]
        public string PDHLVerticalLineColor { get; set; }

        [Parameter("Label Font Size", DefaultValue = 9, Group = "Previous Day")]
        public int PDHLFontSize { get; set; }

        // ===== PREVIOUS WEEK PARAMETERS =====
        [Parameter("Show Week Lines", DefaultValue = true, Group = "Previous Week")]
        public bool ShowWeekLines { get; set; }

        [Parameter("High Line Color", DefaultValue = "Orange", Group = "Previous Week")]
        public string PWHLHighLineColor { get; set; }

        [Parameter("Low Line Color", DefaultValue = "Orange", Group = "Previous Week")]
        public string PWHLLowLineColor { get; set; }

        [Parameter("Line Thickness", DefaultValue = 2, Group = "Previous Week")]
        public int PWHLLineThickness { get; set; }

        [Parameter("Extend Lines (Candles)", DefaultValue = 10, Group = "Previous Week")]
        public int ExtendPWHLLines { get; set; }

        [Parameter("Vertical Line Color", DefaultValue = "DarkOrange", Group = "Previous Week")]
        public string PWHLVerticalLineColor { get; set; }

        [Parameter("Label Font Size", DefaultValue = 9, Group = "Previous Week")]
        public int PWHLFontSize { get; set; }

        // ===== NY MIDNIGHT PARAMETERS =====
        [Parameter("Show NY Midnight Line", DefaultValue = true, Group = "NY Midnight")]
        public bool ShowNYMidnight { get; set; }

        [Parameter("Line Color", DefaultValue = "DodgerBlue", Group = "NY Midnight")]
        public string NYMidnightLineColor { get; set; }

        [Parameter("Line Thickness", DefaultValue = 2, Group = "NY Midnight")]
        public int NYMidnightLineThickness { get; set; }

        [Parameter("Line Style", DefaultValue = LineStyle.DotsRare, Group = "NY Midnight")]
        public LineStyle NYMidnightLineStyle { get; set; }

        [Parameter("Extend Lines (Candles)", DefaultValue = 10, Group = "NY Midnight")]
        public int ExtendNYMidnightLines { get; set; }

        [Parameter("Label Font Size", DefaultValue = 9, Group = "NY Midnight")]
        public int NYMidnightFontSize { get; set; }

        // ===== DISPLAY OPTIONS =====
        [Parameter("Show Vertical Lines", DefaultValue = true, Group = "Display")]
        public bool ShowVerticalLines { get; set; }

        [Parameter("Display Dashboard", DefaultValue = true, Group = "Display")]
        public bool DisplayDashboard { get; set; }

        [Parameter("Enable Print", DefaultValue = false, Group = "Display")]
        public bool EnablePrint { get; set; }

        // Previous Day variables
        private DateTime PreviousDay;
        private double PreviousDayHigh;
        private double PreviousDayMid;
        private double PreviousDayLow;

        // Previous Week variables
        private DateTime PreviousWeekStart;
        private double PreviousWeekHigh;
        private double PreviousWeekLow;

        // NY Midnight variables
        private double NYMidnightOpenPrice;

        private Bars _hourlyBars;

        protected override void Initialize()
        {
            // Get the hourly timeframe bars
            _hourlyBars = MarketData.GetBars(TimeFrame.Hour);
        }

        public override void Calculate(int index)
        {
            if (index < 0) return;
            
            // Always calculate values (needed for dashboard even if lines are hidden)
            CalculateDayHighLow();
            CalculateWeekHighLow();
            CalculateNYMidnight();

            // Draw lines on the Chart (only if enabled)
            DrawLines();
            
            // Display the dashboard with High & Low prices
            if (DisplayDashboard)
                DisplayHighLowPrices();
        }

        private void CalculateDayHighLow()
        {
            // Ensure we have enough data
            if (_hourlyBars.Count < 48) return;

            // Remove existing day high & low lines only if we're showing them
            if (ShowDayLines)
            {
                foreach (var obj in Chart.Objects.Where(o => o.Name.StartsWith("DayHighLow")).ToList())
                {
                    Chart.RemoveObject(obj.Name);
                }
            }

            PreviousDayHigh = double.MinValue;
            PreviousDayMid = double.MinValue;
            PreviousDayLow = double.MaxValue;
            
            int gapPreviousDay = -1;
            
            if (Server.TimeInUtc.Hour < PDHLStartingHour)
            {
                gapPreviousDay -= 1;
            }
            
            if (Server.Time.DayOfWeek == DayOfWeek.Sunday)
            {
                gapPreviousDay -= 1;
            }
            if (Server.Time.DayOfWeek == DayOfWeek.Monday)
            {
                gapPreviousDay -= 2;
            }
            
            PreviousDay = Server.Time.Date.AddDays(gapPreviousDay);
            
            for (int i = 0; i < 24; i++)
            {
                var barIndex = GetCandleIndexAtHour((PDHLStartingHour + i), PreviousDay);
                
                if (PDHLStartingHour + i > 23)
                {
                    barIndex = GetCandleIndexAtHour((PDHLStartingHour + i - 23), PreviousDay.AddDays(1));
                }
                
                if (barIndex >= 0)
                {
                    PreviousDayHigh = Math.Max(PreviousDayHigh, _hourlyBars.HighPrices[barIndex]);
                    PreviousDayLow = Math.Min(PreviousDayLow, _hourlyBars.LowPrices[barIndex]);
                    PreviousDayMid = PreviousDayLow + ((PreviousDayHigh - PreviousDayLow) / 2);
                }
            }

            if (EnablePrint)
            {
                Print("Previous Day High: {0}, Mid: {1}, Low: {2}", PreviousDayHigh, PreviousDayMid, PreviousDayLow);
            }
        }

        private void CalculateWeekHighLow()
        {
            // Ensure we have enough data
            if (_hourlyBars.Count < 168) return; // 7 days * 24 hours

            // Remove existing week high & low lines only if we're showing them
            if (ShowWeekLines)
            {
                foreach (var obj in Chart.Objects.Where(o => o.Name.StartsWith("WeekHighLow")).ToList())
                {
                    Chart.RemoveObject(obj.Name);
                }
            }

            PreviousWeekHigh = double.MinValue;
            PreviousWeekLow = double.MaxValue;

            // Calculate the start of the previous week (Monday)
            int daysToSubtract = 7 + (int)Server.Time.DayOfWeek;
            if (Server.Time.DayOfWeek == DayOfWeek.Sunday)
                daysToSubtract = 7;
            else
                daysToSubtract = 7 + (int)Server.Time.DayOfWeek - 1;

            PreviousWeekStart = Server.Time.Date.AddDays(-daysToSubtract);

            // Loop through 5 trading days (Monday to Friday)
            for (int day = 0; day < 5; day++)
            {
                DateTime currentDay = PreviousWeekStart.AddDays(day);
                
                for (int hour = 0; hour < 24; hour++)
                {
                    var barIndex = GetCandleIndexAtHour(hour, currentDay);
                    
                    if (barIndex >= 0)
                    {
                        PreviousWeekHigh = Math.Max(PreviousWeekHigh, _hourlyBars.HighPrices[barIndex]);
                        PreviousWeekLow = Math.Min(PreviousWeekLow, _hourlyBars.LowPrices[barIndex]);
                    }
                }
            }

            if (EnablePrint)
            {
                Print("Previous Week High: {0}, Low: {1}", PreviousWeekHigh, PreviousWeekLow);
            }
        }

        private void CalculateNYMidnight()
        {
            // Remove existing NY Midnight lines
            if (ShowNYMidnight)
            {
                foreach (var obj in Chart.Objects.Where(o => o.Name.StartsWith("NYMidnight")).ToList())
                {
                    Chart.RemoveObject(obj.Name);
                }
            }

            NYMidnightOpenPrice = double.MinValue;

            DateTime today = Server.Time.Date;
            DateTime midnightNY = GetNewYorkMidnight(today);

            // Find the bar at NY midnight
            int midnightIndex = -1;
            for (int i = Bars.Count - 1; i >= 0; i--)
            {
                if (Bars.OpenTimes[i] <= midnightNY)
                {
                    midnightIndex = i;
                    break;
                }
            }

            if (midnightIndex >= 0)
            {
                NYMidnightOpenPrice = Bars.OpenPrices[midnightIndex];
            }

            if (EnablePrint)
            {
                Print("NY Midnight Open Price: {0}", NYMidnightOpenPrice);
            }
        }

        private DateTime GetNewYorkMidnight(DateTime date)
        {
            TimeZoneInfo nyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime midnightNY = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(midnightNY, nyTimeZone);
        }

        private DateTime GetLabelPosition(DateTime endTime, int barsFromEnd)
        {
            // Find the last bar before endTime and go back barsFromEnd bars
            for (int i = Bars.Count - 1; i >= barsFromEnd; i--)
            {
                if (Bars.OpenTimes[i] <= endTime)
                {
                    return Bars.OpenTimes[i - barsFromEnd];
                }
            }
            // Fallback: return endTime minus some minutes
            return endTime.AddMinutes(-GetMinutesPerCandle() * barsFromEnd);
        }

        private void DrawLines()
        {
            // ===== PREVIOUS DAY LINES =====
            if (ShowDayLines)
            {
                var dayStartTime = _hourlyBars.OpenTimes[GetCandleIndexAtHour(PDHLStartingHour, PreviousDay)];
                var dayEndTime = Server.TimeInUtc.AddMinutes(GetMinutesPerCandle() * ExtendPDHLLines);
                var dayLabelTime = GetLabelPosition(dayEndTime, 5);

                if (PreviousDayHigh > double.MinValue && PreviousDayLow < double.MaxValue)
                {
                    // High Line
                    var highLine = Chart.DrawTrendLine("DayHighLow HighLine", dayStartTime, PreviousDayHigh, dayEndTime, PreviousDayHigh, Color.FromName(PDHLHighLineColor), PDHLLineThickness, LineStyle.Solid);
                    highLine.IsInteractive = false;
                    var highLabel = Chart.DrawText("DayHighLow HighLabel", "PDH", dayLabelTime, PreviousDayHigh, Color.FromName(PDHLHighLineColor));
                    highLabel.IsInteractive = false;
                    highLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    highLabel.VerticalAlignment = VerticalAlignment.Center;
                    highLabel.FontSize = PDHLFontSize;

                    // Low Line
                    var lowLine = Chart.DrawTrendLine("DayHighLow LowLine", dayStartTime, PreviousDayLow, dayEndTime, PreviousDayLow, Color.FromName(PDHLLowLineColor), PDHLLineThickness, LineStyle.Solid);
                    lowLine.IsInteractive = false;
                    var lowLabel = Chart.DrawText("DayHighLow LowLabel", "PDL", dayLabelTime, PreviousDayLow, Color.FromName(PDHLLowLineColor));
                    lowLabel.IsInteractive = false;
                    lowLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    lowLabel.VerticalAlignment = VerticalAlignment.Center;
                    lowLabel.FontSize = PDHLFontSize;

                    // Mid Line
                    if (PDHLShowMidLine)
                    {
                        var midLine = Chart.DrawTrendLine("DayHighLow MidLine", dayStartTime, PreviousDayMid, dayEndTime, PreviousDayMid, Color.FromName(PDHLMidLineColor), PDHLLineThickness, LineStyle.DotsRare);
                        midLine.IsInteractive = false;
                        var midLabel = Chart.DrawText("DayHighLow MidLabel", "PDM", dayLabelTime, PreviousDayMid, Color.FromName(PDHLMidLineColor));
                        midLabel.IsInteractive = false;
                        midLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        midLabel.VerticalAlignment = VerticalAlignment.Center;
                        midLabel.FontSize = PDHLFontSize;
                    }
                }

                if (ShowVerticalLines)
                {
                    Chart.DrawVerticalLine("DayHighLow StartLine", dayStartTime, Color.FromName(PDHLVerticalLineColor), 1, LineStyle.Dots);
                    Chart.DrawVerticalLine("DayHighLow EndLine", dayStartTime.AddHours(23), Color.FromName(PDHLVerticalLineColor), 1, LineStyle.Dots);
                }
            }

            // ===== PREVIOUS WEEK LINES =====
            if (ShowWeekLines)
            {
                var weekStartTime = _hourlyBars.OpenTimes[GetCandleIndexAtHour(0, PreviousWeekStart)];
                var weekEndTime = Server.TimeInUtc.AddMinutes(GetMinutesPerCandle() * ExtendPWHLLines);
                var weekLabelTime = GetLabelPosition(weekEndTime, 5);

                if (PreviousWeekHigh > double.MinValue && PreviousWeekLow < double.MaxValue)
                {
                    // High Line
                    var highLine = Chart.DrawTrendLine("WeekHighLow HighLine", weekStartTime, PreviousWeekHigh, weekEndTime, PreviousWeekHigh, Color.FromName(PWHLHighLineColor), PWHLLineThickness, LineStyle.Solid);
                    highLine.IsInteractive = false;
                    var highLabel = Chart.DrawText("WeekHighLow HighLabel", "PWH", weekLabelTime, PreviousWeekHigh, Color.FromName(PWHLHighLineColor));
                    highLabel.IsInteractive = false;
                    highLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    highLabel.VerticalAlignment = VerticalAlignment.Center;
                    highLabel.FontSize = PWHLFontSize;

                    // Low Line
                    var lowLine = Chart.DrawTrendLine("WeekHighLow LowLine", weekStartTime, PreviousWeekLow, weekEndTime, PreviousWeekLow, Color.FromName(PWHLLowLineColor), PWHLLineThickness, LineStyle.Solid);
                    lowLine.IsInteractive = false;
                    var lowLabel = Chart.DrawText("WeekHighLow LowLabel", "PWL", weekLabelTime, PreviousWeekLow, Color.FromName(PWHLLowLineColor));
                    lowLabel.IsInteractive = false;
                    lowLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    lowLabel.VerticalAlignment = VerticalAlignment.Center;
                    lowLabel.FontSize = PWHLFontSize;
                }

                if (ShowVerticalLines)
                {
                    Chart.DrawVerticalLine("WeekHighLow StartLine", weekStartTime, Color.FromName(PWHLVerticalLineColor), 1, LineStyle.Dots);
                    Chart.DrawVerticalLine("WeekHighLow EndLine", weekStartTime.AddDays(5), Color.FromName(PWHLVerticalLineColor), 1, LineStyle.Dots);
                }
            }

            // ===== NY MIDNIGHT LINE =====
            if (ShowNYMidnight && NYMidnightOpenPrice > double.MinValue)
            {
                DateTime midnightNY = GetNewYorkMidnight(Server.Time.Date);
                var nyEndTime = Server.TimeInUtc.AddMinutes(GetMinutesPerCandle() * ExtendNYMidnightLines);

                // Position the label 5 bars before the end
                DateTime labelTime = GetLabelPosition(nyEndTime, 5);

                var line = Chart.DrawTrendLine("NYMidnight Line", midnightNY, NYMidnightOpenPrice, nyEndTime, NYMidnightOpenPrice, Color.FromName(NYMidnightLineColor), NYMidnightLineThickness, NYMidnightLineStyle);
                line.IsInteractive = false;

                var text = Chart.DrawText("NYMidnight Label", "0 NY", labelTime, NYMidnightOpenPrice, Color.FromName(NYMidnightLineColor));
                text.IsInteractive = false;
                text.HorizontalAlignment = HorizontalAlignment.Left;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.FontSize = NYMidnightFontSize;
            }
        }

        private void DisplayHighLowPrices()
        {
            // Remove existing text objects
            foreach (var obj in Chart.Objects.Where(o => o.Name.StartsWith("PrevText")).ToList())
            {
                Chart.RemoveObject(obj.Name);
            }
            
            // Always display all values (independent of line visibility)
            string rangeText = "";
            string pricesText = "\n\n";
            
            if (PreviousDayHigh > double.MinValue && PreviousDayLow < double.MaxValue)
            {
                rangeText += string.Format("Day Range: {0:F1} pips\n", (PreviousDayHigh - PreviousDayLow) / Symbol.PipSize);
                
                string dayHighText = PreviousDayHigh.ToString("F" + Symbol.Digits);
                string dayMidText = PreviousDayMid.ToString("F" + Symbol.Digits);
                string dayLowText = PreviousDayLow.ToString("F" + Symbol.Digits);
                
                pricesText += string.Format("Prev Day H: {0}\nPrev Day M: {1}\nPrev Day L: {2}\n\n",
                    dayHighText, dayMidText, dayLowText);
            }
            
            if (PreviousWeekHigh > double.MinValue && PreviousWeekLow < double.MaxValue)
            {
                rangeText += string.Format("Week Range: {0:F1} pips\n", (PreviousWeekHigh - PreviousWeekLow) / Symbol.PipSize);
                
                string weekHighText = PreviousWeekHigh.ToString("F" + Symbol.Digits);
                string weekLowText = PreviousWeekLow.ToString("F" + Symbol.Digits);
                
                pricesText += string.Format("Prev Week H: {0}\nPrev Week L: {1}\n",
                    weekHighText, weekLowText);
            }
            
            rangeText += "\n\n\n\n\n\n\n\n";
            pricesText += " ";
            
            Chart.DrawStaticText("PrevText_Range", rangeText, VerticalAlignment.Bottom, HorizontalAlignment.Right, Color.LightBlue);
            Chart.DrawStaticText("PrevText_Prices", pricesText, VerticalAlignment.Bottom, HorizontalAlignment.Right, Color.SlateGray);
        }
        
        private int GetCandleIndexAtHour(int hour, DateTime date)
        {
            for (int i = _hourlyBars.Count - 1; i >= 0; i--)
            {
                var barTimeUtc = _hourlyBars.OpenTimes[i].ToUniversalTime();
                if (barTimeUtc.Hour == hour && barTimeUtc.Date == date)
                {
                    return i;
                }
            }
            return -1;
        }

        private int GetMinutesPerCandle()
        {
            if (Bars.TimeFrame == TimeFrame.Minute) return 1;
            if (Bars.TimeFrame == TimeFrame.Minute5) return 5;
            if (Bars.TimeFrame == TimeFrame.Minute15) return 15;
            if (Bars.TimeFrame == TimeFrame.Minute30) return 30;
            if (Bars.TimeFrame == TimeFrame.Hour) return 60;
            if (Bars.TimeFrame == TimeFrame.Hour4) return 240;
            if (Bars.TimeFrame == TimeFrame.Daily) return 1440;
            if (Bars.TimeFrame == TimeFrame.Weekly) return 10080;

            return 1;
        }

        protected override void OnDestroy()
        {
            // Clean up all chart objects created by this indicator
            foreach (var obj in Chart.Objects.Where(o =>
                o.Name.StartsWith("DayHighLow") ||
                o.Name.StartsWith("WeekHighLow") ||
                o.Name.StartsWith("NYMidnight") ||
                o.Name.StartsWith("PrevText")).ToList())
            {
                Chart.RemoveObject(obj.Name);
            }
        }
    }
}