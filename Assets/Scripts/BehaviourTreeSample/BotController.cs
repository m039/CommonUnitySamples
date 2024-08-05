using m039.Common;
using m039.Common.Blackboard;
using m039.Common.StateMachine;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static m039.Common.EasingFunction;

namespace Game.BehaviourTreeSample
{
    public enum BotClass
    {
        Blue = 0,
        Yellow = 1,
        Green = 2
    }

    public class BotController : CoreBotController, IGameEntity
    {
        #region Inspector

        [SerializeField]
        BotClass _TypeClass;

        #endregion

        public int id { get; private set; }

        public Vector2 position
        {
            get
            {
                return transform.position;
            }

            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                p.z = Mathf.InverseLerp(-100, 100, value.y) * 100;
                transform.position = p;
            }
        }

        public GameEntityType type => GameEntityType.Bot;

        public bool IsAlive { get; set; }

        ServiceLocator IGameEntity.locator => ServiceLocator;

        public int typeClass => (int)_TypeClass;

        bool _created;

        public void OnCreate(BlackboardBase blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out int _id)) {
                id = _id;
            }

            if (blackboard.TryGetValue(BlackboardKeys.Position, out Vector2 _position))
            {
                position = _position;
            }

            if (blackboard.TryGetValue(BlackboardKeys.GroupBlackboard, out BlackboardBase _groupBlackboard))
            {
                Blackboard.SetValue(BlackboardKeys.GroupBlackboard, _groupBlackboard);
            }

            _created = true;
        }

        public void SetState(MonoBehaviourState state)
        {
            ServiceLocator.Get<StateMachine>().SetState(state);
        }

        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.Register(GetComponent<Animator>());
            ServiceLocator.Register<IGameEntity>(this);
        }

        protected override void Start()
        {
            base.Start();

            foreach (var botSystem in GetComponentsInChildren<BotSystem>())
            {
                botSystem.Init(this);
            }

            if (!_created)
            {
                CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>()?.CreateManually(this);
            }

            Blackboard.SetValue(BlackboardKeys.StartPosition, transform.position);
        }

        void IGameEntity.OnDestroy()
        {
            _created = false;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BotController))]
    public class BotControllerEditor : Editor
    {

#if false
        bool showDebug;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var botController = (BotController)target;

            showDebug = EditorGUILayout.Toggle("Debug Blackboard", showDebug);
            if (!showDebug)
                return;

            if (botController.Blackboard.Count <= 0)
            {
                GUILayout.Label("Blackboard is empty");
            } else
            {
                GUILayout.Label("Blackboard");

                foreach (var entries in botController.Blackboard)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label($"{entries.Key.name}: {entries.Value}");
                    GUILayout.EndHorizontal();
                }
            }
        }
#endif
    }
#endif
}
