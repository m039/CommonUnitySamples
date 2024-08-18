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

        public TMPro.TMP_Text fpsCounter;

        public Toggle debugModeToggle;

        public Toggle debugPathfindingToggle;

        [SerializeField]
        RectTransform _WarningNotification;

        #endregion

        public event System.Action onRegenerate;

        public void OnRegenerateClicked()
        {
            onRegenerate?.Invoke();
        }

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
