using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class Food : GameEntity
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer[] _Renderers;

        [SerializeField]
        float _TimeToLive = 6f;

        #endregion

        public override GameEntityType type => GameEntityType.Food;

        bool _created;

        Coroutine _coroutine;

        List<(SpriteRenderer r, float a)> _spriteRenders;

        protected override void Awake()
        {
            base.Awake();
            _spriteRenders = _Renderers.Select(r => (r, r.color.a)).ToList();
        }

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

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

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _created = false;
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
    }
}
