using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class SelectionBotSystem : BotSystem
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _Selection;

        #endregion

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.Blackboard.Subscribe(BlackboardKeys.Selection, OnSelectionChanged);
            OnSelectionChanged();
        }

        void OnSelectionChanged()
        {
            _Selection.gameObject.SetActive(botController.Blackboard.GetValue(BlackboardKeys.Selection));
        }
    }
}
