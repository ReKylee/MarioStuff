using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Editor.Panels.Conditions;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Factories
{
    /// <summary>
    ///     Factory for creating condition views
    /// </summary>
    public class ConditionViewFactory
    {
        private readonly ConditionListPanel _panel;
        private readonly Dictionary<ConditionDataType, Func<ConditionData, VisualElement>> _viewCreators;

        public ConditionViewFactory(ConditionListPanel panel)
        {
            _panel = panel;
            _viewCreators = new Dictionary<ConditionDataType, Func<ConditionData, VisualElement>>
            {
                { ConditionDataType.Composite, data => new CompositeConditionView(data, _panel) },
                { ConditionDataType.Boolean, data => new ConditionElementView(data, _panel) },
                { ConditionDataType.Float, data => new ConditionElementView(data, _panel) },
                { ConditionDataType.Integer, data => new ConditionElementView(data, _panel) },
                { ConditionDataType.String, data => new ConditionElementView(data, _panel) }
            };
        }

        public VisualElement CreateView(ConditionData condition)
        {
            if (_viewCreators.TryGetValue(condition.DataType, out var creator))
                return creator(condition);

            return new ConditionElementView(condition, _panel);
        }
    }
}
