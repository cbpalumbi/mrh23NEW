using System;
using System.Collections.Generic;
using System.Linq;

namespace Recognissimo
{
    /// <summary>
    ///     Helper class for <see cref="SpeechProcessorDependency" /> that stores the condition of the associated call.
    /// </summary>
    public class CallCondition
    {
        /// <summary>
        ///     <see cref="CallCondition" /> which allows to execute the associated call only once.
        /// </summary>
        public static readonly CallCondition Once = new(() => false);

        /// <summary>
        ///     <see cref="CallCondition" /> that always allows to execute the associated call.
        /// </summary>
        public static readonly CallCondition Always = new(() => true);

        private readonly Func<bool> _condition;

        /// <summary>
        ///     Construct <see cref="CallCondition" /> from a predicate.
        /// </summary>
        /// <param name="predicate">
        ///     Predicate function, the return value of which determines whether or not to execute the
        ///     initialization task to which it is linked.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="predicate" /> is null.</exception>
        public CallCondition(Func<bool> predicate)
        {
            _condition = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        ///     Aggregates multiple conditions into one.
        /// </summary>
        /// <param name="conditions">Array of conditions.</param>
        /// <returns><see cref="CallCondition" /> instance.</returns>
        public static CallCondition Aggregate(CallCondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
            {
                return Once;
            }

            return new CallCondition(() => { return conditions.Any(cond => cond.Check()); });
        }

        /// <summary>
        ///     Create <see cref="CallCondition" /> that allows to execute the associated call only
        ///     if the return value of <paramref name="dependencyGetter" /> changes.
        /// </summary>
        /// <param name="dependencyGetter">
        ///     Function, a change in the return value of which activates the
        ///     <see cref="CallCondition" />.
        /// </param>
        /// <param name="equalityComparer">
        ///     Custom equality comparer. If null, <see cref="EqualityComparer{T}.Equals(T,T)" /> is
        ///     used.
        /// </param>
        /// <typeparam name="T">Generic parameter of <paramref name="dependencyGetter" />.</typeparam>
        /// <returns><see cref="CallCondition" /> instance.</returns>
        public static CallCondition ValueChanged<T>(Func<T> dependencyGetter, Func<T, T, bool> equalityComparer = null)
        {
            if (dependencyGetter == null)
            {
                return Once;
            }

            equalityComparer ??= EqualityComparer<T>.Default.Equals;

            var currentValue = dependencyGetter();

            return new CallCondition(() =>
            {
                var newValue = dependencyGetter();

                if (equalityComparer(currentValue, newValue))
                {
                    return false;
                }

                currentValue = newValue;
                return true;
            });
        }

        /// <summary>
        ///     Check if the condition is satisfied.
        /// </summary>
        /// <returns>Value of underlying condition.</returns>
        public bool Check()
        {
            return _condition();
        }
    }
}