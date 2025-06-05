# Animation Flow System
# Animation Flow System
# Animation Flow System
# Flow Animation System

## Overview

Flow is a powerful, data-driven animation system for Unity that uses behavior trees to control sprite animations. It provides a flexible framework for creating complex animation state machines with visual editing tools.

## Core Features

- **Behavior Tree Architecture**: Uses a behavior tree system to control animation logic
- **Data-Driven Design**: All animation flows are defined in ScriptableObjects
- **Visual Editor**: Custom editor for creating and editing behavior trees
- **Sprite Animation Support**: Integrates with SpriteAnimator for frame-by-frame animations
- **Parameter System**: Flexible parameter system for controlling animation transitions
- **Extensible Framework**: Easy to extend with custom nodes and adapters

## Main Components

### Core

- **AnimationFlowAsset**: ScriptableObject that contains the behavior tree definition
- **FlowNode**: Base class for all nodes in the behavior tree
- **AnimationContext**: Contains parameters and animator reference
- **AnimationFlowController**: Base controller that executes the behavior tree

### Nodes

#### Composite Nodes
- **Selector**: Executes children until one succeeds
- **Sequence**: Executes children in order until one fails
- **Parallel**: Executes all children simultaneously

#### Decorator Nodes
- **Inverter**: Inverts the result of its child
- **Repeater**: Repeats execution of its child

#### Leaf Nodes
- **PlayAnimation**: Plays a specific animation
- **CheckParameter**: Checks a parameter against a condition
- **Wait**: Waits for a specified amount of time

### Adapters

- **SpriteAnimatorAdapter**: Adapts SpriteAnimator to the IAnimator interface

## Getting Started

### Creating an Animation Flow

1. Open the Animation Flow Editor (Window > Animation > Flow Editor)
2. Create a new Animation Flow asset
3. Add nodes to the graph and connect them
4. Save the asset

### Using Animation Flow in Your Game

1. Add a SpriteAnimationController component to your GameObject
2. Assign the Animation Flow asset to the controller
3. Set up parameters in your code

```csharp
// Example of setting parameters
var controller = GetComponent<SpriteAnimationController>();
controller.SetParameter("IsGrounded", true);
controller.SetParameter("Speed", 5.0f);
controller.SetParameter("State", "Running");
```

## Creating Custom Nodes

You can extend the system by creating custom nodes:

```csharp
[CreateAssetMenu(fileName = "New Custom Node", menuName = "Animation/Flow/Nodes/Custom/MyCustomNode")]
public class MyCustomNode : FlowNode
{
    [SerializeField] private string _myParameter;

    public override NodeStatus Execute(AnimationContext context)
    {
        // Custom logic here
        return NodeStatus.Success;
    }
}
```

## Integration with Other Systems

The Flow animation system can be integrated with other systems by:

1. Creating custom adapters for different animation systems
2. Extending the controller to handle specific game logic
3. Creating custom nodes for game-specific behavior

## Example Usage

```csharp
// Create a character controller that uses the animation flow system
public class PlayerAnimationController : SpriteAnimationController
{
    private PlayerMovement _movement;

    protected override void Awake()
    {
        base.Awake();
        _movement = GetComponent<PlayerMovement>();
    }

    protected override void Update()
    {
        // Update parameters based on player state
        SetParameter("IsGrounded", _movement.IsGrounded);
        SetParameter("Speed", Mathf.Abs(_movement.Velocity.x));
        SetParameter("IsJumping", _movement.IsJumping);

        // Let the base controller handle the animation flow
        base.Update();
    }
}
```

## Best Practices

- Create reusable animation flow assets for common behaviors
- Use clear naming conventions for parameters
- Split complex animation logic into multiple sub-trees
- Use the editor to visualize and debug your animation flows
## Overview

The Animation Flow system is a behavior tree-based animation controller for Unity. It provides a data-driven approach to creating complex animation logic without writing code. The system is designed to be:

