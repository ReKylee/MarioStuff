using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [GeneratePropertyBag]
    [NodeDescription(
        "Check Collision2D BoxCast At Offset On Layer",
        story: "Check [Agent] box cast ( [BoxWidth] x [BoxHeight] ) at [Offset] on layer [TargetLayerName]",
        description:
        "Checks for a 2D collision using a box cast, starting at an offset from the agent, with a specified width, casting down for a specified height, on a specific layer using Physics2D.BoxCast. " +
        "\nIf a collision is found, the collided object is stored in [CollidedObject].",
        category: "Action/Physics",
        id: "955637f9a64b71044e27ef99d301560e")]
    public class CheckCollision2DBoxCastAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> agent;
        [SerializeReference] public BlackboardVariable<float> boxWidth;
        [SerializeReference] public BlackboardVariable<float> boxHeight;

        [SerializeReference] public BlackboardVariable<string> targetLayerName;

        [SerializeReference] public BlackboardVariable<Vector2> offset;

        [Tooltip(
            "[Out Value] Optional: This field is assigned with the collided object, if a collision was found and a variable is linked.")]
        [SerializeReference]
        public BlackboardVariable<GameObject> collidedObject;

        private int _computedLayerMask;
        private string _previousLayerName;

        protected override Status OnStart()
        {
            if (agent == null)
            {
                LogFailure("Agent variable is not assigned.");
                return Status.Failure;
            }

            if (agent.Value is null)
            {
                LogFailure("Agent GameObject (Blackboard Variable Value) is null.");
                return Status.Failure;
            }

            if (boxWidth == null)
            {
                LogFailure("BoxWidth variable is not assigned.");
                return Status.Failure;
            }

            if (boxWidth.Value <= 0f)
            {
                LogFailure("BoxWidth must be greater than 0.");
                return Status.Failure;
            }

            if (boxHeight == null)
            {
                LogFailure("BoxHeight variable is not assigned.");
                return Status.Failure;
            }

            if (boxHeight.Value <= 0f)
            {
                LogFailure("BoxHeight must be greater than 0.");
                return Status.Failure;
            }

            if (targetLayerName == null)
            {
                LogFailure("TargetLayerName variable (string layer name) is not assigned.");
                return Status.Failure;
            }

            if (string.IsNullOrEmpty(targetLayerName.Value))
            {
                LogFailure("TargetLayerName value is null or empty. Please specify a layer name.");
                return Status.Failure;
            }

            if (offset == null)
            {
                LogFailure("Offset variable is not assigned.");
                return Status.Failure;
            }

            if (targetLayerName.Value != _previousLayerName)
            {
                _computedLayerMask = LayerMask.GetMask(targetLayerName.Value);
                _previousLayerName = targetLayerName.Value;
            }

            Status statusToReturn = Status.Failure;
            Color finalDebugColor = Color.red; // Default to red (no hit / general failure

            if (collidedObject != null) // Only default to null if the variable is actually linked
            {
                collidedObject.Value = null;
            }

            if (_computedLayerMask == 0)
            {
                if (!targetLayerName.Value.Equals("Nothing", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(targetLayerName.Value))
                {
                    LogFailure(
                        $"Layer name(s) \"{targetLayerName.Value}\" did not match any existing layers. Collision check will fail.");
                }

                finalDebugColor = Color.magenta;
            }
            else
            {
                Vector2 boxTopCenter = (Vector2)agent.Value.transform.position + offset.Value;
                Vector2 castBoxSize = new(boxWidth.Value, 0.01f);
                float castDistance = boxHeight.Value;

                RaycastHit2D hit = Physics2D.BoxCast(
                    boxTopCenter,
                    castBoxSize,
                    0f,
                    Vector2.down,
                    castDistance,
                    _computedLayerMask
                );

                if (hit.collider is not null)
                {
                    if (hit.collider.gameObject == agent.Value)
                    {
                        finalDebugColor = Color.yellow; // Hit self
                    }
                    else
                    {
                        finalDebugColor = Color.green; // Valid hit
                        if (collidedObject != null) // Only assign if the variable is linked
                        {
                            collidedObject.Value = hit.collider.gameObject;
                        }

                        statusToReturn = Status.Success;
                    }
                }
            }

#if UNITY_EDITOR
            // Ensure agent, offset, boxWidth, and boxHeight are valid for drawing
            // These checks are implicitly covered by the initial null/value checks that would lead to an early return.
            Vector2 vizBoxTopCenter = (Vector2)agent.Value.transform.position + offset.Value;
            float vizHalfWidth = boxWidth.Value / 2f;
            float vizCastDistance = boxHeight.Value;
            Vector2 vizTopLeft = vizBoxTopCenter + new Vector2(-vizHalfWidth, 0);
            Vector2 vizTopRight = vizBoxTopCenter + new Vector2(vizHalfWidth, 0);
            Vector2 vizBottomLeft = vizTopLeft + Vector2.down * vizCastDistance;
            Vector2 vizBottomRight = vizTopRight + Vector2.down * vizCastDistance;

            Debug.DrawLine(vizTopLeft, vizBottomLeft, finalDebugColor);
            Debug.DrawLine(vizTopRight, vizBottomRight, finalDebugColor);
#endif

            return statusToReturn;
        }
    }
}
