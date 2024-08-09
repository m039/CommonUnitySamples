using Game.BehaviourTreeSample;
using m039.Common.Blackboard;
using UnityEngine;

namespace Game
{
    public class HungerBotSystem : BotSystem, IFoodEater
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _FoodCounter;

        [SerializeField]
        float _BlinkDuration = 0.3f;

        [SerializeField]
        Color _BlinkColor = Color.white;

        #endregion

        public void Eat(IGameEntity food)
        {
            int foodEaten;
            if (!botController.Blackboard.TryGetValue(BlackboardKeys.EatenFood, out foodEaten))
            {
                foodEaten = 0;
            }

            foodEaten++;
            botController.Blackboard.SetValue(BlackboardKeys.EatenFood, foodEaten);
            _FoodCounter.text = foodEaten.ToString();

            botController.EventBus.Raise<IBlinkEvent>(a => a.Blink(_BlinkColor, _BlinkDuration));

            if (botController.Blackboard.TryGetValue(BlackboardKeys.GroupBlackboard, out BlackboardBase groupBlackboard))
            {
                if (groupBlackboard.TryGetValue(BlackboardKeys.EatenFood, out int foodEatenGroup))
                {
                    groupBlackboard.SetValue(BlackboardKeys.EatenFood, foodEatenGroup + 1);
                } else
                {
                    groupBlackboard.SetValue(BlackboardKeys.EatenFood, 1);
                }
            }
        }

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _FoodCounter.text = 0.ToString();
            botController.ServiceLocator.Register<IFoodEater>(this);
        }

        public override void Deinit()
        {
            base.Deinit();

            botController.ServiceLocator.Unregister<IFoodEater>();
        }
    }
}
