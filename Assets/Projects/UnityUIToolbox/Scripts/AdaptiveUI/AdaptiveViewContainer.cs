using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AdaptiveViewContainer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        string _Id;

        [SerializeField]
        bool _HideIfNoView = true;

        #endregion

        public string id => _Id;

        public bool hideIfNoView => _HideIfNoView;
    }
}
