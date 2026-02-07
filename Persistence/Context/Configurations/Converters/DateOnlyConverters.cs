using System;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Context.Configurations.Converters;

public class DateOnlyConverters
{
    public static readonly ValueConverter<DateOnly, string> DateOnlyToStringConverter = 
        new(d => d.ToString("yyyy-MM-dd"),
            s => DateOnly.Parse(s));

    public static readonly ValueConverter<DateOnly?, string?> NullableDateonlyToStringConverter = 
        new(d => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null,
            s => string.IsNullOrWhiteSpace(s) ? null : DateOnly.Parse(s));

    public static readonly ValueComparer<DateOnly> DateOnlyComparer = 
        new((x, y) => x.DayNumber == y.DayNumber,
            d => d.GetHashCode(),
            d => DateOnly.FromDayNumber(d.DayNumber));

    public static readonly ValueComparer<DateOnly?> NullableDateOnlyConverter =
        new((x, y) => x.GetValueOrDefault().DayNumber == y.GetValueOrDefault().DayNumber,
            d => d.HasValue ? d.Value.GetHashCode() : 0,
            d => d.HasValue ? DateOnly.FromDayNumber(d.Value.DayNumber) : null);

}
