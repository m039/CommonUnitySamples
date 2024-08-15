using m039.Common.DependencyInjection;
using UnityEngine;

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

        #endregion

        public event System.Action onRegenerate;

        public void OnRegenerateClicked()
        {
            onRegenerate?.Invoke();
        }
    }
}
