using System;
using System.Linq;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Core;
using Animation.Flow.Editor.Managers;
using Animation.Flow.Parameters;
using Animation.Flow.Parameters.ConcreteParameters;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;


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
            // Default is already bottom-right
            // Set resize handle to bottom right
            ResizeHandlePos = ResizeHandlePosition.BottomRight;
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

            RefreshParameterList();
        }

        #endregion

        #region Fields

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
        ///     Refreshes the parameter list with the latest parameters from AnimationContextAccessor
        /// </summary>
        public void RefreshParameterList()
        {
            Content.Clear();

            foreach (FlowParameter parameter in ParameterRegistry.GetAllParameterDefinitions())
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
            string typeName = parameter.ParameterType.Name.ToLower();
            typeIcon.AddToClassList(typeName);

            element.Add(typeIcon);

            Label nameLabel = new(parameter.Name);
            nameLabel.AddToClassList("parameter-name");
            element.Add(nameLabel);

            Label typeLabel = new(parameter.ParameterType.Name);
            typeLabel.AddToClassList("parameter-type-label");
            // Add type-specific class
            typeLabel.AddToClassList(typeName);
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
            else if (parameterType == typeof(float))
                return "◈";
            else if (parameterType == typeof(int))
                return "◆";
            else if (parameterType == typeof(string))
                return "◉";
            else
                return "○";
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
                () => AddNewParameter("New Bool", ParameterValueType.Bool, false));

            menu.AddItem(new GUIContent("Float"), false,
                () => AddNewParameter("New Float", ParameterValueType.Float, 0f));

            menu.AddItem(new GUIContent("Integer"), false,
                () => AddNewParameter("New Int", ParameterValueType.Int, 0));

            menu.AddItem(new GUIContent("String"), false,
                () => AddNewParameter("New String", ParameterValueType.String, ""));

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
                ParameterRegistry.UnregisterParameter(parameter.Name);
                RefreshParameterList();
            }
        }

        private void AddNewParameter(string defaultName, ParameterValueType paramType, object defaultValue)
        {
            // Generate a unique name
            string uniqueName = GenerateUniqueName(defaultName);

            // Create the parameter based on its type
            FlowParameter newParameter;

            switch (paramType)
            {
                case ParameterValueType.Bool:
                    newParameter = new BoolParameter(uniqueName, (bool)defaultValue);
                    break;
                case ParameterValueType.Int:
                    newParameter = new IntParameter(uniqueName, (int)defaultValue);
                    break;
                case ParameterValueType.Float:
                    newParameter = new FloatParameter(uniqueName, (float)defaultValue);
                    break;
                case ParameterValueType.String:
                    newParameter = new StringParameter(uniqueName, (string)defaultValue);
                    break;
                default:
                    Debug.LogError($"Unsupported parameter type: {paramType}");
                    return;
            }

            // Add to parameter manager and refresh UI
            ParameterRegistry.RegisterParameter(newParameter);
            RefreshParameterList();

        }

        private string GenerateUniqueName(string baseName)
        {
            // Check if name already exists
            int count = ParameterRegistry.GetAllParameterDefinitions().Count((parameter => parameter.Name == baseName));
            string newName = $"{baseName}_{count + 1}";
            return newName;
        }

       
        #endregion

    }
}
