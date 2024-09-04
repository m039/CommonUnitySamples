using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveView : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        string _Id;

        #endregion

        public string id => _Id;

        private void Awake()
        {
            AdaptiveManager.Instance.Register(this);
        }
    }
}
