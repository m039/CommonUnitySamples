using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using System.Collections;
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
        SpriteRenderer _Renderer;

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
                transform.position = p;
            }
        }

        public GameEntityType type => GameEntityType.Food;

        public bool IsAlive { get; set; }

        public ServiceLocator locator => _serviceLocator;

        public int typeClass => 0;

        bool _created;

        readonly ServiceLocator _serviceLocator = new();

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

            StartCoroutine(WaitAndDestroy());

            _created = true;
        }

        IEnumerator WaitAndDestroy()
        {
            yield return new WaitForSeconds(_TimeToLive);

            var v = 0f;
            const float fadeDuration = 1.5f;

            while (v < 1)
            {
                v += Time.deltaTime / fadeDuration;

                _Renderer.color = _Renderer.color.WithAlpha(1 - v);
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
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            var gameEntity = collider.GetComponentInParent<IGameEntity>();
            if (gameEntity != null && gameEntity.locator.TryGet(out IFoodEater foodEater))
            {
                foodEater.Eat(this);
                this.Destroy();
            }
        }
    }
}
