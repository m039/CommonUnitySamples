using m039.Common;
using m039.Common.Events;
using System.Collections;
using UnityEngine;

namespace Game
{
    public interface IBlinkEvent : IEventSubscriber
    {
        public void Blink(Color color, float duration);
    }

    public class BlinkBotFeature : CoreBotFeature, IBlinkEvent
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _Renderer;

        #endregion

        Coroutine _coroutine;

        public void Blink(Color color, float duration)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(BlinkCoroutine(color, duration));
        }

        IEnumerator BlinkCoroutine(Color color, float duration)
        {
            _Renderer.color = Color.white;

            var durationHalf = duration / 2f;

            float v;

            v = 0f;

            while (v < durationHalf) {
                v += Time.deltaTime;
                yield return null;

                var easing = EasingFunction.EaseOutExpo(0, 1, v / durationHalf);

                _Renderer.color = Color.Lerp(Color.white, color, easing);
            }

            v = 0f;

            while (v < durationHalf)
            {
                v += Time.deltaTime;
                yield return null;

                var easing = EasingFunction.EaseInExpo(1, 0, v / durationHalf);

                _Renderer.color = Color.Lerp(Color.white, color, easing);
            }

            _Renderer.color = Color.white;
        }

        public override void Init(CoreBotController botController)
        {
            botController.EventBus.Subscribe(this);
        }

        public override void Deinit(CoreBotController botController)
        {
            botController.EventBus.Unsubscribe(this);
        }
    }
}
