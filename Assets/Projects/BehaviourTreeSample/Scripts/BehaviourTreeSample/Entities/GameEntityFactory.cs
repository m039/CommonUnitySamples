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
        IGameEntity Create(GameEntityType type, BlackboardBase blackboard);

        IGameEntity GetPrefab(GameEntityType type, int typeClass = 0);

        void CreateManually(IGameEntity gameEntity);

        void Destroy(IGameEntity gameEntity);

        IList<IGameEntity> GetEnteties(GameEntityType type);
    }

    public class GameEntityFactory : MonoBehaviour, IGameEntityFactory, IDependencyProvider
    {
        static readonly IList<IGameEntity> s_EmptyCollection = new List<IGameEntity>().AsReadOnly();

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

        readonly Dictionary<GameEntityType, Dictionary<int, GameObject>> _typeToPrefab = new();

        readonly Dictionary<GameEntityType, List<IGameEntity>> _enteties = new();

        readonly Dictionary<GameEntityType, Dictionary<int, Queue<IGameEntity>>> _pool = new();

        int _entitiesId;

        Transform _parent;

        readonly BlackboardBase _blackboard = new GameBlackboard();

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

                var type = gameEntity.type;

                if (!_typeToPrefab.ContainsKey(type))
                {
                    _typeToPrefab[type] = new();
                }

                var typeClass = gameEntity.typeClass;

                if (_typeToPrefab[type].ContainsKey(typeClass)) {
                    Debug.LogError($"The {prefab} of {type} is already registered for {typeClass}.");
                    continue;
                }

                _typeToPrefab[type].Add(typeClass, prefab);
            }

            _serviceLocator.Register<IGameEntityFactory>(this);
        }

        public IGameEntity Create(GameEntityType type, BlackboardBase blackboard)
        {
            if (!_typeToPrefab.ContainsKey(type))
            {
                throw new Exception($"Can't create the object for {type}.");
            }

            if (blackboard == null)
            {
                blackboard = _blackboard;
                blackboard.Clear();
            }

            int typeClass;

            if (blackboard.TryGetValue(BlackboardKeys.TypeClass, out int value))
            {
                typeClass = value;
            } else
            {
                typeClass = 0;
            }

            if (!_typeToPrefab[type].ContainsKey(typeClass))
            {
                throw new Exception($"Can't creat the object of {type} for {typeClass}");
            }

            if (_parent == null)
            {
                _parent = new GameObject("<GameEntities>").transform;
            }

            IGameEntity gameEntity;

            // Take a game entity from the pool.

            if (_pool.ContainsKey(type) && _pool[type].ContainsKey(typeClass))
            {
                gameEntity = _pool[type][typeClass].Dequeue();

                if (_pool[type][typeClass].Count <= 0)
                {
                    _pool[type].Remove(typeClass);

                    if (_pool[type].Count <= 0) {
                        _pool.Remove(type);
                    }
                }
            } else
            {
                // Or create a new one.

                var instance = Instantiate(_typeToPrefab[type][typeClass]);

                instance.transform.SetParent(_parent);

                gameEntity = instance.GetComponent<IGameEntity>();
            }

            if (gameEntity is MonoBehaviour monoBehaviour)
            {
                if (gameEntity is Food food)
                {
                    food.isSetActive = true;
                }
                monoBehaviour.gameObject.name = $"{_typeToPrefab[type][typeClass].gameObject.name}#{_entitiesId}";
                monoBehaviour.gameObject.SetActive(true);
            }

            blackboard.SetValue(BlackboardKeys.Id, _entitiesId++);

            gameEntity.OnCreate(blackboard);
            gameEntity.IsAlive = true;

            if (!_enteties.ContainsKey(gameEntity.type))
            {
                _enteties[gameEntity.type] = new();
            }

            _enteties[gameEntity.type].Add(gameEntity);
            _eventBus.Raise<IGameEntityCreatedEvent>(a => a.OnGameEntityCreated(gameEntity));

            return gameEntity;
        }

        public void Destroy(IGameEntity gameEntity)
        {
            if (!gameEntity.IsAlive)
                return;

            gameEntity.OnDestroy();
            gameEntity.IsAlive = false;
            _eventBus.Raise<IGameEntityDestroyedEvent>(a => a.OnGameEntityDestroyed(gameEntity));

            if (gameEntity is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.gameObject.SetActive(false);
            }

            if (_enteties.ContainsKey(gameEntity.type))
            {
                _enteties[gameEntity.type].Remove(gameEntity);

                if (_enteties[gameEntity.type].Count <= 0)
                {
                    _enteties.Remove(gameEntity.type);
                }
            }

            // Return a game entity to the pool.

            if (!_pool.ContainsKey(gameEntity.type))
            {
                _pool[gameEntity.type] = new();
            }

            if (!_pool[gameEntity.type].ContainsKey(gameEntity.typeClass))
            {
                _pool[gameEntity.type][gameEntity.typeClass] = new();
            }

            _pool[gameEntity.type][gameEntity.typeClass].Enqueue(gameEntity);
        }

        public void CreateManually(IGameEntity gameEntity)
        {
            _blackboard.Clear();
            _blackboard.SetValue(BlackboardKeys.Id, _entitiesId++);
            gameEntity.OnCreate(_blackboard);
            gameEntity.IsAlive = true;
        }

        public IList<IGameEntity> GetEnteties(GameEntityType type)
        {
            if (_enteties.ContainsKey(type))
            {
                return _enteties[type];
            } else
            {
                return s_EmptyCollection;
            }
        }

        public IGameEntity GetPrefab(GameEntityType type, int typeClass = 0)
        {
            return _typeToPrefab[type][typeClass].GetComponent<IGameEntity>();
        }
    }
}
