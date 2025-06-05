# **Flow Animation System**

## *FSM/Behavior Trees Sprite-Oriented Animation System in Unity*

### **Bugs**
- [ ] Saving transitions doesn't save the right animation state type (looping, one shot)
- [ ] Available Parameters don't save or load on the Parameters panel. 
- [ ] Conditions panel not opening on the right side of the window
### **Testing**

- [ ] Unit Tests!!!
- [ ] Make sure transition panel saves conditions and that they work

### **UI**

- [x] Unity Style Sheet
- [x] Better TransitionEditorPanel UI
- [ ] Better Frame Events GUI
- [ ] Graph View remember view position, if not frame around nodes
- [ ] Breadcrumbs

### **States**

- [ ] Mirrored Animation State
- [ ] Four Directional Animation State
- [ ] Animation overrides - look at sprite resolver for inspiration
- [ ] Ctrl + S and AutoSave toggle
- [ ] Sub Systems

### **Core**

- [ ] Auto-register parameters when using them in code with (SetParameter)
- [ ] More Conditions
- [ ] More Parameter Types Support
- [ ] Make an in-house IAnimator instead of relying on SpriteAnimatorAdapter


### **Documentation**

- [ ] Some general template for making new AnimationFlowController types
- [ ] More documentation on how to use the system

### **MISC.**

* Maybe support for multiple sprite renderers ? Something that'd help limb separation or hats or whatever idk

