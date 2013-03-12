using System;

namespace Monads.IndexedState
{
    public interface IPair<out TLeft, out TRight>
    {
        TLeft  Left  { get; }
        TRight Right { get; }
    }

    public class Pair<TLeft, TRight> : IPair<TLeft, TRight>
    {
        public TLeft Left  { get; private set; }
        public TLeft Right { get; private set; }

        private Pair(TLeft left, TRight right)
        {
            Left  = left;
            Right = right;
        }
    }

    public static class Pair
    {
        public static IPair<TLeft, TRight> Create<TLeft, TRight>(TLeft left, TRight right) {
            return new Pair<TLeft, TRight>(left, right);
        }
    }
}