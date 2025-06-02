namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Common transition conditions for animation flow
    /// </summary>
    /// <summary>
    ///     Transition when a boolean parameter has a specific value
    /// </summary>
    public class BoolCondition : ITransitionCondition
    {
        private readonly string _parameterName;
        private readonly bool _targetValue;

        public BoolCondition(string parameterName, bool targetValue = true)
        {
            _parameterName = parameterName;
            _targetValue = targetValue;
        }

        public bool IsSatisfied(IAnimationContext context) =>
            context.GetParameter<bool>(_parameterName) == _targetValue;
    }

    /// <summary>
    ///     Transition when a float parameter is in a specific range
    /// </summary>
    public class FloatRangeCondition : ITransitionCondition
    {
        private readonly float _maxValue;
        private readonly float _minValue;
        private readonly string _parameterName;

        public FloatRangeCondition(string parameterName, float minValue, float maxValue)
        {
            _parameterName = parameterName;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public bool IsSatisfied(IAnimationContext context)
        {
            float value = context.GetParameter<float>(_parameterName);
            return value >= _minValue && value <= _maxValue;
        }
    }

    /// <summary>
    ///     Transition when an animation has completed
    /// </summary>
    public class AnimationCompleteCondition : ITransitionCondition
    {
        public bool IsSatisfied(IAnimationContext context) =>
            // Use IAnimator interface, no cast needed
            context.Animator != null && context.Animator.IsAnimationComplete;
    }

    /// <summary>
    ///     Transition after a certain amount of time has passed
    /// </summary>
    public class TimeElapsedCondition : ITransitionCondition
    {
        private readonly float _duration;
        private readonly string _timerParameterName;

        public TimeElapsedCondition(string timerParameterName, float duration)
        {
            _timerParameterName = timerParameterName;
            _duration = duration;
        }

        public bool IsSatisfied(IAnimationContext context)
        {
            float elapsedTime = context.GetParameter<float>(_timerParameterName);
            return elapsedTime >= _duration;
        }
    }

    /// <summary>
    ///     Compound condition that requires all conditions to be true (AND)
    /// </summary>
    public class AllCondition : ITransitionCondition
    {
        private readonly ITransitionCondition[] _conditions;

        public AllCondition(params ITransitionCondition[] conditions)
        {
            _conditions = conditions;
        }

        public bool IsSatisfied(IAnimationContext context)
        {
            foreach (ITransitionCondition condition in _conditions)
            {
                if (!condition.IsSatisfied(context))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    ///     Compound condition that requires any condition to be true (OR)
    /// </summary>
    public class AnyCondition : ITransitionCondition
    {
        private readonly ITransitionCondition[] _conditions;

        public AnyCondition(params ITransitionCondition[] conditions)
        {
            _conditions = conditions;
        }

        public bool IsSatisfied(IAnimationContext context)
        {
            foreach (ITransitionCondition condition in _conditions)
            {
                if (condition.IsSatisfied(context))
                    return true;
            }

            return false;
        }
    }
}
