using UnityEngine;

namespace Game
{
    public class BotController : CoreBotController, ISetColorEvent
    {
        [SerializeField] float _MoveSpeed = 10f;

        [SerializeField] SpriteRenderer _Renderer;

        private void Awake()
        {
            Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
            Blackboard.SetValue(BlackboardKeys.StartPosition, transform.position);

            EventBus.Subscribe(this);
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe(this);
        }

        void OnValidate()
        {
            Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
        }

        void ISetColorEvent.SetColor(Color color)
        {
            _Renderer.color = color;
        }
    }
}
