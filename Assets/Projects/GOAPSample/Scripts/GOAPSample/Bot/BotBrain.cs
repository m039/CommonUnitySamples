using Game.BehaviourTreeSample;
using Game.StateMachineSample;
using m039.Common;
using m039.Common.BehaviourTrees.Nodes;
using m039.Common.GOAP;
using m039.Common.StateMachine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class BotBrain : CoreBotBrain, Bot.IOnDestoyEntityEvent
    {
        #region Inspector

        [SerializeField]
        float _SensorRadius = 2f;

        [Header("States")]
        [SerializeField]
        CoreBotState _IdleState;

        [SerializeField]
        CoreBotState _MoveState;

        [SerializeField]
        CoreBotState _ChopTreeState;

        [SerializeField]
        CoreBotState _InvisibleState;

        #endregion

        StateMachine _stateMachine { get; } = new();

        readonly Agent _agent = new();

        CoreBotState[] _botStates;

        Timer _timer = new CountdownTimer(1);

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.EventBus.Subscribe(this);

            botController.ServiceLocator.Register(_stateMachine);
            botController.ServiceLocator.Register(_agent);

            // State Machine.

            _botStates = GetComponentsInChildren<CoreBotState>();

            foreach (var botState in _botStates)
            {
                botState.Init(botController);
            }

            _stateMachine.AddTransition(_IdleState, _MoveState, () =>
            {
                return botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            _stateMachine.AddTransition(_MoveState, _IdleState, () =>
            {
                return !botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            _stateMachine.AddAnyTransition(
                _ChopTreeState,
                () => botController.Blackboard.GetValue(BlackboardKeys.IsChoping)
            );

            _stateMachine.AddTransition(
                _ChopTreeState,
                _IdleState,
                () => !botController.Blackboard.GetValue(BlackboardKeys.IsChoping)
            );

            _stateMachine.AddAnyTransition(
                _InvisibleState,
                () => botController.Blackboard.GetValue(BlackboardKeys.IsInvisible)
            );

            _stateMachine.AddTransition(
                _InvisibleState,
                _IdleState,
                () => !botController.Blackboard.GetValue(BlackboardKeys.IsInvisible)
            );

            _stateMachine.SetState(_IdleState);

            _timer.onStop += () =>
            {
                if (!botController.Blackboard.GetValue(BlackboardKeys.NotInterrupt))
                {
                    _agent.CalculatePlan();
                }
                _timer.Start();
            };
            _timer.Start();

            SetupGOAP();
        }

        void SetupGOAP()
        {
            SetupBeliefs();
            SetupActions();
            SetupGoals();
        }

        void SetupBeliefs()
        {
            var beliefs = _agent.beliefs;
            var gameEntity = botController.ServiceLocator.Get<IGameEntity>();

            void addBelief(string name, Func<bool> condition)
            {
                beliefs[name] = new AgentBelief.Builder(name).WithCondition(condition).Build();
            }

            addBelief("Nothing", () => false);
            addBelief("Tired", () => botController.Blackboard.GetValue(BlackboardKeys.Tiredness) >= 10f);
            addBelief("NotTired", () => !beliefs["Tired"].Evaluate());
            addBelief("Hungry", () => botController.Blackboard.GetValue(BlackboardKeys.Hunger) > 0.8f);
            addBelief("NotHungry", () => !beliefs["Hungry"].Evaluate());
            addBelief("AgentMoving", () => botController.Blackboard.ContainsKey(BlackboardKeys.Destination));
            addBelief("Warm", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                var bonfire = house.GetBlackboard().GetValue(BlackboardKeys.Bonfire);
                return bonfire.GetBlackboard().GetValue(BlackboardKeys.IsLit);
            });
            addBelief("HasWood", () => botController.Blackboard.GetValue(BlackboardKeys.HasWood));
            addBelief("HasFood", () => botController.Blackboard.GetValue(BlackboardKeys.HasFood));
            addBelief("HasFoodFromGlade", () => beliefs["HasFood"].Evaluate());
            addBelief("HasWoodFromForest", () => beliefs["HasWood"].Evaluate());
            addBelief("NearBonfire", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Bonfire &&
                    Vector2.Distance(target.position, gameEntity.position) < _SensorRadius;
            });
            addBelief("FoundBonfire", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Bonfire;
            });
            addBelief("NearHouseEntrance", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                var entrance = house.GetBlackboard().GetValue(BlackboardKeys.Entrance);
                return Vector2.Distance(entrance, gameEntity.position) < _SensorRadius;

            });
            addBelief("InHouse", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                var insideBots = house.GetBlackboard().GetValue(BlackboardKeys.InsideBots);
                return insideBots.Contains(gameEntity);
            });
            addBelief("NearHouse", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                return Vector2.Distance(house.position, gameEntity.position) < house.spawnRadius;
            });
            addBelief("NearTree", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.IsAlive &&
                    target.type == GameEntityType.Tree &&
                    Vector2.Distance(target.position, gameEntity.position) < _SensorRadius;
            });
            addBelief("FoundTree", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.IsAlive && target.type == GameEntityType.Tree;
            });
            addBelief("NearForest", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Forest &&
                    Vector2.Distance(target.position, gameEntity.position) < target.spawnRadius;
            });
            addBelief("FoundForest", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Forest;
            });
            addBelief("HasWoodInHouse", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                return house.GetBlackboard().GetValue(BlackboardKeys.WoodCount) > 0;
            });
            addBelief("HasFoodInHouse", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                return house.GetBlackboard().GetValue(BlackboardKeys.FoodCount) > 0;
            });
            addBelief("GiveWood", () => false);
            addBelief("GiveFood", () => false);

            addBelief("NearMushroom", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.IsAlive &&
                    target.type == GameEntityType.Mushroom &&
                    Vector2.Distance(target.position, gameEntity.position) < _SensorRadius;
            });
            addBelief("FoundMushroom", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.IsAlive && target.type == GameEntityType.Mushroom;
            });
            addBelief("NearGlade", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Glade &&
                    Vector2.Distance(target.position, gameEntity.position) < target.spawnRadius;
            });
            addBelief("FoundGlade", () =>
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
                {
                    return false;
                }

                return target.type == GameEntityType.Glade;
            });
        }

        void SetupActions()
        {
            var actions = _agent.actions;
            var beliefs = _agent.beliefs;
            var gameEntity = botController.ServiceLocator.Get<IGameEntity>();

            actions.Add(new AgentAction.Builder("Wander Around")
                .WithStrategy(new WanderBotStrategy(botController, 4f))
                .AddPrecondition(beliefs["NotTired"])
                .AddEffect(beliefs["AgentMoving"])
                .AddEffect(beliefs["Tired"])
                .Build());

            actions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleBotStrategy(botController, 5))
                .AddEffect(beliefs["Nothing"])
                .Build());

            // Rest In House goal's action.

            actions.Add(new AgentAction.Builder("RestInHouse")
                .AddPrecondition(beliefs["NearHouseEntrance"])
                .AddPrecondition(beliefs["Tired"])
                .WithStrategy(new RestInHouseBotStrategy(botController, 5))
                .AddEffect(beliefs["InHouse"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToHouseEntrance")
                .AddPrecondition(beliefs["NearHouse"])
                .WithStrategy(new MoveBotStrategy(botController,
                 () =>
                 {
                     var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                     var entrance = house.GetBlackboard().GetValue(BlackboardKeys.Entrance);
                     return entrance;
                 },
                 () =>
                 {
                     return _SensorRadius;
                 }))
                .AddEffect(beliefs["NearHouseEntrance"])
                .Build());

            // Gather wood.

            actions.Add(new AgentAction.Builder("GatherWood")
                .WithStrategy(new RestInHouseBotStrategy(botController, 3))
                .AddPrecondition(beliefs["NearHouseEntrance"])
                .AddPrecondition(beliefs["HasWoodFromForest"])
                .AddEffect(beliefs["GiveWood"])
                .Build());

            // Take food from the house.

            actions.Add(new AgentAction.Builder("TakeFoodFromHouse")
                .WithStrategy(new RestInHouseBotStrategy(botController, 3, takeFood: true))
                .AddPrecondition(beliefs["NearHouseEntrance"])
                .AddPrecondition(beliefs["HasFoodInHouse"])
                .AddEffect(beliefs["HasFood"])
                .Build());

            // Gather mushrooms.

            actions.Add(new AgentAction.Builder("GatherFood")
                .WithStrategy(new RestInHouseBotStrategy(botController, 3))
                .AddPrecondition(beliefs["NearHouseEntrance"])
                .AddPrecondition(beliefs["HasFoodFromGlade"])
                .AddEffect(beliefs["GiveFood"])
                .Build());

            actions.Add(new AgentAction.Builder("Eat")
                .WithStrategy(new EatBotStrategy(botController))
                .AddPrecondition(beliefs["HasFood"])
                .AddEffect(beliefs["NotHungry"])
                .Build());

            actions.Add(new AgentAction.Builder("GatherMushroom")
                .WithStrategy(new TakeMushroomBotStrategy(botController))
                .AddPrecondition(beliefs["NearMushroom"])
                .AddEffect(beliefs["HasFood"])
                .AddEffect(beliefs["HasFoodFromGlade"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToMushroom")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target, false))
                .AddPrecondition(beliefs["FoundMushroom"])
                .AddEffect(beliefs["NearMushroom"])
                .Build());

            actions.Add(new AgentAction.Builder("FindMushroom")
                .WithStrategy(new FindRandomChildBotStrategy(botController))
                .AddPrecondition(beliefs["NearGlade"])
                .AddEffect(beliefs["FoundMushroom"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToGlade")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target))
                .AddPrecondition(beliefs["FoundGlade"])
                .AddEffect(beliefs["NearGlade"])
                .Build());

            actions.Add(new AgentAction.Builder("FindGlade")
                .WithStrategy(new FindGladeBotStrategy(botController))
                .AddEffect(beliefs["FoundGlade"])
                .Build());

            //
            // Lit bonfire goal's actions.
            //

            // Take wood from the house.
            actions.Add(new AgentAction.Builder("TakeWoodFromHouse")
                .WithStrategy(new RestInHouseBotStrategy(botController, 3, takeWood: true))
                .AddPrecondition(beliefs["NearHouseEntrance"])
                .AddPrecondition(beliefs["HasWoodInHouse"])
                .AddEffect(beliefs["HasWood"])
                .Build());

            // The main action.
            actions.Add(new AgentAction.Builder("LitBonfire")
                .WithStrategy(new LitBonfireBotStrategy(botController))
                .AddPrecondition(beliefs["HasWood"])
                .AddPrecondition(beliefs["NearBonfire"])
                .AddEffect(beliefs["Warm"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToBonfire")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target))
                .AddPrecondition(beliefs["FoundBonfire"])
                .AddEffect(beliefs["NearBonfire"])
                .Build());

            actions.Add(new AgentAction.Builder("FindBonfire")
                .WithStrategy(new FindBonfireBotStrategy(botController))
                .AddPrecondition(beliefs["NearHouse"])
                .AddEffect(beliefs["FoundBonfire"])
                .Build());

            actions.Add(new AgentAction.Builder("GoHome")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.House))
                .AddEffect(beliefs["NearHouse"])
                .Build());

            actions.Add(new AgentAction.Builder("ChopTree")
                .WithStrategy(new ChopTreeBotStrategy(botController))
                .AddPrecondition(beliefs["NearTree"])
                .AddPrecondition(beliefs["NotTired"])
                .AddPrecondition(beliefs["NotHungry"])
                .AddEffect(beliefs["HasWood"])
                .AddEffect(beliefs["HasWoodFromForest"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToTree")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target))
                .AddPrecondition(beliefs["FoundTree"])
                .AddEffect(beliefs["NearTree"])
                .Build());

            actions.Add(new AgentAction.Builder("FindTree")
                .WithStrategy(new FindRandomChildBotStrategy(botController))
                .AddPrecondition(beliefs["NearForest"])
                .AddEffect(beliefs["FoundTree"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToForest")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target))
                .AddPrecondition(beliefs["FoundForest"])
                .AddEffect(beliefs["NearForest"])
                .Build());

            actions.Add(new AgentAction.Builder("FindForest")
                .WithStrategy(new FindForestBotStrategy(botController))
                .AddEffect(beliefs["FoundForest"])
                .Build());
        }

        void SetupGoals()
        {
            var goals = _agent.goals;
            var beliefs = _agent.beliefs;

            goals.Add(new AgentGoal.Builder("Chill Out")
                .WithPriority(1)
                .WithDesiredEffect(beliefs["Nothing"])
                .Build());

            goals.Add(new AgentGoal.Builder("Rest in House")
                .WithPriority(2)
                .WithDesiredEffect(beliefs["InHouse"])
                .Build());

            goals.Add(new AgentGoal.Builder("Wander")
                .WithPriority(2)
                .WithDesiredEffect(beliefs["Tired"])
                .Build());

            const float gatherPriority = 4f;
            const float maxCount = 100000f;

            goals.Add(new AgentGoal.Builder("GatherWood")
                .WithPriority(() =>
                {
                    var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                    var woodCount = house.GetBlackboard().GetValue(BlackboardKeys.WoodCount);
                    return gatherPriority + Mathf.Lerp(0.5f, 0f, woodCount / maxCount);
                })
                .WithDesiredEffect(beliefs["GiveWood"])
                .Build());

            goals.Add(new AgentGoal.Builder("GatherFood")
                .WithPriority(() =>
                {
                    var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                    var foodCount = house.GetBlackboard().GetValue(BlackboardKeys.FoodCount);
                    return gatherPriority + Mathf.Lerp(0.5f, 0f, foodCount / maxCount);
                })
                .WithDesiredEffect(beliefs["GiveFood"])
                .Build());

            goals.Add(new AgentGoal.Builder("Lit Bonfire")
                .WithPriority(5)
                .WithDesiredEffect(beliefs["Warm"])
                .Build());

            goals.Add(new AgentGoal.Builder("Eat")
                .WithPriority(6)
                .WithDesiredEffect(beliefs["NotHungry"])
                .Build());
        }

        public override void Deinit()
        {
            base.Deinit();

            botController.EventBus.Unsubscribe(this);
            botController.ServiceLocator.Unregister(_stateMachine);

            foreach (var botState in _botStates)
            {
                botState.Deinit();
            }
        }

        public override void Think()
        {
            _timer.Tick(Time.deltaTime);
            _agent.Update();
            _stateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            _stateMachine.FixedUpdate();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _SensorRadius);
        }

        void Bot.IOnDestoyEntityEvent.OnDestroyEntity()
        {
            _agent.ClearState();
        }
    }
}
