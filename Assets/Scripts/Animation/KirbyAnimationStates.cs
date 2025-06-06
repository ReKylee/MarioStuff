using UnityEngine;
using Animancer;

namespace Kirby.Core.Components
{
    [CreateAssetMenu(fileName = "KirbyAnimationStates", menuName = "Kirby/Animation States")]
    public class KirbyAnimationStates : ScriptableObject
    {
        [Header("Basic Animations")]
        public ClipTransition idle;
        public ClipTransition walk;
        public ClipTransition run;
        public ClipTransition crouch;
        
        [Header("Jump/Fall Animations")]
        public ClipTransition jumpStart;
        public ClipTransition jump;
        public ClipTransition fall;
        public ClipTransition bounceOffFloor;
        public ClipTransition land;
        
        [Header("Flying Animations")]
        public ClipTransition jumpToFly;
        public ClipTransition fly;
        public ClipTransition flyFlap;
        
        [Header("Full State Animations")]
        public ClipTransition idleFull;
        public ClipTransition walkFull;
        public ClipTransition runFull;
        public ClipTransition jumpStartFull;
        public ClipTransition jumpFull;
        public ClipTransition fallFull;
        
        [Header("Special Actions")]
        public ClipTransition inhale;
        public ClipTransition spit;
        public ClipTransition swallow;
        
        [Header("Slope Variations")]
        public ClipTransition idleSloped;
        public ClipTransition idleDeepSloped;
        public ClipTransition crouchSloped;
        public ClipTransition crouchDeepSloped;
    }
}