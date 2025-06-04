using System;
using Animation.Flow.Conditions;
using Animation.Flow.Editor.Managers;
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
            _resizeHandlePosition = ResizeHandlePosition.BottomRight;
            // Set resize handle to bottom right
            _resizeHandlePosition = ResizeHandlePosition.BottomRight;
            // Add class for specific styling
            AddToClassList("parameter-panel");

            // Load the parameter panel stylesheet
            StyleSheet parameterPanelStylesheet = Resources.Load<StyleSheet>("Stylesheets/ParameterPanelStyles");

            if (parameterPanelStylesheet)
            {
                styleSheets.Add(parameterPanelStylesheet);
            }

            LoadDefaultParameters();
            RefreshParameterList();
        }

        #endregion

        #region Fields

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

        private void LoadDefaultParameters()
        {
            // Parameters are now loaded from AnimationContextAccessor, which has its own default parameter set
            AnimationContextAccessor.Instance.Parameters.ForEach(p =>
            {
                /* Just access to initialize */
            });
        }

        private void RefreshParameterList()
        {
            Content.Clear();

            foreach (ParameterData parameter in AnimationContextAccessor.Instance.Parameters)
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

            // Add context menu for right-click options
            element.RegisterCallback<ContextClickEvent>(evt => ShowParameterContextMenu(evt, parameter));

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

        private void ShowAddParameterMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Boolean"), false,
                () => AddNewParameter("New Bool", ConditionDataType.Boolean, false));

            menu.AddItem(new GUIContent("Float"), false,
                () => AddNewParameter("New Float", ConditionDataType.Float, 0f));

            menu.AddItem(new GUIContent("Integer"), false,
                () => AddNewParameter("New Int", ConditionDataType.Integer, 0));

            menu.AddItem(new GUIContent("String"), false,
                () => AddNewParameter("New String", ConditionDataType.String, ""));

            menu.ShowAsContext();
        }

        private void ShowParameterContextMenu(ContextClickEvent evt, ParameterData parameter)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Rename"), false, () => ShowRenameDialog(parameter));
            menu.AddItem(new GUIContent("Delete"), false, () => DeleteParameter(parameter));

            menu.ShowAsContext();
            evt.StopPropagation();
        }

        private void DeleteParameter(ParameterData parameter)
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                    $"Are you sure you want to delete the parameter '{parameter.Name}'?", "Delete", "Cancel"))
            {
                AnimationContextAccessor.Instance.RemoveParameter(parameter.Name);
                RefreshParameterList();
            }
        }

        private void AddNewParameter(string defaultName, ConditionDataType type, object defaultValue)
        {
            // Generate a unique name
            string name = GenerateUniqueName(defaultName);

            // Create the parameter
            ParameterData newParameter = new()
            {
                Name = name,
                Type = type,
                DefaultValue = defaultValue
            };

            // Add to parameter manager and refresh UI
            AnimationContextAccessor.Instance.AddParameter(newParameter);
            RefreshParameterList();

            // Show rename dialog after short delay to allow UI to update
            EditorApplication.delayCall += () => ShowRenameDialog(newParameter);
        }

        private string GenerateUniqueName(string baseName)
        {
            string name = baseName;
            int counter = 1;

            // Check if name already exists
            while (AnimationContextAccessor.Instance.Parameters.Exists(p => p.Name == name))
            {
                name = $"{baseName}_{counter}";
                counter++;
            }

            return name;
        }

        private void ShowRenameDialog(ParameterData parameter)
        {
            // Position near the window center
            Vector2 position = EditorWindow.GetWindow<EditorWindow>().position.center;

            // Create and show rename popup
            PopupWindow.Show(
                new Rect(position.x - 100, position.y - 50, 0, 0),
                new ParameterRenamePopup(parameter.Name, newName =>
                {
                    // Validate name is unique
                    if (string.IsNullOrWhiteSpace(newName) ||
                        AnimationContextAccessor.Instance.Parameters.Exists(p => p != parameter && p.Name == newName))
                    {
                        EditorUtility.DisplayDialog("Invalid Name", "Parameter name must be unique and not empty.",
                            "OK");

                        return false;
                    }

                    // Update name and refresh
                    string oldName = parameter.Name;
                    parameter.Name = newName;
                    AnimationContextAccessor.Instance.UpdateParameter(parameter);
                    RefreshParameterList();
                    return true;
                })
            );
        }

        #endregion

    }
}
