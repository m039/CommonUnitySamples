using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public interface IFoodEater
    {
        void Eat(IGameEntity food);
    }

    public interface IFoodEatenEvent : IEventSubscriber
    {
        void FoodEaten(IGameEntity eater, IGameEntity food);
    }

    public class Food : MonoBehaviour, IGameEntity
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer[] _Renderers;

        [SerializeField]
        float _TimeToLive = 6f;

        #endregion

        public int id { get; private set; } = 0;

        string IGameEntity.name => "Food#" + id;

        public Vector2 position {
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

        public GameEntityType type => GameEntityType.Food;

        public bool IsAlive { get; set; }

        public ServiceLocator locator {
            get
            {
                if (_serviceLocator == null)
                {
                    _serviceLocator = new();
                    _serviceLocator.Register(_blackboard);
                }

                return _serviceLocator;
            }
        }

        public int typeClass => 0;

        bool _created;

        public bool isSetActive { get; set; }

        ServiceLocator _serviceLocator;

        readonly BlackboardBase _blackboard = new GameBlackboard();

        Coroutine _coroutine;

        List<(SpriteRenderer r, float a)> _spriteRenders;

        void Awake()
        {
            _spriteRenders = _Renderers.Select(r => (r, r.color.a)).ToList();
        }

        public void OnCreate(BlackboardBase blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out int _id))
            {
                id = _id;
            }

            if (blackboard.TryGetValue(BlackboardKeys.Position, out Vector2 _position))
            {
                position = _position;
            }

            _coroutine = StartCoroutine(WaitAndDestroy());

            _created = true;

            SetAlpha(1f);
        }

        void SetAlpha(float value)
        {
            foreach (var (r, a) in _spriteRenders)
            {
                r.color = r.color.WithAlpha(value * a);
            }
        }

        IEnumerator WaitAndDestroy()
        {
            yield return new WaitForSeconds(_TimeToLive);

            var v = 0f;
            const float fadeDuration = 1.5f;

            while (v < 1)
            {
                v += Time.deltaTime / fadeDuration;

                SetAlpha(1 - v);
                yield return null;
            }

            this.Destroy();
        }

        void Start()
        {
            if (!_created)
            {
                CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>()?.CreateManually(this);
            }
        }

        void IGameEntity.OnDestroy()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _blackboard.Clear();
            _created = false;
            isSetActive = false;
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!IsAlive)
            {
                // When the object is disabled OnTriggerEnter can be called.
                return;
            }

            var gameEntity = collider.GetComponentInParent<IGameEntity>();
            if (gameEntity != null && gameEntity.locator.TryGet(out IFoodEater foodEater))
            {
                foodEater.Eat(this);
                CoreGameController.Instance.EventBus.Raise<IFoodEatenEvent>(a => a.FoodEaten(gameEntity, this));
                this.Destroy();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Food))]
        public class FoodEditor : Editor
        {
#if false

            bool showDebug;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var food = (Food)target;

                showDebug = EditorGUILayout.Toggle("Debug Blackboard", showDebug);
                if (!showDebug)
                    return;

                if (food._blackboard == null || food._blackboard.Count <= 0)
                {
                    GUILayout.Label("Blackboard is empty");
                }
                else
                {
                    GUILayout.Label("Blackboard");


                    foreach (var entries in food._blackboard)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.Label($"{entries.Key.name}: {entries.Value}");
                        GUILayout.EndHorizontal();
                    }


                if (food._blackboard.TryGetValue(new(nameof(FindFreeTargetBotNode) + ".takenKey"), out HashSet<int> hashSet))
                    {
                        GUILayout.Label($"taken ids: " + string.Join(",", hashSet));
                    }
                }
            }
#endif
        }
#endif
    }
}
