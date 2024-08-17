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

        #endregion

        public event System.Action onRegenerate;

        public void OnRegenerateClicked()
        {
            onRegenerate?.Invoke();
        }
    }
}
