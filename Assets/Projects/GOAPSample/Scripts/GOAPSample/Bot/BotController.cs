using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class BotController : CoreBotController
    {
        Bot _bot;

        protected override void Awake()
        {
            base.Awake();

            _bot = GetComponent<Bot>();

            ServiceLocator.Register(GetComponentInChildren<Animator>());
            ServiceLocator.Register<IGameEntity>(_bot);
            ServiceLocator.Register(Blackboard);
        }
    }
}
