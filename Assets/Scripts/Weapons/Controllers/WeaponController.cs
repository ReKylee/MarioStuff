using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons.Interfaces;

namespace Weapons.Controllers
{
    /// <summary>
    ///     Controller for managing multiple weapons following the MVC pattern
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private List<WeaponMapping> weaponMappings = new();

        // Input system reference
        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            // Initialize input actions
            _inputActions = new InputSystem_Actions();

            // Initialize each weapon mapping
            foreach (WeaponMapping mapping in weaponMappings)
            {
                // Initialize the mapping with our input actions
                mapping.Initialize(_inputActions);
            }
        }


        private void OnEnable()
        {
            // Subscribe all weapon mappings to their actions
            foreach (WeaponMapping mapping in weaponMappings)
            {
                mapping.Subscribe();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe all weapon mappings from their actions
            foreach (WeaponMapping mapping in weaponMappings)
            {
                mapping.Unsubscribe();
            }
        }


        [Serializable]
        public class WeaponMapping
        {
            [Header("Weapon Info")] public string weaponName;

            public MonoBehaviour weaponComponent;

            [Header("Input Configuration")]
            [Tooltip("The action name from the Player action map (case-sensitive)")]
            [SerializeField]
            private string actionName;

            // Reference to the resolved action
            private InputAction _action;

            // Property for easy access to the weapon component as IWeapon
            public IWeapon WeaponComponent => weaponComponent as IWeapon;

            // Initialize with the input actions instance
            public void Initialize(InputSystem_Actions inputActions)
            {
                // Get the action directly by property name using reflection
                Type playerActionsType = typeof(InputSystem_Actions.PlayerActions);
                PropertyInfo propertyInfo = playerActionsType.GetProperty(actionName);

                if (propertyInfo != null)
                {
                    _action = propertyInfo.GetValue(inputActions.Player) as InputAction;
                    if (_action == null)
                    {
                        Debug.LogError($"Action '{actionName}' found but could not be cast to InputAction type.");
                    }
                }
                else
                {
                    // Try to get the action directly by name as a fallback
                    _action = inputActions.Player.FindAction($"Player/{actionName}", false);

                    if (_action == null)
                    {
                        Debug.LogWarning($"Action '{actionName}' not found in Player action map.");
                    }
                }
            }

            // Subscribe to the action events
            public void Subscribe()
            {
                if (_action != null)
                {
                    _action.performed += OnActionPerformed;
                }
            }

            // Unsubscribe from the action events
            public void Unsubscribe()
            {
                if (_action != null)
                {
                    _action.performed -= OnActionPerformed;
                }
            }

            // Handle the action event
            private void OnActionPerformed(InputAction.CallbackContext context)
            {
                if (WeaponComponent != null)
                {
                    WeaponComponent.Shoot();
                }
            }
        }
    }
}
