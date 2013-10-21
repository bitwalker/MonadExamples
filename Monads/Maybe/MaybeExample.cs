using System;
using System.Collections.Generic;

namespace Monads.Maybe
{
    public class MaybeExample
    {
        public static void Main(string[] args)
        {
            // The Maybe monad allows you to write code safely against a value, including child properties
            // of that value, and get either the result of the transformations of that value, or a sane default
            MaybeTest test = null;
            var joined = test.AsMaybe()
                             .Select(x => x.Values)
                             .Select(values => string.Join(", ", values))
                             .GetValueOrDefault(string.Empty);
            Console.WriteLine(joined);
        }

        public class MaybeTest
        {
            public int Id { get; set; }
            public List<string> Values { get; set; }
        }
    }
}
