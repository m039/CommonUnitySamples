using UnityEngine;

namespace Game.GOAPSample
{
    public class MoveBotSystem : CoreBotSystem
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _SpriteRenderer;

        #endregion

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.Blackboard.Subscribe(BlackboardKeys.IsFacingLeft, UpdateFacing);
            UpdateFacing();
        }

        void UpdateFacing()
        {
            _SpriteRenderer.flipX = botController.Blackboard.GetValue(BlackboardKeys.IsFacingLeft);
        }
    }
}
