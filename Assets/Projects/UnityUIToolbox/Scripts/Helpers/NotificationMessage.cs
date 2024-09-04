using m039.Common.DependencyInjection;
using UnityEngine;

namespace Game
{
    public class NotificationMessage : MonoBehaviour, IDependencyProvider
    {
        public enum Level
        {
            Normal, Warning
        }

        [Provide]
        NotificationMessage GetNotificationMessage() => this;

        Transform _content;

        TMPro.TMP_Text _text;

        bool _inited;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            if (_inited)
                return;

            _content = transform.Find("Content");
            _text = _content.Find("Text").GetComponent<TMPro.TMP_Text>();
            _inited = true;
            _content.gameObject.SetActive(false);
        }

        public void SetLevel(Level level)
        {
            Init();

            if (level == Level.Normal)
            {
                _text.color = Color.white;
            } else if (level == Level.Warning)
            {
                _text.color = Color.red;
            }
        }

        public void SetMessage(string message)
        {
            Init();

            _content.gameObject.SetActive(true);
            _text.text = message;
        }
    }
}
