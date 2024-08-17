using UnityEngine;

namespace Game
{
    public abstract class CoreBotBrain : MonoBehaviour
    {
        protected CoreBotController botController { get; private set; }

        public virtual void Init(CoreBotController botController)
        {
            this.botController = botController;
        }

        public abstract void Think();

        public virtual void FixedThink()
        {
        }
    }
}
