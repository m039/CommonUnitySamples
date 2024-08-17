using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class EntityBotController : CoreBotController
    {
        IGameEntity _gameEntity;

        protected override void Awake()
        {
            base.Awake();

            _gameEntity = GetComponent<IGameEntity>();

            ServiceLocator.Register(GetComponentInChildren<Animator>());
            ServiceLocator.Register(_gameEntity);
            ServiceLocator.Register(Blackboard);
        }
    }
}
