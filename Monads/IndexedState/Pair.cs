using System;

namespace Monads.IndexedState
{
    /// <summary>
    /// Defines a pair of items, similar to Tuple, but is covariant rather than invariant.
    /// In addition, this interface represents a contract for an immutable value (no setters),
    /// but ultimately it's up to the implementation to uphold that contract.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left side item</typeparam>
    /// <typeparam name="TRight">The type of the right side item</typeparam>
    public interface IPair<out TLeft, out TRight>
    {
        TLeft  Left  { get; }
        TRight Right { get; }
    }

    /// <summary>
    /// Immutable immplementation of the IPair interface.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left side item</typeparam>
    /// <typeparam name="TRight">The type of the right side item.</typeparam>
    public class Pair<TLeft, TRight> : IPair<TLeft, TRight>
    {
        public TLeft Left  { get; private set; }
        public TRight Right { get; private set; }

        public Pair(TLeft left, TRight right)
        {
            Left  = left;
            Right = right;
        }
    }
}