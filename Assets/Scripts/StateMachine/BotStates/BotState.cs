namespace Game
{
    public class BotState : MonoBehaviourState
    {
        public BotController botController { get; private set; }

        public void Init(BotController botController)
        {
            this.botController = botController;
            OnInit(botController);
        }

        protected virtual void OnInit(BotController botController)
        {

        }
    }
}
