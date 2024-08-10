using UnityEngine;

namespace Game.GOAPSample
{
    public class MoveBotFeature : CoreBotFeature
    {
        #region Inspector

        [SerializeField]
        float _MoveSpeed = 1f;

        [SerializeField]
        SpriteRenderer _SpriteRenderer;

        #endregion

        protected override void OnInit()
        {
            botController.Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
            botController.Blackboard.Subscribe(BlackboardKeys.IsFacingLeft, UpdateFacing);
            UpdateFacing();
        }

        protected override void OnDeinit()
        {
            botController.Blackboard.Unsubscribe(BlackboardKeys.IsFacingLeft, UpdateFacing);
        }

        void UpdateFacing()
        {
            _SpriteRenderer.flipX = botController.Blackboard.GetValue(BlackboardKeys.IsFacingLeft);
        }
    }
}
