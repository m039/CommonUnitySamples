using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveViewContainer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        string _Id;

        [SerializeField]
        bool _HideIfNoView = true;

        [SerializeField]
        string _Label;

        #endregion

        public string label => _Label;

        public string id => _Id;

        public bool hideIfNoView => _HideIfNoView;
    }
}
