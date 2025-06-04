using Animation.Flow.Conditions;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Represents a draggable parameter
    /// </summary>
    public class ParameterData
    {
        public string Name { get; set; }
        public ConditionDataType Type { get; set; }
        public object DefaultValue { get; set; }
    }
}
