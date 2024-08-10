using m039.Common.StateMachine;

namespace Game.StateMachineSample
{
    public class CoreBotState : MonoBehaviourState
    {
        public CoreBotController botController { get; private set; }

        public void Init(CoreBotController botController)
        {
            this.botController = botController;
            OnInit(botController);
        }

        protected virtual void OnInit(CoreBotController botController)
        {

        }

        public virtual void Deinit()
        {
        }
    }
}
