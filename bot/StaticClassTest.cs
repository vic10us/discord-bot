using System;

namespace bot;

public static class StaticClassTest
{
    // Static variable that must be initialized at run time.
    static readonly long baseline;

    // Static constructor is called at most one time, before any
    // instance constructor is invoked or member is accessed.
    static StaticClassTest()
    {
        baseline = DateTime.Now.Ticks;
        Console.WriteLine("Static constructor called at {0}.", baseline);
    }
}
