using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class HungerBotSystem : BotSystem
    {
        #region Inspector

        [SerializeField]
        float _FullHungerDuration = 60;

        #endregion

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var hunger = botController.Blackboard.GetValue(BlackboardKeys.Hunger);
            hunger += deltaTime / _FullHungerDuration;
            if (hunger > 1)
            {
                hunger = 1;
            }
            botController.Blackboard.SetValue(BlackboardKeys.Hunger, hunger);
        }
    }
}
