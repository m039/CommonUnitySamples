using m039.Common.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameUI : MonoBehaviour, IDependencyProvider
    {
        [Provide]
        GameUI GetUI()
        {
            return this;
        }

        #region Inspector

        public TMPro.TMP_Text botInfo;

        [SerializeField]
        RectTransform _WarningNotification;

        #endregion

        public void ClearWarningNotification()
        {
            _WarningNotification.gameObject.SetActive(false);
        }

        public void SetWarningNotification(string text)
        {
            _WarningNotification.gameObject.SetActive(true);
            _WarningNotification.Find("Text").GetComponent<TMPro.TMP_Text>().text = text;
        }
    }
}
