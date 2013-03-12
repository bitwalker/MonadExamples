using System;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    public delegate Pair<TValue, TOutput> State<in TInput, out TOutput, out TValue> (TInput input);

    public static class StateExtensions
    {
        public static Pair<TValue, TOutput> Run<TInput, TOutput, TValue> (this State<TInput, TOutput, TValue> state, TInput input)
        {
            return state (input);
        }

        public static Pair<TValue, TOutput> Eval<TInput, TOutput, TValue> (this State<TInput, TOutput, TValue> state, TInput input)
        {
            return state (input).Left;
        }

        public static Pair<TValue, TOutput> Exec<TInput, TOutput, TValue> (this State<TInput, TOutput, TValue> state, TInput input)
        {
            return state (input).Right;
        }

        // unit
        public static State<S, S, A> ToState<S, A> (this A a)
        {
            return (s => Pair.Create<A, S> (a, s));
        }

        // map
        public static State<I, O, B> Select<I, O, A, B> (this State<I, O, A> st, Func<A, B> func)
        {
            return (i =>
            {
                var ao = st.Run (i);
                return Pair.Create<B, O> (func (ao.Left), ao.Right);
            });
        }

        // join
        public static State<I, O, A> Flatten<I, M, O, A> (this State<I, M, State<M, O, A>> st)
        {
            return (i =>
            {
                var qm = st.Run (i);
                return qm.Left.Run (qm.Right);
            });
        }

        // bind
        public static State<I, O, B> SelectMany<I, M, O, A, B> (this State<I, M, A> st, Func<A, State<M, O, B>> func)
        {
            return (i =>
            {
                var am = st.Run (i);
                return func (am.Left).Run (am.Right);
            });
        }

        // bindMap
        public static State<I, O, C> SelectMany<I, M, O, A, B, C> (this State<I, M, A> st, Func<A, State<M, O, B>> func, Func<A, B, C> selector)
        {
            return (i =>
            {
                var am = st.Run (i);
                var a = am.Left;
                var bo = func (a).Run (am.Right);
                return Pair.Create<C, O> (selector (a, bo.Left), bo.Right);
            });
        }

        public static State<S, S, S> Get<S> ()
        {
            return (s => Pair.Create<S, S> (s, s));
        }

        public static State<I, O, Unit> Put<I, O> (O o)
        {
            return (_ => Pair.Create<Unit, O> (Unit.Default, o));
        }
       
        public static State<I, O, Unit> Modify<I, O> (Func<I, O> func)
        {
            return (i => Pair.Create<Unit, O> (Unit.Default, func (i)));
        }
    }
}