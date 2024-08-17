using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class EntityBotController : CoreBotController
    {
        protected override void Awake()
        {
            base.Awake();

            var gameEntity = GetComponent<IGameEntity>();

            ServiceLocator.Register(GetComponentInChildren<Animator>());
            ServiceLocator.Register(gameEntity);
            ServiceLocator.Register(Blackboard);
        }
    }
}
