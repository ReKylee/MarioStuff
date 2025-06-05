using System.Collections.Generic;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Extension methods for working with condition groups
    /// </summary>
    public static class ConditionGroupExtensions
    {
        /// <summary>
        ///     Group conditions with an AND operator
        /// </summary>
        public static CompositeCondition And(this ICondition condition, params ICondition[] others)
        {
            CompositeCondition group = new(CompositeType.And);
            group.AddCondition(condition);

            foreach (ICondition other in others)
            {
                group.AddCondition(other);
            }

            return group;
        }

        /// <summary>
        ///     Group conditions with an OR operator
        /// </summary>
        public static CompositeCondition Or(this ICondition condition, params ICondition[] others)
        {
            CompositeCondition group = new(CompositeType.Or);
            group.AddCondition(condition);

            foreach (ICondition other in others)
            {
                group.AddCondition(other);
            }

            return group;
        }


        /// <summary>
        ///     Create an "At Least N of these conditions" group
        /// </summary>
        public static CompositeCondition AtLeast(this IEnumerable<ICondition> conditions, int count)
        {
            CompositeCondition group = new(CompositeType.AtLeast, count);

            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an "Exactly N of these conditions" group
        /// </summary>
        public static CompositeCondition Exactly(this IEnumerable<ICondition> conditions, int count)
        {
            CompositeCondition group = new(CompositeType.Exactly, count);

            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an "At Most N of these conditions" group
        /// </summary>
        public static CompositeCondition AtMost(this IEnumerable<ICondition> conditions, int count)
        {
            CompositeCondition group = new(CompositeType.AtMost, count);

            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }
    }
}
