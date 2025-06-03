namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Factory for creating composite condition groups
    /// </summary>
    public static class ConditionGroupFactory
    {
        /// <summary>
        ///     Create an AND group of conditions
        /// </summary>
        public static CompositeCondition CreateAndGroup(params ICondition[] conditions)
        {
            CompositeCondition group = new(CompositeType.And);
            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an OR group of conditions
        /// </summary>
        public static CompositeCondition CreateOrGroup(params ICondition[] conditions)
        {
            CompositeCondition group = new(CompositeType.Or);
            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an "At Least N" group of conditions
        /// </summary>
        public static CompositeCondition CreateAtLeastGroup(int requiredCount, params ICondition[] conditions)
        {
            CompositeCondition group = new(CompositeType.AtLeast, requiredCount);
            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an "Exactly N" group of conditions
        /// </summary>
        public static CompositeCondition CreateExactlyGroup(int requiredCount, params ICondition[] conditions)
        {
            CompositeCondition group = new(CompositeType.Exactly, requiredCount);
            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }

        /// <summary>
        ///     Create an "At Most N" group of conditions
        /// </summary>
        public static CompositeCondition CreateAtMostGroup(int requiredCount, params ICondition[] conditions)
        {
            CompositeCondition group = new(CompositeType.AtMost, requiredCount);
            foreach (ICondition condition in conditions)
            {
                group.AddCondition(condition);
            }

            return group;
        }
    }
}
