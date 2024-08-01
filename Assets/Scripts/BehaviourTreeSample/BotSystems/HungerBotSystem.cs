using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game
{
    public class HungerBotSystem : BotSystem, IFoodEater
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _FoodCounter;

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
        }

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _FoodCounter.text = 0.ToString();
        }
    }
}