- **Modular**: Each component has a single responsibility
- **Robust**: Built-in validation and error handling
- **Extensible**: Easy to add new node types and conditions
- **Data-Driven**: All animation logic is stored in scriptable objects

## Core Components

### Behavior Tree

The behavior tree structure consists of these node types:

- **Composite Nodes**: Nodes with multiple children
  - **Sequence**: Executes children in order until one fails
  - **Selector**: Executes children in order until one succeeds
  - **Parallel**: Executes all children simultaneously

- **Decorator Nodes**: Nodes that modify a single child
  - **Inverter**: Inverts the result of its child
  - **Repeat**: Repeats execution of its child
  - **Timeout**: Limits execution time of its child

- **Leaf Nodes**: Nodes that perform actions
  - **AnimationAction**: Plays an animation
  - **Condition**: Evaluates a condition
  - **Wait**: Waits for a specified time

### Conditions

Conditions evaluate parameters in the animation context:

- **Parameter Conditions**:
  - **BoolCondition**: Checks boolean parameters
  - **IntCondition**: Compares integer parameters
  - **FloatCondition**: Compares float parameters
  - **StringCondition**: Compares string parameters

- **CompositeCondition**: Combines multiple conditions with AND/OR logic

### Parameters

Parameters store data that can be used by conditions:

- **BoolParameter**: Boolean values
- **IntParameter**: Integer values
- **FloatParameter**: Float values
- **StringParameter**: String values

## How to Use

1. Create an AnimationFlowAsset scriptable object
2. Build a behavior tree by adding nodes
3. Add parameters that will control animation flow
4. Attach the asset to an AnimationFlowController component
5. Configure the controller with an animator implementation

## Examples

### Simple Player Animation

```
- Selector (Root)
  |- Sequence (Movement)
  |  |- Condition (IsMoving == true)
  |  |- AnimationAction (Play "Walk")
  |
  |- Sequence (Idle)
     |- AnimationAction (Play "Idle")
```

### Attack Combo

```
- Sequence (AttackCombo)
  |- Condition (AttackPressed == true)
  |- AnimationAction (Play "Attack1")
  |- Selector (ContinueCombo)
     |- Sequence (SecondAttack)
     |  |- Condition (AttackPressed == true)
     |  |- AnimationAction (Play "Attack2")
     |
     |- AnimationAction (Play "ReturnToIdle")
```

## Best Practices

1. Keep behavior trees focused on a single aspect of animation control
2. Use meaningful names for nodes and parameters
3. Validate your trees regularly during development
4. Use the debug visualization to troubleshoot issues
5. Keep parameter names consistent across different assets
## Parameter System

The Animation Flow System uses a strongly-typed parameter system to store and manage animation state parameters. These parameters can be used in conditions to control state transitions.

### Key Features

- **Serializable Parameters**: All parameters are serializable and stored in the AnimationFlowAsset.
- **Strong Typing**: Parameters are strongly typed using generics (bool, int, float, string).
- **Automatic Registration**: Parameters used in code via `SetParameter()` are automatically added to the asset.
- **Editor Support**: Parameters can be viewed and edited in the ParameterPanel.

### Parameter Types

- `BoolParameter`: Boolean values (true/false)
- `IntParameter`: Integer values with optional min/max constraints
- `FloatParameter`: Float values with optional min/max constraints
- `StringParameter`: String values with optional max length

## Condition System

Conditions determine when transitions between animation states should occur.

### Condition Types

- **Parameter Conditions**: Check parameter values
  - `BoolCondition`: Check boolean parameters
  - `IntCondition`: Compare integer parameters with various comparison types
  - `FloatCondition`: Compare float parameters with various comparison types
  - `StringCondition`: Compare string parameters with various comparison types

- **Special Conditions**: Self-managed conditions
  - `AnimationCompleteCondition`: Triggers when the current animation completes
  - `TimeCondition`: Triggers based on time spent in the current state

