using UnityEngine;

namespace Game
{
    public class SetColorBotSystem : CoreBotSystem, ISetColorEvent
    {
        #region Inspector

        [SerializeField] SpriteRenderer _Renderer;

        #endregion

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.EventBus.Subscribe(this);
        }

        void ISetColorEvent.SetColor(Color color)
        {
            _Renderer.color = color;
        }
    }
}
