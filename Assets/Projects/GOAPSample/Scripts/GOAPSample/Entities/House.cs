using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.GOAPSample
{
    public class House : GameEntity
    {
        #region Inspector

        [Header("Settings")]
        [SerializeField]
        MinMaxInt _BotsCount = new(1, 2);

        [SerializeField]
        float _SpawnRadius = 1f;

        [Header("Setup")]
        [SerializeField]
        Transform _Pivot;

        [SerializeField]
        Transform _Entrance;

        [SerializeField]
        Rigidbody2D _Rigidbody;

        [SerializeField]
        TMPro.TMP_Text _DebugInfo;

        #endregion

        public override Vector2 position
        {
            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                transform.position = p;

                var y = _Pivot.position.y;
                p.z = Mathf.InverseLerp(-100, 100, y) * 100;
                transform.position = p;
            }
        }

        public override float spawnRadius => _SpawnRadius;

        public override GameEntityType type => GameEntityType.House;

        IGameEntity _bonfire;

        readonly List<IGameEntity> _bots = new();

        readonly StateMachine _stateMachine = new();

        Coroutine _createCoroutine;

        IState _openedState;

        IState _closedState;

        Animator _animator;

        readonly HashSet<IGameEntity> _insideBots = new();

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponentInChildren<Animator>();

            _openedState = new OpenedHouseState(this);
            _closedState = new ClosedHouseState(this);

            _stateMachine.AddTransition(_openedState, _closedState, () =>
            {
                return _insideBots.Count > 0;
            });

            _stateMachine.AddTransition(_closedState, _openedState, () =>
            {
                return _insideBots.Count <= 0;
            });
        }

        protected override void Update()
        {
            base.Update();

            _stateMachine.Update();
            UpdateDebugInfo();
        }

        void UpdateDebugInfo()
        {
            if (!CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugMode))
            {
                _DebugInfo.gameObject.SetActive(false);
                return;
            }

            _DebugInfo.gameObject.SetActive(true);

            var sb = new StringBuilder();
            sb.AppendLine($"Bots Inside: {_insideBots.Count}");
            sb.AppendLine($"Wood: {Blackboard.GetValue(BlackboardKeys.WoodCount)}");
            sb.AppendLine($"Food: {Blackboard.GetValue(BlackboardKeys.FoodCount)}");
            _DebugInfo.text = sb.ToString();
        }

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            IEnumerator createEntities()
            {
                yield return new WaitForFixedUpdate();

                // Create bonfire.

                var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
                var bonfireSpawnRadius = factory.GetPrefab(GameEntityType.Bonfire).spawnRadius;
                var b = new Blackboard();

                for (int i = 0; i < 100; i++)
                {
                    var point = position + UnityEngine.Random.insideUnitCircle * this.spawnRadius;
                    var rect = new Rect(point.x - bonfireSpawnRadius, point.y - bonfireSpawnRadius, bonfireSpawnRadius * 2, bonfireSpawnRadius * 2);

                    if (_Rigidbody.OverlapPoint(rect.min) || _Rigidbody.OverlapPoint(rect.max))
                        continue;

                    b.Clear();
                    b.SetValue(BlackboardKeys.Position, point);
                    _bonfire = factory.Create(GameEntityType.Bonfire, b);
                    Blackboard.SetValue(BlackboardKeys.Bonfire, _bonfire);
                    break;
                }

                // Create bots.

                var botsCount = _BotsCount.Random();

                for (int i = 0; i < botsCount; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var point = position + UnityEngine.Random.insideUnitCircle * this.spawnRadius;

                        if (_Rigidbody.OverlapPoint(point))
                            continue;

                        b.Clear();
                        b.SetValue(BlackboardKeys.Position, point);
                        b.SetValue(BlackboardKeys.House, this);
                        _bots.Add(factory.Create(GameEntityType.Bot, b));
                        break;
                    }
                }
            }

            _createCoroutine = StartCoroutine(createEntities());

            _stateMachine.SetState(_openedState);

            Blackboard.SetValue(BlackboardKeys.InsideBots, _insideBots);
            Blackboard.SetValue(BlackboardKeys.Entrance, _Entrance.position);
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            if (_createCoroutine != null)
            {
                StopCoroutine(_createCoroutine);
                _createCoroutine = null;
            }

            _bonfire?.Destroy();

            foreach (var bot in _bots)
            {
                bot.Destroy();
            }

            _bots.Clear();

            _insideBots.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }

        abstract class HouseState : IState
        {
            protected House house;

            public HouseState(House house)
            {
                this.house = house;
            }

            public virtual void OnEnter() { }
            public virtual void OnExit() { }
            public virtual void OnFixedUpdate() { }
            public virtual void OnUpdate() { }
        }

        class OpenedHouseState : HouseState
        {
            public OpenedHouseState(House house) : base(house)
            {
            }

            public override void OnEnter()
            {
                house._animator.Play(AnimationKeys.Opened);
            }
        }

        class ClosedHouseState : HouseState
        {
            public ClosedHouseState(House house) : base(house)
            {
            }

            public override void OnEnter()
            {
                base.OnEnter();

                house._animator.Play(AnimationKeys.Closed);
            }
        }
    }
}
