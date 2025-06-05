using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Parameters;
using Animation.Flow.Parameters.ConcreteParameters;
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

        public ParameterPanel(VisualElement parentContainer, List<FlowParameter> parameters = null)
            : base(parentContainer, "Parameters", new Vector2(20, 100))
        {
            // Set resize handle to bottom right
            ResizeHandlePos = ResizeHandlePosition.BottomRight;

            // Add class for specific styling
            AddToClassList("parameter-panel");

            // Load the parameter panel stylesheet
            StyleSheet parameterPanelStylesheet = Resources.Load<StyleSheet>("Stylesheets/ParameterPanelStyles");

            if (parameterPanelStylesheet)
            {
                styleSheets.Add(parameterPanelStylesheet);
            }

            _parameters = parameters ?? new List<FlowParameter>();
            RefreshParameterList();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Update the parameters list and refresh the UI
        /// </summary>
        public void UpdateParameters(List<FlowParameter> parameters)
        {
            _parameters = parameters ?? new List<FlowParameter>();
            RefreshParameterList();
        }

        #endregion

        #region Fields

        private List<FlowParameter> _parameters;
        public event Action<FlowParameter> OnParameterDragStart;

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
            // Add 'Add Parameter' button at the top
            Button addParameterButton = new(ShowAddParameterMenu)
            {
                text = "+ Add Parameter"
            };

            addParameterButton.AddToClassList("add-parameter-button");

            // Create a header container to hold the button
            VisualElement headerContainer = new();
            headerContainer.AddToClassList("parameter-panel-header");
            headerContainer.style.width = new StyleLength(StyleKeyword.Auto);
            headerContainer.style.flexGrow = 1;
            headerContainer.Add(addParameterButton);

            ContentContainer.Add(headerContainer);
            ContentContainer.Add(content);
            ContentContainer.style.flexGrow = 1;
            ContentContainer.style.width = new StyleLength(StyleKeyword.Auto);
        }

        #endregion

        #region Parameter Management

        /// <summary>
        ///     Refreshes the parameter list with the current parameters
        /// </summary>
        public void RefreshParameterList()
        {
            Content.Clear();

            if (_parameters == null) return;

            foreach (FlowParameter parameter in _parameters)
            {
                VisualElement element = CreateParameterElement(parameter);
                Content.Add(element);
            }
        }

        private VisualElement CreateParameterElement(FlowParameter parameter)
        {
            VisualElement element = new();
            element.AddToClassList("parameter-item");
            element.userData = parameter;

            // Create type icon with appropriate type-specific class
            Label typeIcon = new(GetTypeIcon(parameter.ParameterType));
            typeIcon.AddToClassList("parameter-type-icon");

            // Add type-specific class
            string typeName = GetTypeDisplayName(parameter.ParameterType);
            typeIcon.AddToClassList(typeName.ToLower());

            element.Add(typeIcon);

            Label nameLabel = new(parameter.Name);
            nameLabel.AddToClassList("parameter-name");
            element.Add(nameLabel);

            Label typeLabel = new(typeName);
            typeLabel.AddToClassList("parameter-type-label");
            typeLabel.AddToClassList(typeName.ToLower());
            element.Add(typeLabel);

            // Make draggable
            element.RegisterCallback<MouseDownEvent>(evt => OnParameterMouseDown(evt, parameter));

            // Add context menu for right-click options
            element.RegisterCallback<ContextClickEvent>(evt => ShowParameterContextMenu(evt, parameter));

            return element;
        }

        private string GetTypeIcon(Type parameterType)
        {
            if (parameterType == typeof(bool))
                return "☑";

            if (parameterType == typeof(float))
                return "◈";

            if (parameterType == typeof(int))
                return "◆";

            if (parameterType == typeof(string))
                return "◉";

            return "○";
        }

        private string GetTypeDisplayName(Type parameterType)
        {
            if (parameterType == typeof(bool))
                return "Bool";

            if (parameterType == typeof(float))
                return "Float";

            if (parameterType == typeof(int))
                return "Int";

            if (parameterType == typeof(string))
                return "String";

            return parameterType.Name;
        }

        private void OnParameterMouseDown(MouseDownEvent evt, FlowParameter parameter)
        {
            if (evt.button == 0)
            {
                OnParameterDragStart?.Invoke(parameter);
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[0];
                DragAndDrop.SetGenericData("FlowParameter", parameter);
                DragAndDrop.StartDrag(parameter.Name);
                evt.StopPropagation();
            }
        }

        private void ShowAddParameterMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Boolean"), false,
                () => AddNewParameter("New Bool", typeof(bool), false));

            menu.AddItem(new GUIContent("Float"), false,
                () => AddNewParameter("New Float", typeof(float), 0f));

            menu.AddItem(new GUIContent("Integer"), false,
                () => AddNewParameter("New Int", typeof(int), 0));

            menu.AddItem(new GUIContent("String"), false,
                () => AddNewParameter("New String", typeof(string), ""));

            menu.ShowAsContext();
        }

        private void ShowParameterContextMenu(ContextClickEvent evt, FlowParameter parameter)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Delete"), false, () => DeleteParameter(parameter));

            menu.ShowAsContext();
            evt.StopPropagation();
        }

        private void DeleteParameter(FlowParameter parameter)
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                    $"Are you sure you want to delete the parameter '{parameter.Name}'?", "Delete", "Cancel"))
            {
                _parameters?.Remove(parameter);
                RefreshParameterList();
            }
        }

        private void AddNewParameter(string defaultName, Type paramType, object defaultValue)
        {
            // Generate a unique name
            string uniqueName = GenerateUniqueName(defaultName);

            // Create the parameter based on its type
            FlowParameter newParameter = CreateParameterByType(uniqueName, paramType, defaultValue);

            if (newParameter != null)
            {
                _parameters ??= new List<FlowParameter>();
                _parameters.Add(newParameter);
                RefreshParameterList();
            }
        }

        private FlowParameter CreateParameterByType(string paramName, Type paramType, object defaultValue)
        {
            try
            {
                if (paramType == typeof(bool))
                    return new BoolParameter(paramName, (bool)defaultValue);

                if (paramType == typeof(int))
                    return new IntParameter(paramName, (int)defaultValue);

                if (paramType == typeof(float))
                    return new FloatParameter(paramName, (float)defaultValue);

                if (paramType == typeof(string))
                    return new StringParameter(paramName, (string)defaultValue);

                Debug.LogError($"Unsupported parameter type: {paramType}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create parameter of type {paramType}: {ex.Message}");
                return null;
            }
        }

        private string GenerateUniqueName(string baseName)
        {
            if (_parameters == null || _parameters.Count == 0)
                return baseName;

            // Check if name already exists
            var existingNames = _parameters.Where(p => p.Name.StartsWith(baseName)).Select(p => p.Name).ToList();

            if (!existingNames.Contains(baseName))
                return baseName;

            // Find the next available number
            int counter = 1;
            string newName;
            do
            {
                newName = $"{baseName}_{counter}";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }

        #endregion

    }
}
