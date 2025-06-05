using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Conditions.SpecialConditions;
using UnityEngine;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Factory for creating condition instances
    /// </summary>
    public static class ConditionFactory
    {
        #region Parameter Conditions

        /// <summary>
        ///     Create a boolean condition
        /// </summary>
        public static BoolCondition CreateBoolCondition(string parameterName, bool expectedValue = true, bool isNegated = false)
        {
            return new BoolCondition(parameterName, expectedValue, isNegated);
        }

        /// <summary>
        ///     Create a float condition
        /// </summary>
        public static FloatCondition CreateFloatCondition(string parameterName, float compareValue, 
            ComparisonType comparisonType = ComparisonType.Equal, bool isNegated = false)
        {
            return new FloatCondition(parameterName, compareValue, comparisonType, 0.0001f, isNegated);
        }

        /// <summary>
        ///     Create an integer condition
        /// </summary>
        public static IntCondition CreateIntCondition(string parameterName, int compareValue,
            ComparisonType comparisonType = ComparisonType.Equal, bool isNegated = false)
        {
            return new IntCondition(parameterName, compareValue, comparisonType, isNegated);
        }

        /// <summary>
        ///     Create a string condition
        /// </summary>
        public static StringCondition CreateStringCondition(string parameterName, string compareValue,
            ComparisonType comparisonType = ComparisonType.Equal, bool ignoreCase = true, bool isNegated = false)
        {
            return new StringCondition(parameterName, compareValue, comparisonType, ignoreCase, isNegated);
        }

        #endregion

        #region Special Conditions

        /// <summary>
        ///     Create an animation complete condition
        /// </summary>
        public static AnimationCompleteCondition CreateAnimationCompleteCondition(bool isNegated = false)
        {
            return new AnimationCompleteCondition(isNegated);
        }

        /// <summary>
        ///     Create a time condition
        /// </summary>
        public static TimeCondition CreateTimeCondition(float duration, 
            ComparisonType comparisonType = ComparisonType.GreaterOrEqual, bool isNegated = false)
        {
            return new TimeCondition(duration, comparisonType, isNegated);
        }

        #endregion

        #region Composite Conditions

        /// <summary>
        ///     Create an AND composite condition
        /// </summary>
        public static CompositeCondition CreateAndCondition(bool isNegated = false)
        {
            return new CompositeCondition(CompositeType.And, isNegated);
        }

        /// <summary>
        ///     Create an OR composite condition
        /// </summary>
        public static CompositeCondition CreateOrCondition(bool isNegated = false)
        {
            return new CompositeCondition(CompositeType.Or, isNegated);
        }

        #endregion

        #region Serialization Support

        /// <summary>
        ///     Create a condition from serialized data
        /// </summary>
        public static FlowCondition CreateFromData(ConditionData data)
        {
            if (data == null) return null;

            try
            {
                switch (data.Type)
                {
                    case ConditionType.ParameterComparison:
                        return CreateParameterConditionFromData(data);

                    case ConditionType.AnimationComplete:
                        return new AnimationCompleteCondition(data.IsNegated);

                    case ConditionType.TimeBased:
                        return new TimeCondition(data.FloatValue, 
                            (ComparisonType)data.IntValue, data.IsNegated);

                    case ConditionType.Composite:
                        var composite = new CompositeCondition((CompositeType)data.IntValue, data.IsNegated);

                        if (data.ChildConditions != null)
                        {
                            foreach (var childData in data.ChildConditions)
                            {
                                var child = CreateFromData(childData);
                                if (child != null)
                                {
                                    composite.AddCondition(child);
                                }
                            }
                        }

                        return composite;

                    default:
                        Debug.LogWarning($"Unknown condition type: {data.Type}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating condition from data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Create a parameter condition from serialized data
        /// </summary>
        private static FlowCondition CreateParameterConditionFromData(ConditionData data)
        {
            if (string.IsNullOrEmpty(data.ParameterName))
                return null;

            switch (data.ParameterValueType)
            {
                case ParameterValueType.Bool:
                    return new BoolCondition(data.ParameterName, data.BoolValue, data.IsNegated);

                case ParameterValueType.Float:
                    return new FloatCondition(data.ParameterName, data.FloatValue, 
                        (ComparisonType)data.IntValue, 0.0001f, data.IsNegated);

                case ParameterValueType.Int:
                    return new IntCondition(data.ParameterName, data.IntValue, 
                        (ComparisonType)data.ComparisonType, data.IsNegated);

                case ParameterValueType.String:
                    return new StringCondition(data.ParameterName, data.StringValue, 
                        (ComparisonType)data.ComparisonType, data.BoolValue, data.IsNegated);

                default:
                    Debug.LogWarning($"Unknown parameter value type: {data.ParameterValueType}");
                    return null;
            }
        }

        /// <summary>
        ///     Create serialization data from a condition
        /// </summary>
        public static ConditionData CreateDataFromCondition(FlowCondition condition)
        {
            if (condition == null) return null;

            var data = new ConditionData
            {
                Type = condition.ConditionType,
                IsNegated = condition.IsNegated
            };

            switch (condition)
            {
                case BoolCondition boolCond:
                    data.ParameterName = boolCond.ParameterName;
                    data.ParameterValueType = ParameterValueType.Bool;
                    data.BoolValue = boolCond.ExpectedValue;
                    break;

                case FloatCondition floatCond:
                    data.ParameterName = floatCond.ParameterName;
                    data.ParameterValueType = ParameterValueType.Float;
                    data.FloatValue = floatCond.CompareValue;
                    data.IntValue = (int)floatCond.ComparisonType;
                    data.ComparisonType = floatCond.ComparisonType;
                    break;

                case IntCondition intCond:
                    data.ParameterName = intCond.ParameterName;
                    data.ParameterValueType = ParameterValueType.Int;
                    data.IntValue = intCond.CompareValue;
                    data.ComparisonType = intCond.ComparisonType;
                    break;

                case StringCondition stringCond:
                    data.ParameterName = stringCond.ParameterName;
                    data.ParameterValueType = ParameterValueType.String;
                    data.StringValue = stringCond.CompareValue;
                    data.ComparisonType = stringCond.ComparisonType;
                    data.BoolValue = stringCond.IgnoreCase; // Store ignoreCase in BoolValue
                    break;

                case TimeCondition timeCond:
                    data.FloatValue = timeCond.Duration;
                    data.IntValue = (int)timeCond.ComparisonType;
                    break;

                case CompositeCondition compositeCond:
                    data.IntValue = (int)compositeCond.CompositeType;

                    if (compositeCond.Conditions.Count > 0)
                    {
                        data.ChildConditions = new List<ConditionData>();
                        foreach (var child in compositeCond.Conditions)
                        {
                            var childData = CreateDataFromCondition(child);
                            if (childData != null)
                            {
                                data.ChildConditions.Add(childData);
                            }
                        }
                    }
                    break;
            }

            return data;
        }

        #endregion
    }
}
