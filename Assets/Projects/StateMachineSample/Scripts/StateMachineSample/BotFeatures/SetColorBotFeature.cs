using UnityEngine;

namespace Game
{
    public class SetColorBotFeature : CoreBotFeature, ISetColorEvent
    {
        #region Inspector

        [SerializeField] SpriteRenderer _Renderer;

        #endregion

        protected override void OnInit()
        {
            botController.EventBus.Subscribe(this);
        }

        protected override void OnDeinit()
        {
            botController.EventBus.Unsubscribe(this);
        }

        void ISetColorEvent.SetColor(Color color)
        {
            _Renderer.color = color;
        }
    }
}
