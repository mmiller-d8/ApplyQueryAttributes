using System;
namespace D8.Core.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime source) =>
        CalculateAge(DateTime.Now, source);

    public static int CalculateAgeUtc(this DateTime source) =>
        CalculateAge(DateTime.UtcNow, source);



    private static int CalculateAge(DateTime referenceDate, DateTime BirthDate)
    {
        int YearsPassed = referenceDate.Year - BirthDate.Year;
        // Are we before the birth date this year? If so subtract one year from the mix
        if (DateTime.Now.Month < BirthDate.Month || (DateTime.Now.Month == BirthDate.Month && DateTime.Now.Day < BirthDate.Day))
        {
            YearsPassed--;
        }
        return YearsPassed;
    }
}

