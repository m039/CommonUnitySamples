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
        CoreBotState _IdleState;

        [SerializeField]
        CoreBotState _MoveState;

        [SerializeField]
        CoreBotState _ChopTreeState;

        [SerializeField]
        float _SensorRadius = 2f;

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

            _stateMachine.SetState(_IdleState);

            _timer.onStop += () =>
            {
                _agent.CalculatePlan();
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
            addBelief("NotTired", () => botController.Blackboard.GetValue(BlackboardKeys.Tiredness) < 10f);
            addBelief("AgentMoving", () => botController.Blackboard.ContainsKey(BlackboardKeys.Destination));
            addBelief("Warm", () =>
            {
                var house = botController.Blackboard.GetValue(BlackboardKeys.House);
                var bonfire = house.GetBlackboard().GetValue(BlackboardKeys.Bonfire);
                return bonfire.GetBlackboard().GetValue(BlackboardKeys.IsLit);
            });
            addBelief("HasWood", () => botController.Blackboard.GetValue(BlackboardKeys.HasWood));
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
        }

        void SetupActions()
        {
            var actions = _agent.actions;
            var beliefs = _agent.beliefs;

            actions.Add(new AgentAction.Builder("Wander Around")
                .WithStrategy(new WanderBotStrategy(botController, 4f))
                .AddPrecondition(beliefs["NotTired"])
                .AddEffect(beliefs["AgentMoving"])
                .Build());

            actions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleBotStrategy(botController, 5))
                .AddEffect(beliefs["Nothing"])
                .Build());

            // Lit bonfire actions.

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
                .AddEffect(beliefs["HasWood"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToTree")
                .WithStrategy(new MoveToBotStrategy(botController, BlackboardKeys.Target))
                .AddPrecondition(beliefs["FoundTree"])
                .AddEffect(beliefs["NearTree"])
                .Build());

            actions.Add(new AgentAction.Builder("FindTree")
                .WithStrategy(new FindTreeBotStrategy(botController))
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

            goals.Add(new AgentGoal.Builder("Wander")
                .WithPriority(2)
                .WithDesiredEffect(beliefs["AgentMoving"])
                .Build());

            goals.Add(new AgentGoal.Builder("Lit Bonfire")
                .WithPriority(3)
                .WithDesiredEffect(beliefs["Warm"])
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
