using System.Collections.Generic;
using System.Linq;
using System.Text;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{

    /// <summary>
    ///     A composite condition that groups multiple conditions with AND/OR logic
    /// </summary>
    public class CompositeCondition : BaseCondition
    {
        private readonly List<ICondition> _conditions = new();
        private readonly int _count;

        private ComparisonType _comparisonType = ComparisonType.IsTrue;
        /// <summary>
        ///     Create a new composite condition
        /// </summary>
        /// <param name="compositeType">Logic type for evaluating child conditions</param>
        /// <param name="count"></param>
        public CompositeCondition(CompositeType compositeType, int count = 0)
        {
            CompositeType = compositeType;
            _count = count;
        }

        /// <summary>
        ///     Logic type for this composite
        /// </summary>
        public CompositeType CompositeType { get; }

        /// <summary>
        ///     Child conditions in this composite
        /// </summary>
        public IReadOnlyList<ICondition> Conditions => _conditions;

        /// <summary>
        ///     The type of this condition
        /// </summary>

        public override ConditionDataType DataType => ConditionDataType.Composite;

        public override ComparisonType ComparisonType => _comparisonType;

        /// <summary>
        ///     Set the comparison type for this composite condition
        /// </summary>
        public void SetComparisonType(ComparisonType comparisonType)
        {
            if (comparisonType == ComparisonType.IsTrue || comparisonType == ComparisonType.IsFalse)
            {
                _comparisonType = comparisonType;
            }
        }

        /// <summary>
        ///     Add a condition to this composite
        /// </summary>
        public void AddCondition(ICondition condition)
        {
            if (condition != null)
            {
                _conditions.Add(condition);
            }
        }

        /// <summary>
        ///     Remove a condition from this composite
        /// </summary>
        public bool RemoveCondition(ICondition condition) => _conditions.Remove(condition);

        /// <summary>
        ///     Clear all conditions from this composite
        /// </summary>
        public void ClearConditions()
        {
            _conditions.Clear();
        }

        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            // Empty condition group is always true
            if (_conditions.Count == 0)
            {
                return _comparisonType == ComparisonType.IsTrue;
            }

            // Evaluate based on logic type
            bool result = CompositeType switch
            {
                CompositeType.And => _conditions.All(c => c.Evaluate(context)),
                CompositeType.Or => _conditions.Any(c => c.Evaluate(context)),
                _ => false
            };

            // If comparison type is IsFalse, invert the result (NAND/NOR logic)
            return _comparisonType == ComparisonType.IsTrue ? result : !result;
        }
        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription()
        {
            if (_conditions.Count == 0)
                return "Empty Group";

            string logicOperator = CompositeType == CompositeType.And ? " AND " : " OR ";
            bool isNegated = _comparisonType == ComparisonType.IsFalse;

            StringBuilder sb = new();

            // Add NOT if using IsFalse comparison type
            if (isNegated)
                sb.Append("NOT ");

            sb.Append("(");

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (i > 0)
                    sb.Append(logicOperator);

                sb.Append(_conditions[i].GetDescription());
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}