- **Logical Conditions**:
  - `CompositeCondition`: Combines multiple conditions with AND/OR logic

### How to Use

#### Setting Parameters

```csharp
// In your game code
flowController.SetParameter("IsGrounded", true);
flowController.SetParameter("Speed", 5.0f);
flowController.SetParameter("JumpCount", 2);
flowController.SetParameter("PlayerState", "Jumping");
```

#### Creating Conditions in Code

```csharp
// Create a transition with conditions
var transition = state.TransitionTo("JumpState");

// Add a bool condition
transition.AddCondition(ConditionFactory.CreateBoolCondition("IsGrounded", false));

// Add a composite condition with multiple checks
var composite = ConditionFactory.CreateAndCondition();
composite.AddCondition(ConditionFactory.CreateFloatCondition("Speed", 3.0f, ComparisonType.GreaterOrEqual));
composite.AddCondition(ConditionFactory.CreateBoolCondition("CanJump", true));
transition.AddCondition(composite);

// Add a time-based condition
transition.AddCondition(ConditionFactory.CreateTimeCondition(0.5f));
```

## Architecture

The system follows SOLID principles:

- **Single Responsibility**: Each class has a single responsibility
- **Open/Closed**: The system can be extended with new parameter and condition types
- **Liskov Substitution**: All parameters and conditions follow their base interfaces
- **Interface Segregation**: Clear interfaces define behavior boundaries
- **Dependency Inversion**: Higher-level modules depend on abstractions

## Extending the System

### Adding a New Parameter Type

1. Create a new class inheriting from `FlowParameter<T>`
2. Add any specialized properties or validation
3. Register factory methods in `ParameterRegistry` if needed

### Adding a New Condition Type

1. Create a new class inheriting from `FlowCondition`
2. Implement the `EvaluateInternal` method
3. Add factory methods in `ConditionFactory` if needed

## Integration with Editor

The parameter system is designed to work with the ParameterPanel editor script:

- Parameters gathered from usage in code are available in the panel
- New parameters can be created directly in the panel
- Parameters can be visualized and edited during runtime

## Best Practices

- Use descriptive parameter names
- Keep parameter types simple and appropriate for their use
- Use composite conditions to create complex logic
- Prefer special conditions (AnimationComplete, Time) over manual parameter tracking
A data-driven animation state machine system for Unity, designed with SOLID principles in mind. This system provides a flexible and extensible way to create and manage animation transitions with complex conditions.

## Core Features

- **State Machine Architecture**: Define animation states and transitions between them
- **Flexible Condition System**: Create complex condition logic with AND/OR grouping
- **Data-Driven Design**: Use ScriptableObjects to configure and save animation flows
- **Runtime Extension**: Register custom condition types through the API
- **Animator Abstraction**: Works with any animation system through the IAnimator interface

## Usage Example

```csharp
// Create a transition from one state to another
AnimationTransition transition = sourceState.TransitionTo(targetState.Id);

// Add a simple condition
transition.AddCondition(new BoolCondition("IsJumping", true));

// Add a time-based condition
transition.AddCondition(new TimeElapsedCondition(1.5f));

// Create a complex condition group
CompositeCondition orGroup = transition.AddOrGroup();
orGroup.AddCondition(new FloatCondition("Speed", FloatComparisonType.GreaterThan, 5f));
orGroup.AddCondition(new AnimationCompleteCondition());
```

## Extending with Custom Conditions

To create a custom condition type:

1. Implement the `ICondition` interface
2. Register your condition factory with `AnimationConditionRegistry`
3. Use your custom condition in transitions

```csharp
// Register a custom condition type
AnimationConditionRegistry.RegisterConditionType("MyCustomCondition", 
    data => new MyCustomCondition(data.Parameters["customParam"]));
```

## Best Practices

- Create reusable animation flows as ScriptableObjects
- Organize conditions logically with composite groups
- Use meaningful parameter names for better readability
- Register custom conditions in a central place (like a startup manager)
