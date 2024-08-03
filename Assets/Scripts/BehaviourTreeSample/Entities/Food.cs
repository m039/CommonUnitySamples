using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public interface IFoodEater
    {
        void Eat(IGameEntity food);
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
                p.z = value.y;
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

        readonly Blackboard _blackboard = new();

        Coroutine _coroutine;

        List<(SpriteRenderer r, float a)> _spriteRenders;

        void Awake()
        {
            _spriteRenders = _Renderers.Select(r => (r, r.color.a)).ToList();
        }

        public void OnCreate(Blackboard blackboard)
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
                this.Destroy();
            }
        }
    }
}
