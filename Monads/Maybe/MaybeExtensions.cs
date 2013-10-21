namespace Monads.Maybe
{
    public static class MaybeExtensions
    {
        public static Maybe<T> AsMaybe<T>(this T @this)
        {
            if (@this == null)
                return Maybe<T>.Empty;
            else
                return new Maybe<T>(@this);
        }
    }
}
