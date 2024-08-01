using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IGameEntityCreatedEvent : IEventSubscriber
    {
        void OnGameEntityCreated(IGameEntity gameEntity);
    }

    public interface IGameEntityDestroyedEvent : IEventSubscriber
    {
        void OnGameEntityDestroyed(IGameEntity gameEntity);
    }

    public interface IGameEntityFactory
    {
        T Create<T>(Blackboard blackboard) where T : IGameEntity;

        void CreateManually<T>(T gameEntity) where T : IGameEntity;

        void Destroy<T>(T gameEntity) where T : IGameEntity;
    }

    public class GameEntityFactory : MonoBehaviour, IGameEntityFactory, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        GameObject[] _Prefabs;

        #endregion

        [Inject]
        readonly EventBusByInterface _eventBus;

        [Inject]
        readonly ServiceLocator _serviceLocator;

        [Provide]
        IGameEntityFactory GetEntityFactory()
        {
            return this;
        }

        readonly Dictionary<Type, GameObject> _typeToPrefab = new();

        int _entitiesId;

        Transform _parent;

        readonly Blackboard _blackboard = new();

        void Awake()
        {
            foreach (var prefab in _Prefabs)
            {
                var gameEntity = prefab.GetComponent<IGameEntity>();
                if (gameEntity == null)
                {
                    Debug.LogError($"The {prefab} should implements {nameof(IGameEntity)}.");
                    continue;
                }

                var type = gameEntity.GetType();

                if (_typeToPrefab.ContainsKey(type)) {
                    Debug.LogError($"The {prefab} is already registered.");
                    continue;
                }

                _typeToPrefab[type] = prefab;
            }

            _serviceLocator.Register<IGameEntityFactory>(this);
        }

        public T Create<T>(Blackboard blackboard) where T: IGameEntity
        {
            if (!_typeToPrefab.ContainsKey(typeof(T)))
            {
                throw new Exception($"Can't create the object for {nameof(T)}.");
            }

            if (blackboard == null)
            {
                blackboard = _blackboard;
                blackboard.Clear();
            }

            if (_parent == null)
            {
                _parent = new GameObject("<GameEntities>").transform;
            }

            var instance = Instantiate(_typeToPrefab[typeof(T)]);

            instance.name += $"#{_entitiesId}";

            var gameEntity = instance.GetComponent<IGameEntity>();

            blackboard.SetValue(BlackboardKeys.Id, _entitiesId++);

            gameEntity.OnCreate(blackboard);
            gameEntity.IsAlive = true;

            instance.transform.SetParent(_parent);

            _eventBus.Raise<IGameEntityCreatedEvent>(a => a.OnGameEntityCreated(gameEntity));

            return (T)gameEntity;
        }

        public void Destroy<T>(T gameEntity) where T : IGameEntity
        {
            gameEntity.OnDestroy();
            gameEntity.IsAlive = false;
            _eventBus.Raise<IGameEntityDestroyedEvent>(a => a.OnGameEntityDestroyed(gameEntity));

            if (gameEntity is MonoBehaviour monoBehaviour)
            {
                UnityEngine.Object.Destroy(monoBehaviour.gameObject);
            }
        }

        public void CreateManually<T>(T gameEntity) where T : IGameEntity
        {
            _blackboard.Clear();
            _blackboard.SetValue(BlackboardKeys.Id, _entitiesId++);
            gameEntity.OnCreate(_blackboard);
            gameEntity.IsAlive = true;
        }
    }
}
