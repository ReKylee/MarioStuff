namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Data for parameters that can be used in conditions
    /// </summary>
    public class ParameterData
    {


        /// <summary>
        ///     Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Data type of the parameter
        /// </summary>
        public ConditionDataType Type { get; set; }

        /// <summary>
        ///     Default value for the parameter
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
