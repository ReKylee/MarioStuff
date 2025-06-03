using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Condition that checks if a certain amount of time has elapsed in the current state
    /// </summary>
    public class TimeElapsedCondition : BaseCondition
    {

        /// <summary>
        ///     Create a new time elapsed condition
        /// </summary>
        /// <param name="timeToWait">Time in seconds to wait before condition is true</param>
        public TimeElapsedCondition(float timeToWait)
        {
            TimeToWait = timeToWait;
        }

        /// <summary>
        ///     Time to wait in seconds
        /// </summary>
        public float TimeToWait { get; }

        public override ConditionDataType DataType => ConditionDataType.Time;
        public override ComparisonType ComparisonType => ComparisonType.GreaterThanOrEqual;
        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null || !context.HasParameter("StateTime"))
                return false;

            float stateTime = context.GetParameter<float>("StateTime");
            return stateTime >= TimeToWait;
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription() => $"Time Elapsed >= {TimeToWait:F2}s";
    }
}
