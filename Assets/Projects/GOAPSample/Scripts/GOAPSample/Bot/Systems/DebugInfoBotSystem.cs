using Game.BehaviourTreeSample;
using System.Text;
using UnityEngine;

namespace Game.GOAPSample
{
    public class DebugInfoBotSystem : BotSystem
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _Text;

        #endregion

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            if (botController.Blackboard.ContainsKey(BlackboardKeys.IsInvisible))
            {
                _Text.gameObject.SetActive(false);
            } else
            {
                _Text.gameObject.SetActive(true);

                var sb = new StringBuilder();

                if (botController.Blackboard.ContainsKey(BlackboardKeys.HasFood))
                {
                    sb.AppendLine("+Food");
                }

                if (botController.Blackboard.ContainsKey(BlackboardKeys.HasWood))
                {
                    sb.AppendLine("+Wood");
                }

                if (botController.Blackboard.TryGetValue(BlackboardKeys.Hunger, out var hunger))
                {
                    sb.AppendLine($"Hunger: {hunger.ToString("0.0")}");
                }

                _Text.text = sb.ToString();
            }
        }
    }
}
