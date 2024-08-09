using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SetColorBotFeature : CoreBotFeature, ISetColorEvent
    {
        #region Inspector

        [SerializeField] SpriteRenderer _Renderer;

        #endregion

        public override void Init(CoreBotController botController)
        {
            botController.EventBus.Subscribe(this);
        }

        public override void Deinit(CoreBotController botController)
        {
            botController.EventBus.Unsubscribe(this);
        }

        void ISetColorEvent.SetColor(Color color)
        {
            _Renderer.color = color;
        }
    }
}
