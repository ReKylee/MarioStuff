using System;
using Animation.Flow.Interfaces;
using Animation.Flow.States;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Factory for creating animation states
    /// </summary>
    public static class StateFactory
    {
        /// <summary>
        ///     Create a state from serialized data
        /// </summary>
        public static IAnimationState CreateFromData(AnimationStateData stateData)
        {
            if (stateData == null)
            {
                throw new ArgumentNullException(nameof(stateData));
            }

            AnimationStateType stateType = stateData.StateType;

            // Use the registry to create the state
            switch (stateType)
            {
                case AnimationStateType.HoldFrame:
                    return new HoldFrameState(stateData.Id, stateData.AnimationName, stateData.FrameToHold);

                case AnimationStateType.Looping:
                    return new LoopingState(stateData.Id, stateData.AnimationName);

                case AnimationStateType.OneTime:
                    return new OneTimeState(stateData.Id, stateData.AnimationName);

                default:
                    throw new ArgumentException($"Unknown state type: {stateType}");
            }
        }

        /// <summary>
        ///     Create a new state of the specified type
        /// </summary>
        public static IAnimationState Create(AnimationStateType stateType, string id, string animationName, int frameToHold = 0)
        {
            return stateType switch
            {
                AnimationStateType.HoldFrame => new HoldFrameState(id, animationName, frameToHold),
                AnimationStateType.Looping => new LoopingState(id, animationName),
                AnimationStateType.OneTime => new OneTimeState(id, animationName),
                _ => throw new ArgumentException($"Unknown state type: {stateType}")
            };
        }

        /// <summary>
        ///     Create a new state of a specific implementation type
        /// </summary>
        public static T Create<T>(string id, string animationName, params object[] parameters) where T : IAnimationState
        {
            // Handle special cases
            if (typeof(T) == typeof(HoldFrameState))
            {
                int frameToHold = 0;
                if (parameters.Length > 0 && parameters[0] is int frame)
                {
                    frameToHold = frame;
                }

                return (T)(IAnimationState)new HoldFrameState(id, animationName, frameToHold);
            }

            if (typeof(T) == typeof(LoopingState))
            {
                return (T)(IAnimationState)new LoopingState(id, animationName);
            }

            if (typeof(T) == typeof(OneTimeState))
            {
                return (T)(IAnimationState)new OneTimeState(id, animationName);
            }

            throw new ArgumentException($"Unsupported state type: {typeof(T).Name}");
        }
    }
}
