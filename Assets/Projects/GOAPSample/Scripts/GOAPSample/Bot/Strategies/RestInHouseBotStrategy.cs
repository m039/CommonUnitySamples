using Game.BehaviourTreeSample;
using m039.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class RestInHouseBotStrategy : BotStrategy
    {
        readonly CountdownTimer _timer;

        bool _failedStart;

        IGameEntity _house;

        HashSet<IGameEntity> _insideBots;

        IGameEntity _gameEntity;

        bool _takeWood;

        bool _takeFood;

        public RestInHouseBotStrategy(CoreBotController botController, float duration, bool takeWood = false, bool takeFood = false) : base(botController)
        {
            _timer = new CountdownTimer(duration);
            _takeWood = takeWood;
            _takeFood = takeFood;
        }

        protected override bool isInterruptable => false;

        public override bool canPerform => true;

        public override bool complete => _failedStart || _timer.isFinished;

        public override void Start()
        {
            base.Start();

            _failedStart = false;

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.House, out _house))
            {
                _failedStart = true;
                return;
            }

            if (!_house.GetBlackboard().TryGetValue(BlackboardKeys.InsideBots, out _insideBots))
            {
                _failedStart = true;
                return;
            }

            if (!botController.ServiceLocator.TryGet(out _gameEntity))
            {
                _failedStart = true;
                return;
            }

            if (_insideBots.Contains(_gameEntity))
            {
                _failedStart = true;
                return;
            }

            _insideBots.Add(_gameEntity);
            _timer.Start();
            botController.Blackboard.SetValue(BlackboardKeys.IsInvisible, true);
        }

        public override void Stop()
        {
            base.Stop();

            botController.Blackboard.Remove(BlackboardKeys.Tiredness);
            botController.Blackboard.Remove(BlackboardKeys.Target);
            botController.Blackboard.Remove(BlackboardKeys.InDanger);

            if (_house != null) {

                if (_takeWood)
                {
                    if (!botController.Blackboard.ContainsKey(BlackboardKeys.HasWood) &&
                        _house.GetBlackboard().GetValue(BlackboardKeys.WoodCount) > 0)
                    {
                        _house.GetBlackboard().UpdateValue(BlackboardKeys.WoodCount, w => Mathf.Max(w - 1, 0));
                        botController.Blackboard.SetValue(BlackboardKeys.HasWood, true);
                    }
                }
                else
                {
                    if (botController.Blackboard.ContainsKey(BlackboardKeys.HasWood))
                    {
                        _house.GetBlackboard().UpdateValue(BlackboardKeys.WoodCount, w => w + 1);
                        botController.Blackboard.Remove(BlackboardKeys.HasWood);
                    }
                }

                if (_takeFood)
                {
                    if (!botController.Blackboard.ContainsKey(BlackboardKeys.HasFood) &&
                        _house.GetBlackboard().GetValue(BlackboardKeys.FoodCount) > 0)
                    {
                        _house.GetBlackboard().UpdateValue(BlackboardKeys.FoodCount, w => Mathf.Max(w - 1, 0));
                        botController.Blackboard.SetValue(BlackboardKeys.HasFood, true);
                    }
                }
                else
                {
                    if (botController.Blackboard.ContainsKey(BlackboardKeys.HasFood))
                    {
                        _house.GetBlackboard().UpdateValue(BlackboardKeys.FoodCount, w => w + 1);
                        botController.Blackboard.Remove(BlackboardKeys.HasFood);
                    }
                }
            }

            botController.Blackboard.Remove(BlackboardKeys.IsInvisible);

            _house = null;

            if (!_failedStart)
            {
                _insideBots.Remove(_gameEntity);
                _insideBots = null;
                _gameEntity = null;
            }
            _timer.Stop();
            _failedStart = false;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _timer.Tick(deltaTime);
        }
    }
}
