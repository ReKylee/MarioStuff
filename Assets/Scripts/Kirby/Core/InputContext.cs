using UnityEngine;

namespace Kirby
{
    public struct InputContext
    {
        public Vector2 MoveInput;

        public bool JumpPressed; // True for the frame jump was pressed
        public bool JumpReleased; // True for the frame jump was released
        public bool JumpHeld; // True if jump is currently held down
        public bool AttackPressed; // True for the frame attack was pressed
        public bool AttackReleased; // True for the frame attack was released
        public bool AttackHeld;


    }
}
