using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class BotController : CoreBotController
    {
        Bot _bot;

        BotSystem[] _systems;

        protected override void Awake()
        {
            base.Awake();

            _bot = GetComponent<Bot>();

            ServiceLocator.Register(GetComponentInChildren<Animator>());
            ServiceLocator.Register<IGameEntity>(_bot);
            ServiceLocator.Register(Blackboard);
        }

        protected override void Start()
        {
            base.Start();

            _systems = GetComponentsInChildren<BotSystem>();

            foreach (var botSystem in _systems)
            {
                botSystem.Init(this);
            }
        }

        protected override void Update()
        {
            base.Update();

            foreach (var botSystem in _systems)
            {
                botSystem.Process(Time.deltaTime);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var botSystem in _systems)
            {
                botSystem.Deinit();
            }
        }
    }
}
