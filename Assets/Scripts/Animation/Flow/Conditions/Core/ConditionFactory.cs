using System;
using Animation.Flow.Conditions.Core;
using UnityEngine;

// Required for Debug.LogWarning

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Factory for creating condition instances from ConditionData
    /// </summary>
    public static class ConditionFactory
    {
        /// <summary>
        ///     Create an ICondition instance from ConditionData.
        /// </summary>
        public static ICondition CreateCondition(ConditionData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Attempted to create condition from null data.");
                return null;
            }

            if (data.DataType == ConditionDataType.Composite)
            {
                if (Enum.TryParse(data.StringValue, out CompositeType compositeType))
                {
                    // The 'count' for AtLeast, Exactly, AtMost comes from IntValue.
                    CompositeCondition condition = new(compositeType, data.IntValue);
                    // Set the comparison type for the composite (whether we want the group to be true or false)
                    condition.SetComparisonType(data.ComparisonType);
                    return condition;
                }

                Debug.LogWarning($"Invalid CompositeType string: {data.StringValue}. Defaulting to AND.");
                return new CompositeCondition(CompositeType.And);
            }

            string parameterName = data.ParameterName;
            // Value is implicitly handled by specific condition constructors or data fields

            switch (data.DataType)
            {
                case ConditionDataType.Boolean:
                    // BoolCondition's constructor takes (parameterName, expectedValue)
                    // For Boolean conditions, we always use Equals comparison type
                    return new BoolCondition(parameterName, data.BoolValue);

                case ConditionDataType.Float:
                    return new FloatCondition(parameterName, data.ComparisonType, data.FloatValue);

                case ConditionDataType.Integer:
                    return new IntCondition(parameterName, data.ComparisonType, data.IntValue);

                case ConditionDataType.String:
                    return new StringCondition(parameterName, data.ComparisonType, data.StringValue);

                case ConditionDataType.Time:
                    if (data.ComparisonType == ComparisonType.Elapsed)
                        return new TimeElapsedCondition(data.FloatValue);

                    Debug.LogWarning($"Unsupported ComparisonType {data.ComparisonType} for Time DataType.");
                    return null;

                case ConditionDataType.Animation:
                    if (data.ComparisonType == ComparisonType.Completed)
                        return
                            new AnimationCompleteCondition(); // ParameterName is not used by AnimationCompleteCondition

                    Debug.LogWarning($"Unsupported ComparisonType {data.ComparisonType} for Animation DataType.");
                    return null;

                default:
                    Debug.LogWarning($"Unsupported ConditionDataType: {data.DataType}");
                    return null;
            }
        }
    }
}
