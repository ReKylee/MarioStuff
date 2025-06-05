using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Leaves
{
    /// <summary>
    ///     Checks a parameter against a condition and returns success or failure
    /// </summary>
    [CreateAssetMenu(fileName = "New Check Parameter", menuName = "Animation/Flow/Nodes/Leaves/Check Parameter")]
    public class CheckParameterNode : FlowNode
    {
        public enum ParameterType
        {
            Bool,
            Int,
            Float,
            String
        }

        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater, // Not applicable to bool/string
            Less, // Not applicable to bool/string
            GreaterOrEqual, // Not applicable to bool/string
            LessOrEqual // Not applicable to bool/string
        }

        [SerializeField] private string _parameterName;
        [SerializeField] private ParameterType _parameterType;
        [SerializeField] private ComparisonType _comparisonType;

        // Value to compare against
        [SerializeField] private bool _boolValue;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private string _stringValue;

        /// <summary>
        ///     Name of the parameter to check
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value;
        }

        /// <summary>
        ///     Type of the parameter
        /// </summary>
        public ParameterType Type
        {
            get => _parameterType;
            set => _parameterType = value;
        }

        /// <summary>
        ///     Type of comparison to perform
        /// </summary>
        public ComparisonType Comparison
        {
            get => _comparisonType;
            set => _comparisonType = value;
        }

        /// <summary>
        ///     Checks the parameter and returns success or failure
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            if (string.IsNullOrEmpty(_parameterName))
            {
                return NodeStatus.Failure;
            }

            bool result;

            switch (_parameterType)
            {
                case ParameterType.Bool:
                    result = CheckBoolParameter(context);
                    break;
                case ParameterType.Int:
                    result = CheckIntParameter(context);
                    break;
                case ParameterType.Float:
                    result = CheckFloatParameter(context);
                    break;
                case ParameterType.String:
                    result = CheckStringParameter(context);
                    break;
                default:
                    return NodeStatus.Failure;
            }

            return result ? NodeStatus.Success : NodeStatus.Failure;
        }

        private bool CheckBoolParameter(AnimationContext context)
        {
            bool value = context.GetBool(_parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => value == _boolValue,
                ComparisonType.NotEqual => value != _boolValue,
                _ => false // Other comparison types don't apply to booleans
            };
        }

        private bool CheckIntParameter(AnimationContext context)
        {
            int value = context.GetInt(_parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => value == _intValue,
                ComparisonType.NotEqual => value != _intValue,
                ComparisonType.Greater => value > _intValue,
                ComparisonType.Less => value < _intValue,
                ComparisonType.GreaterOrEqual => value >= _intValue,
                ComparisonType.LessOrEqual => value <= _intValue,
                _ => false
            };
        }

        private bool CheckFloatParameter(AnimationContext context)
        {
            float value = context.GetFloat(_parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => Mathf.Approximately(value, _floatValue),
                ComparisonType.NotEqual => !Mathf.Approximately(value, _floatValue),
                ComparisonType.Greater => value > _floatValue,
                ComparisonType.Less => value < _floatValue,
                ComparisonType.GreaterOrEqual => value >= _floatValue,
                ComparisonType.LessOrEqual => value <= _floatValue,
                _ => false
            };
        }

        private bool CheckStringParameter(AnimationContext context)
        {
            string value = context.GetString(_parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => value == _stringValue,
                ComparisonType.NotEqual => value != _stringValue,
                _ => false // Other comparison types don't apply to strings
            };
        }
    }
}
