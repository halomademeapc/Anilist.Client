using Anilist.Client.GraphQl;
using System;

public static class Extensions
{
    public static DateTime? ToDate(this FuzzyDate date) => date?.Year != null
         ? null
         : (DateTime?)new DateTime(date.Year.Value, date.Month ?? 1, date.Day ?? 1, 0, 0, 0, DateTimeKind.Unspecified);
}