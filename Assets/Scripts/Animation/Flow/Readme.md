# Animation Flow System
# Animation Flow System

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
