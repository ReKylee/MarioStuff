# Animation Flow System

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
