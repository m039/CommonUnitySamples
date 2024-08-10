using m039.Common.GOAP;

namespace Game.GOAPSample
{
    public abstract class BotStrategy : IActionStrategy
    {
        public abstract bool canPerform { get; }

        public abstract bool complete { get; }

        protected CoreBotController botController;

        public BotStrategy(CoreBotController botController)
        {
            this.botController = botController;
        }

        public virtual void Start()
        {
            // noop
        }

        public virtual void Update(float deltaTime)
        {
            // noop
        }

        public virtual void Stop()
        {
            // noop
        }
    }
}
