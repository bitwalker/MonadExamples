using System;

namespace Monads.IndexedState
{
    // We could use Tuple here, but Tuple is invariant, and we require covariance for this
    public interface IPair<out TLeft, out TRight>
    {
        TLeft  Left  { get; }
        TRight Right { get; }
    }

    public class Pair<TLeft, TRight> : IPair<TLeft, TRight>
    {
        public TLeft Left  { get; private set; }
        public TRight Right { get; private set; }

        private Pair(TLeft left, TRight right)
        {
            Left  = left;
            Right = right;
        }

        public static IPair<TLeft, TRight> Create(TLeft left, TRight right) {
            return new Pair<TLeft, TRight>(left, right);
        }
    }
}