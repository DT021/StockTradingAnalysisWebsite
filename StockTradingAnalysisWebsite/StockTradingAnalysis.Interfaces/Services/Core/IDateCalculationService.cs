using System;

namespace StockTradingAnalysis.Interfaces.Services
{
    public interface IDateCalculationService
    {
        /// <summary>
        /// Returns the start date of the week calculated with the given date
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <returns>Start Date of Week</returns>
        DateTime GetStartAndEndDateOfWeek(DateTime date, out DateTime end);

        /// <summary>
        /// Returns the end date of the month calculated with the given date
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <returns>Start Date of Month</returns>
        DateTime GetStartAndEndDateOfMonth(DateTime date, out DateTime end);

        /// <summary>
        /// Returns the end date of the month calculated with the given date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>End Date of Month</returns>
        DateTime GetEndDateOfMonth(DateTime date);

        /// <summary>
        /// Returns the end date of the week calculated with the given date
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <returns>End Date of two weeks</returns>
        DateTime GetStartAndEndDateOf2Weeks(DateTime date, out DateTime end);

        /// <summary>
        /// Returns the end date of the the year
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <returns>Start Date of year</returns>
        DateTime GetStartAndEndDateOfYear(DateTime date, out DateTime end);

        /// <summary>
        /// Returns the end date of the the year
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>End Date of year</returns>
        DateTime GetEndDateOfYear(DateTime date);

        /// <summary>
        /// Returns the start date of the the year
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Start Date of year</returns>
        DateTime GetStartDateOfYear(DateTime date);

        /// <summary>
        /// Returns the end date of the the quarter
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <returns>End Date of quarter</returns>
        DateTime GetStartAndEndDateOfQuarter(DateTime date, out DateTime end);

        /// <summary>
        /// Returns the end date of the the quarter
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="end">End date</param>
        /// <param name="quarter">Quarter</param>
        /// <returns>End Date of quarter</returns>
        DateTime GetStartAndEndDateOfQuarter(DateTime date, out DateTime end, out int quarter);

        /// <summary>
        /// Returns the calendar week of the given date
        /// </summary>
        /// <param name="Datum"></param>
        /// <returns></returns>
        int GetCalendarWeek(DateTime Datum);

        /// <summary>
        /// Returns the start date of the given calendar week and the end date
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekOfYear"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        DateTime FirstDateOfWeek(int year, int weekOfYear, out DateTime end);

        /// <summary>
        /// Returns the end of today
        /// </summary>
        /// <returns>End of Day</returns>
        DateTime GetEndOfToDay();
    }
}