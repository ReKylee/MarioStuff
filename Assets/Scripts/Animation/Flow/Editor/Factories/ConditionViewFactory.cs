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
        private readonly Dictionary<ConditionType, Func<ConditionData, VisualElement>> _viewCreators;

        public ConditionViewFactory(ConditionListPanel panel)
        {
            _panel = panel;
            _viewCreators = new Dictionary<ConditionType, Func<ConditionData, VisualElement>>
            {
                { ConditionType.Composite, data => new CompositeConditionView(data, _panel) },
                { ConditionType.ParameterComparison, data => new ConditionElementView(data, _panel) },
            };
        }

        public VisualElement CreateView(ConditionData condition)
        {
            if (_viewCreators.TryGetValue(condition.Type, out var creator))
                return creator(condition);

            return new ConditionElementView(condition, _panel);
        }
    }
}
