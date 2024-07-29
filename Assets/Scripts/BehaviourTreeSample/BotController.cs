using m039.Common.StateMachine;

namespace Game.BehaviourTreeSample
{
    public class BotController : CoreBotController
    {
        public void SetState(MonoBehaviourState state)
        {
            ServiceLocator.Get<StateMachine>().SetState(state);
        }
    }
}
