using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BotSystem : MonoBehaviour
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
