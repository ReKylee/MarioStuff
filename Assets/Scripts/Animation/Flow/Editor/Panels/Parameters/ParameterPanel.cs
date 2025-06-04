using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Animation.Flow.Editor.Panels.Parameters
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
            // Add class for specific styling
            AddToClassList("parameter-panel");

            // Load the parameter panel stylesheet
            StyleSheet parameterPanelStylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/Animation/Flow/Editor/ParameterPanelStyles.uss");

            if (parameterPanelStylesheet)
            {
                styleSheets.Add(parameterPanelStylesheet);
            }

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
            scrollView.name = "ParameterScrollView";
            scrollView.style.flexGrow = 1;
            scrollView.style.width = new StyleLength(StyleKeyword.Auto);
            scrollView.contentContainer.style.width = new StyleLength(StyleKeyword.Auto);

            // Apply explicit background color programmatically
            scrollView.style.backgroundColor = new Color(0.176f, 0.176f, 0.176f, 1f);

            return scrollView;
        }

        protected override void OnContentCreated(ScrollView content)
        {
            ContentContainer.Add(content);
            ContentContainer.style.flexGrow = 1;
            ContentContainer.style.width = new StyleLength(StyleKeyword.Auto);
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
            Content.Clear();

            foreach (ParameterData parameter in _parameters)
            {
                VisualElement element = CreateParameterElement(parameter);
                Content.Add(element);
            }
        }

        private VisualElement CreateParameterElement(ParameterData parameter)
        {
            VisualElement element = new();
            element.AddToClassList("parameter-item");
            element.userData = parameter;

            // Create type icon with appropriate type-specific class
            Label typeIcon = new(GetTypeIcon(parameter.Type));
            typeIcon.AddToClassList("parameter-type-icon");

            // Add type-specific class
            string typeName = parameter.Type.ToString().ToLower();
            typeIcon.AddToClassList(typeName);

            element.Add(typeIcon);

            Label nameLabel = new(parameter.Name);
            nameLabel.AddToClassList("parameter-name");
            element.Add(nameLabel);

            Label typeLabel = new(parameter.Type.ToString());
            typeLabel.AddToClassList("parameter-type-label");
            // Add type-specific class
            typeLabel.AddToClassList(typeName);
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
