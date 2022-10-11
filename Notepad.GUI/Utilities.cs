using System;
using System.Linq;

namespace Notepad.GUI
{
    /// <summary>
    /// A collection of utility functions that just make life easier
    /// </summary>
    public static class Utilities
    {
        public static int FindPattern<T>(T[] source, T[] pattern)
        {
            if (source.Length >= pattern.Length)
            {
                for (int i = 0; i + pattern.Length <= source.Length; i++)
                {
                    var slice = source[i..(i + pattern.Length)];
                    if (slice.SequenceEqual(pattern))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
