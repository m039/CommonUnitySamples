using UnityEngine;

namespace Game.StateMachineSample
{
    public class MoveRandomBotState : MoveBotState
    {
        Vector2 _target;

        public override Vector2? Target => _noStartPosition? null : _target;

        bool _noStartPosition;

        public override void OnEnter()
        {
            base.OnEnter();

            _target = CameraUtils.RandomPositionOnScreen();
        }
    }
}
