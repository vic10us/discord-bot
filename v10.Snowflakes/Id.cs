﻿using System.Diagnostics;
using v10.Snowflakes.Extensions;

namespace v10.Snowflakes;


/// <summary>
///     Represents a unique, K-ordered, sortable identifier.
/// </summary>
[DebuggerDisplay("{_value}")]
public struct Id(long value) : IComparable<Id>
{
    // This implementation of Snowflake ID is based on the specification as published by Discord:
    // https://discord.com/developers/docs/reference
    //
    // Every Snowflake fits in a 64-bit integer, consisting of various components that make it unique across generations.
    // The layout of the components that comprise a snowflake can be expressed as:
    //
    // Timestamp                                   Thread Proc  Increment
    // 111111111111111111111111111111111111111111  11111  11111 111111111111
    // 64                                          22     17    12          0
    //
    // The Timestamp component is represented as the milliseconds since the first second of 2015. 
    // Since we're using all 64 bits available, this epoch can be any point in time, as long as it's in the past.
    // If the epoch is set to a point in time in the future, it may result in negative snowflakes being generated.
    //
    // Where the original Discord reference mentions worker ID and process ID, we substitute these with the
    // thread and process ID respectively, as the combination of these two provide sufficient uniqueness, and they are
    // the closest we can get to the original specification within the .NET ecosystem.
    //
    // The Increment component is a monotonically incrementing number, which is incremented every time a snowflake is generated.
    // This is in contrast with some other flake-ish implementations, which only increment the counter any time a snowflake is 
    // generated twice at the exact same instant in time. We believe Discord's implementation is more correct here,
    // as even two snowflakes that are generated at the exact same point in time will not be identical, because of their increments.
    //
    // This implementation is optimised for high-throughput applications, while providing IDs that are roughly sortable, and 
    // with a very high degree of uniqueness.

    private long _value = value;

    private static int s_increment;

    // Calling Process.GetCurrentProcess() is a very slow operation, as it has to query the operating system.
    // Because it's highly unlikely the process ID will change (if at all possible) during our run time, we'll cache it.
    private static int? s_processId;

    private const int TimestampBits = 42;
    private const int ThreadIdBits = 5;
    private const int ProcessIdBits = 5;
    private const int IncrementBits = 12;

    private const long TimestampMask = (1L << TimestampBits) - 1;
    private const int ThreadIdMask = (1 << ThreadIdBits) - 1;
    private const int ProcessIdMask = (1 << ProcessIdBits) - 1;
    private const int IncrementMask = (1 << IncrementBits) - 1;

    /// <summary>
    ///     Creates a new, unique ID.
    /// </summary>
    /// <returns></returns>
    public static Id Create()
    {
        Id id = new Id();

        id.CreateInternal();

        return id;
    }

    /// <summary>
    ///     Attempts to parse an ID from the specified <see cref="long" /> value. This method will return false if the
    ///     specified value doesn't match the shape of a snowflake ID.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool TryParse(long value, out Id id)
    {
        Id input = new Id(value);

        if (!input.IsSnowflake())
        {
            id = default;
            return false;
        }

        id = input;
        return true;
    }

    /// <summary>
    ///     Attempts to parse an ID from the specified <see cref="long" /> value. This method will return false if the
    ///     specified value doesn't match the shape of a snowflake ID.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool TryParse(string strValue, out Id id)
    {
        try
        {
            var input = Parse(strValue);

            if (!input.IsSnowflake())
            {
                id = default;
                return false;
            }

            id = input;
            return true;
        }
        catch (Exception)
        {
            id = default;
            return false;
        }
    }

    /// <summary>
    ///     Parses an ID from the specified <see cref="long" /> value, and throws an exception if the shape of the value
    ///     doesn't match that of a valid ID.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Id Parse(long value)
    {
        Id id = new Id(value);

        if (!id.IsSnowflake())
        {
            throw new FormatException("The specified value is not a valid snowflake");
        }

        return id;
    }

    /// <summary>
    ///     Parses an ID from the specified <see cref="string" /> value, and throws an exception if the shape of the value
    ///     doesn't match that of a valid ID.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="OverflowException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static Id Parse(string strValue)
    {
        var value = long.Parse(strValue);
        Id id = Parse(value);
        return id;
    }

    private void CreateInternal()
    {
        long milliseconds = MonotonicTimer.ElapsedMilliseconds;
        long timestamp = milliseconds & TimestampMask;
        int threadId = Environment.CurrentManagedThreadId & ThreadIdMask;
        int processId = s_processId ??= Environment.ProcessId & ProcessIdMask;

        Interlocked.Increment(ref s_increment);

        int increment = s_increment & IncrementMask;

        unchecked
        {
            _value = (timestamp << (ThreadIdBits + ProcessIdBits + IncrementBits))
                     + (threadId << (ProcessIdBits + IncrementBits))
                     + (processId << IncrementBits)
                     + increment;
        }
    }

    public override readonly string ToString() => _value.ToString();

    public static implicit operator long(Id id) => id._value;
    public static explicit operator Id(long value) => new(value);

    public static implicit operator string(Id id) => id.ToString();
    public static explicit operator Id(string value) => Parse(value);

    public static bool operator ==(Id left, Id right) => left._value == right._value;

    public static bool operator !=(Id left, Id right) => !(left == right);

    public readonly int CompareTo(Id other) => _value.CompareTo(other._value);

    public readonly bool Equals(Id other) => _value == other._value;

    public override readonly bool Equals(object? obj) => obj is Id other && Equals(other);

    public override readonly int GetHashCode() => _value.GetHashCode();
}
