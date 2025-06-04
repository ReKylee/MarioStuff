using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Panel showing available parameters that can be dragged
    /// </summary>
    public class ParameterPanel : DraggablePanel<ScrollView>
    {

        #region Constructor

        public ParameterPanel(VisualElement parentContainer)
            : base(parentContainer, "Parameters", new Vector2(20, 100))
        {
            LoadDefaultParameters();
            RefreshParameterList();
        }

        #endregion

        #region Fields

        private readonly List<ParameterData> _parameters = new();
        public event Action<ParameterData> OnParameterDragStart;

        #endregion

        #region Overrides

        protected override ScrollView CreateContent()
        {
            ScrollView scrollView = new();
            scrollView.AddToClassList("parameter-scroll-view");
            return scrollView;
        }

        protected override void OnContentCreated(ScrollView content)
        {
            _contentContainer.Add(content);
        }

        #endregion

        #region Parameter Management

        private void LoadDefaultParameters()
        {
            _parameters.Clear();
            _parameters.Add(new ParameterData
                { Name = "IsGrounded", Type = ConditionDataType.Boolean, DefaultValue = false });

            _parameters.Add(new ParameterData { Name = "Speed", Type = ConditionDataType.Float, DefaultValue = 0f });
            _parameters.Add(new ParameterData
                { Name = "Health", Type = ConditionDataType.Integer, DefaultValue = 100 });

            _parameters.Add(
                new ParameterData { Name = "State", Type = ConditionDataType.String, DefaultValue = "Idle" });
        }

        private void RefreshParameterList()
        {
            _content.Clear();

            foreach (ParameterData parameter in _parameters)
            {
                VisualElement element = CreateParameterElement(parameter);
                _content.Add(element);
            }
        }

        private VisualElement CreateParameterElement(ParameterData parameter)
        {
            VisualElement element = new();
            element.AddToClassList("parameter-item");
            element.userData = parameter;

            Label typeIcon = new(GetTypeIcon(parameter.Type));
            typeIcon.AddToClassList("parameter-type-icon");
            element.Add(typeIcon);

            Label nameLabel = new(parameter.Name);
            nameLabel.AddToClassList("parameter-name");
            element.Add(nameLabel);

            Label typeLabel = new(parameter.Type.ToString());
            typeLabel.AddToClassList("parameter-type-label");
            element.Add(typeLabel);

            // Make draggable
            element.RegisterCallback<MouseDownEvent>(evt => OnParameterMouseDown(evt, parameter));

            return element;
        }

        private string GetTypeIcon(ConditionDataType type)
        {
            return type switch
            {
                ConditionDataType.Boolean => "☑",
                ConditionDataType.Float => "◈",
                ConditionDataType.Integer => "◆",
                ConditionDataType.String => "◉",
                _ => "○"
            };
        }

        private void OnParameterMouseDown(MouseDownEvent evt, ParameterData parameter)
        {
            if (evt.button == 0)
            {
                OnParameterDragStart?.Invoke(parameter);
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[0];
                DragAndDrop.SetGenericData("ParameterData", parameter);
                DragAndDrop.StartDrag(parameter.Name);
                evt.StopPropagation();
            }
        }

        #endregion

    }
}
