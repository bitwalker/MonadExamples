using System;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    /// <summary>
    /// The Indexed State monad is implemented as a function from the initial state
    /// to a Pair of the result and the final state. This can be described with the notation:
    /// 
    ///     f(TInitialState) -> IPair[TResult, TNextState]
    ///
    /// This pair is the basis for continuing to transform the result and the state, extracting
    /// the result, or moving to the next state. In this way, the IndexedState monad is a lot like
    /// a deterministic finite state machine, as all of the steps must be described with the type
    /// system, and the valid transformations are described by the implementations of those types.
    /// See the ClientConnection example in IndexedStateExample.cs.
    /// </summary>
    public static class State
    {

        /// <summary>
        /// Runs a state transformation function on the input value.
        /// </summary>
        /// <typeparam name="TInitial">The type of the initial value (state)</typeparam>
        /// <typeparam name="TResult">The type of the data produced by the transformation</typeparam>
        /// <typeparam name="TNext">The type of the updated state</typeparam>
        /// <param name="transformer">The transformation function</param>
        /// <param name="input">The input state</param>
        /// <returns>IPair of type TResult/TNext</returns>
        public static IPair<TResult, TNext> Run<TInitial, TResult, TNext>(this Func<TInitial, IPair<TResult, TNext>> transformer, TInitial state)
        {
            return transformer(state);
        }

        /// <summary>
        /// Runs a state transformation function on the input value,
        /// and returns the data produced by the transformation
        /// </summary>
        /// <typeparam name="TInitial">The type of the initial value (state)</typeparam>
        /// <typeparam name="TResult">The type of the data produced by the transformation</typeparam>
        /// <typeparam name="TNext">The type of the updated state</typeparam>
        /// <param name="transformer">The transformation function</param>
        /// <param name="input">The input state</param>
        public static TResult Eval<TInitial, TResult, TNext>(this Func<TInitial, IPair<TResult, TNext>> transformer, TInitial state)
        {
            return transformer(state).Left;
        }

        /// <summary>
        /// Runs a state transformation function on the input value,
        /// and returns the new state produced by the transformation.
        /// </summary>
        /// <typeparam name="TInitial">The type of the initial value (state)</typeparam>
        /// <typeparam name="TResult">The type of the data produced by the transformation</typeparam>
        /// <typeparam name="TNext">The type of the updated state</typeparam>
        /// <param name="transformer">The transformation function</param>
        /// <param name="input">The input state</param>
        public static TNext Exec<TInitial, TResult, TNext>(this Func<TInitial, IPair<TResult, TNext>> transformer, TInitial state)
        {
            return transformer(state).Right;
        }

        // NOTE: The following methods implement the monad, which in our case is synonmous with Linq.

        /// <summary>
        /// Given a value, produce a function which takes the initial state, and returns
        /// a pair of the value and the initial state. This implements the monadic Unit function.
        /// </summary>
        /// <typeparam name="TInitial">The type of the initial state</typeparam>
        /// <typeparam name="TResult">The type of the result value</typeparam>
        /// <param name="input">The input value</param>
        public static Func<TInitial, IPair<TResult, TInitial>> ToState<TInitial, TResult>(this TResult input)
        {
            return (initialState => new Pair<TResult, TInitial>(input, initialState));
        }

        /// <summary>
        /// Given a state transformation function, and a function which transforms the result data,
        /// produce a new function which takes the initial state, runs it through the state transformer,
        /// executes the result data transformer on the result value, and produces a new IPair value of
        /// type TTransformed/TNext. This implements the monadic Map function.
        /// </summary>
        /// <typeparam name="TInitial">The type of the initial state</typeparam>
        /// <typeparam name="TNext">The type of the updated state</typeparam>
        /// <typeparam name="TResult">The type of the initial result</typeparam>
        /// <typeparam name="TTransformed">The type of the transformed result</typeparam>
        /// <param name="stateTransformer">The state transformation function</param>
        /// <param name="resultTransformer">The result value transformation function</param>
        public static Func<TInitial, IPair<TTransformed, TNext>> Select<TInitial, TNext, TResult, TTransformed>(
            this Func<TInitial, IPair<TResult, TNext>> stateTransformer, Func<TResult, TTransformed> resultTransformer
        )
        {
            return (i =>
                {
                    var updatedState      = stateTransformer.Run(i);
                    var transformedResult = resultTransformer(updatedState.Left);
                    return new Pair<TTransformed, TNext>(transformedResult, updatedState.Right);
                });
        }

        /// <summary>
        /// Given a state transformation function where the result type is a function which produces a pair,
        /// produce a new function which takes the initial state value, runs the transformation function,
        /// then runs the result function with the state value produced by the former. This implements
        /// the monadic Join function
        /// </summary>
        /// <typeparam name="I">The type of the initial state</typeparam>
        /// <typeparam name="M">The type of the initial result value</typeparam>
        /// <typeparam name="O">The type of the next state</typeparam>
        /// <typeparam name="A">The type of the transformed result value</typeparam>
        /// <param name="transformer">The state transformation function</param>
        public static Func<TInitial, IPair<TTransformed, TNext>> Flatten<TInitial, TResult, TNext, TTransformed>(
            this Func<TInitial, IPair<Func<TResult, IPair<TTransformed, TNext>>, TResult>>  transformer
        )
        {
            return (i =>
                {
                    var updatedState = transformer.Run(i);
                    return updatedState.Left.Run(updatedState.Right);
                });
        }

        /// <summary>
        /// Produces a state transformation function which flattens nested
        /// state transformation results (nested IPairs). This implements the monadic Bind function.
        /// </summary>
        /// <typeparam name="I">Type of the initial state</typeparam>
        /// <typeparam name="M">Type of the intermediate (next) state</typeparam>
        /// <typeparam name="O">Type of the final state</typeparam>
        /// <typeparam name="A">Type of the initial result value</typeparam>
        /// <typeparam name="B">Type of the transformed result value</typeparam>
        /// <param name="stateTransformer">The current state transformation function</param>
        /// <param name="stateProducer">A function which produces a state transformation function</param>
        public static Func<TInitial, IPair<TTransformed, TFinal>> SelectMany<TInitial, TNext, TFinal, TResult, TTransformed>(
            this Func<TInitial, IPair<TResult, TNext>> stateTransformer,
            Func<TResult, Func<TNext, IPair<TTransformed, TFinal>>>  stateProducer)
        {
            return (i =>
                {
                    var updatedState = stateTransformer.Run(i);
                    return stateProducer(updatedState.Left).Run(updatedState.Right);
                });
        }

        // bindMap
        /// <summary>
        /// Represents a transformation of the result value during a monadic Bind operation.
        /// Effectively the same as SelectMany, with the addition of a function which takes the
        /// initial result value, and the intermediate result value, and must produce a new result value.
        /// Implements the monadic BindMap function.
        /// </summary>
        /// <typeparam name="I">Type of the initial state</typeparam>
        /// <typeparam name="M">Type of the intermediate (next) state</typeparam>
        /// <typeparam name="O">Type of the final state</typeparam>
        /// <typeparam name="A">Type of the initial result</typeparam>
        /// <typeparam name="B">Type of the intermediate (next) result</typeparam>
        /// <typeparam name="C">Type of the final result</typeparam>
        /// <param name="stateTransformer">The state transformation function</param>
        /// <param name="stateProducer">A function which produces a state transformation function</param>
        /// <param name="resultTransformer">A function which produces a new result given the initial and intermediate results</param>
        public static Func<TInitial, IPair<C, TFinal>> SelectMany<TInitial, TNext, TFinal, A, B, C>(
            this Func<TInitial, IPair<A, TNext>> stateTransformer,
            Func<A, Func<TNext, IPair<B, TFinal>>>  stateProducer,
            Func<A, B, C> resultTransformer)
        {
            return (i =>
                {
                    var updatedState  = stateTransformer.Run(i);
                    var initialResult = updatedState.Left;
                    var nextState     = stateProducer(initialResult).Run(updatedState.Right);
                    var result        = resultTransformer(initialResult, nextState.Left);
                    return new Pair<C, TFinal>(result, nextState.Right);
                });
        }

        // NOTE: State manipulation primitives

        /// <summary>
        /// Returns a function which produces an IPair from an input value.
        /// </summary>
        /// <typeparam name="T">The type of both sides of the IPair</typeparam>
        public static Func<T, IPair<T, T>> Get<T>()
        {
            return (input => new Pair<T, T>(input, input));
        }

        /// <summary>
        /// Returns a function which produces an IPair with a constant value (state).
        /// </summary>
        /// <typeparam name="TInput">The type of the input value</typeparam>
        /// <typeparam name="TRight">The type of the right hand side of the produced IPair</typeparam>
        /// <param name="value">The value to put in the pair on the right.</param>
        public static Func<TInput, IPair<Unit, TRight>> Put<TInput, TRight>(TRight value)
        {
            return (_ => new Pair<Unit, TRight>(Unit.Default, value));
        }

        /// <summary>
        /// Returns a function which produces an IPair by executing a function over an input value.
        /// </summary>
        /// <typeparam name="TInput">The type of the input value</typeparam>
        /// <typeparam name="TTransformed">The type of the transformed input value</typeparam>
        /// <param name="transformer">The transformation function</param>
        public static Func<TInput, IPair<Unit, TTransformed>> Modify<TInput, TTransformed>(Func<TInput, TTransformed> transformer)
        {
            return (i => new Pair<Unit, TTransformed>(Unit.Default, transformer(i)));
        }
    }
}