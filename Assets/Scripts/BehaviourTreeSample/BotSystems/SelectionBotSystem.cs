using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class SelectionBotSystem : BotSystem
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _Selection;

        #endregion

        void Update()
        {
            _Selection.gameObject.SetActive(botController.Blackboard.GetValue(BlackboardKeys.Selection));
        }
    }
}
