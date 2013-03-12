using System;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    public static class State
    {
        // The Indexed State monad is implemented as a function from initial state to a tuple of the result
        // and the final state.

        public static IPair<A, O> Run<I, O, A>(this Func<I, IPair<A, O>> state, I input)
        {
            return state(input);
        }

        public static A Eval<I, O, A>(this Func<I, IPair<A, O>> state, I input)
        {
            return state(input).Left;
        }

        public static O Exec<I, O, A>(this Func<I, IPair<A, O>> state, I input)
        {
            return state(input).Right;
        }

        // The following methods implement the necessary methods to allows us the syntactic sugar
        // of LINQ query expressions. This is what gives us the monadic behavior.

        // unit
        public static Func<I, IPair<A, I>> ToState<I, A>(this A a)
        {
            return (i => Pair<A, I>.Create(a, i));
        }

        // map
        public static Func<I, IPair<B, O>> Select<I, O, A, B>(this Func<I, IPair<A, O>> state, Func<A, B> func)
        {
            return (i =>
                {
                    var ao = state.Run(i);
                    return Pair<B, O>.Create(func(ao.Left), ao.Right);
                });
        }

        // join
        public static Func<I, IPair<A, O>> Flatten<I, M, O, A>(this Func<I, IPair<Func<M, IPair<A, O>>, M>>  state)
        {
            return (i =>
                {
                    var qm = state.Run(i);
                    return qm.Left.Run(qm.Right);
                });
        }

        // bind
        public static Func<I, IPair<B, O>> SelectMany<I, M, O, A, B>(this Func<I, IPair<A, M>> state,
                                                                     Func<A, Func<M, IPair<B, O>>>  func)
        {
            return (i =>
                {
                    var am = state.Run(i);
                    return func(am.Left).Run(am.Right);
                });
        }

        // bindMap
        public static Func<I, IPair<C, O>> SelectMany<I, M, O, A, B, C>(this Func<I, IPair<A, M>> state,
                                                                        Func<A, Func<M, IPair<B, O>>>  func,
                                                                        Func<A, B, C> selector)
        {
            return (i =>
                {
                    var am = state.Run(i);
                    var a = am.Left;
                    var bo = func(a).Run(am.Right);
                    return Pair<C, O>.Create(selector(a, bo.Left), bo.Right);
                });
        }

        // State manipulation primitives

        public static Func<I, IPair<I, I>> Get<I>()
        {
            return (i => Pair<I, I>.Create(i, i));
        }

        public static Func<I, IPair<Unit, O>> Put<I, O>(O o)
        {
            return (_ => Pair<Unit, O>.Create(Unit.Default, o));
        }

        public static Func<I, IPair<Unit, O>> Modify<I, O>(Func<I, O> func)
        {
            return (i => Pair<Unit, O>.Create(Unit.Default, func(i)));
        }
    }
}