using UnityEngine;

namespace Game
{
    public class CoreBotSystem : MonoBehaviour
    {
        public CoreBotController botController { get; private set; }

        public virtual void Init(CoreBotController botController)
        {
            this.botController = botController;
        }

        public virtual void Process(float deltaTime)
        {
        }
    }
}
