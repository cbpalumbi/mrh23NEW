using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recognissimo
{
    /// <summary>
    ///     Callback raised when initialization failed.
    /// </summary>
    public delegate void InitializationFailedCallback(string failedTaskName, Exception exception);

    /// <summary>
    ///     Callback raised when a new initialization task is started
    /// </summary>
    public delegate void InitializationTaskStartedCallback(string taskName, bool isLongRunning);

    /// <summary>
    ///     This class extends <see cref="MonoBehaviour" /> by adding lazy evaluation of user-defined
    ///     initialization tasks.
    /// </summary>
    public abstract class SpeechProcessorDependency : MonoBehaviour
    {
        private readonly Dictionary<int, Action> _actions = new();
        private readonly Dictionary<int, Func<IEnumerator>> _coroutines = new();
        private readonly List<TaskData> _tasks = new();

        private InitializationState _initializationState;

        private bool _isFirstRun = true;

        private int _taskRegistrationIndex;

        private void WrapThrowingAction(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
                _initializationState.Exception = exception;
            }
        }

        private IEnumerator WrapThrowingCoroutine(Func<IEnumerator> coroutine)
        {
            var enumerators = new Stack<IEnumerator>();

            enumerators.Push(coroutine());

            while (enumerators.Count > 0)
            {
                var currentEnumerator = enumerators.Peek();

                object currentYielded;

                try
                {
                    if (!currentEnumerator.MoveNext())
                    {
                        enumerators.Pop();
                        continue;
                    }

                    currentYielded = currentEnumerator.Current;
                }
                catch (Exception exception)
                {
                    _initializationState.Exception = exception;
                    yield break;
                }

                if (currentYielded is IEnumerator yieldedAsEnumerator)
                {
                    enumerators.Push(yieldedAsEnumerator);
                }
                else
                {
                    yield return currentYielded;
                }
            }
        }

        /// <summary>
        ///     Register initialization task.
        ///     Task will be executed on the first call to <see cref="Initialize" />
        ///     and on subsequent calls if <paramref name="callCondition" /> is true.
        ///     Tasks order is preserved.
        /// </summary>
        /// <param name="taskName">Name of the task.</param>
        /// <param name="task">Initialization task.</param>
        /// <param name="callCondition">Task call condition.</param>
        protected void RegisterInitializationTask(string taskName, Action task, CallCondition callCondition)
        {
            _isFirstRun = true;

            var id = _taskRegistrationIndex++;

            _actions.Add(id, task);

            _tasks.Add(new TaskData
            {
                ID = id,
                Name = taskName,
                Type = CallableType.Action,
                Condition = callCondition
            });
        }

        /// <inheritdoc cref="RegisterInitializationTask(string, Action, CallCondition)" />
        protected void RegisterInitializationTask(string taskName, Func<IEnumerator> task, CallCondition callCondition)
        {
            _isFirstRun = true;

            var id = _taskRegistrationIndex++;

            _coroutines.Add(id, task);

            _tasks.Add(new TaskData
            {
                ID = id,
                Name = taskName,
                Type = CallableType.Coroutine,
                Condition = callCondition
            });
        }

        /// <summary>
        ///     Execute all initialization tasks registered by
        ///     <see cref="RegisterInitializationTask(string, Action, CallCondition)" />
        ///     (or any other overload) whose <see cref="CallCondition" /> is met.
        ///     At the first call all registered tasks will be executed regardless of their <see cref="CallCondition" />
        /// </summary>
        /// <param name="initializationTaskStartedCallback">
        ///     Callback invoked when a new initialization task is started.
        /// </param>
        /// <param name="initializationFailedCallback">
        ///     Callback invoked when exception is thrown during initialization.
        /// </param>
        /// <returns>Enumerator to run coroutine on.</returns>
        public IEnumerator Initialize(InitializationTaskStartedCallback initializationTaskStartedCallback,
            InitializationFailedCallback initializationFailedCallback)
        {
            _initializationState = new InitializationState
            {
                IsActive = true,
                Exception = null
            };

            var readyTasks = _tasks.Where(task => task.Condition.Check() || _isFirstRun);

            foreach (var task in readyTasks)
            {
                initializationTaskStartedCallback?.Invoke(task.Name, task.Type == CallableType.Coroutine);

                switch (task.Type)
                {
                    case CallableType.Action:
                        WrapThrowingAction(_actions[task.ID]);
                        break;
                    case CallableType.Coroutine:
                        yield return WrapThrowingCoroutine(_coroutines[task.ID]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(task.Type));
                }

                if (_initializationState.Exception == null)
                {
                    continue;
                }

                initializationFailedCallback?.Invoke(task.Name, _initializationState.Exception);

                yield break;
            }

            _isFirstRun = false;

            _initializationState = new InitializationState
            {
                IsActive = false,
                Exception = null
            };
        }

        /// <summary>
        ///     Mark current initialization task as failed with specified <paramref name="exception" />.
        /// </summary>
        /// <param name="exception">Fail reason.</param>
        protected void FailInitialization(Exception exception)
        {
            if (!_initializationState.IsActive)
            {
                throw new InvalidOperationException(
                    $"Method {nameof(FailInitialization)} must be called during initialization.");
            }

            _initializationState.Exception = exception;
        }

        private struct InitializationState
        {
            public bool IsActive;
            public Exception Exception;
        }

        private enum CallableType
        {
            Action,
            Coroutine
        }

        private struct TaskData
        {
            public int ID;
            public string Name;
            public CallableType Type;
            public CallCondition Condition;
        }
    }
}