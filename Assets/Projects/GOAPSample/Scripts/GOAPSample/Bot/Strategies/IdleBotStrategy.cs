using m039.Common;

namespace Game.GOAPSample
{
    public class IdleBotStrategy : BotStrategy
    {
        bool _complete;
        readonly CountdownTimer _timer;

        public IdleBotStrategy(CoreBotController botController, float duration) : base(botController)
        {
            _timer = new CountdownTimer(duration);
            _timer.onStart += () => _complete = false;
            _timer.onStop += () => _complete = true;
        }

        public override bool canPerform => true;

        public override bool complete => _complete;

        public override void Start() => _timer.Start();

        public override void Update(float deltaTime) => _timer.Tick(deltaTime);

        public override void Stop()
        {
            botController.Blackboard.Remove(BlackboardKeys.Tiredness);
        }
    }
}
