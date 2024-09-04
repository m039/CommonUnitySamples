using m039.Common.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FPSCounter : MonoBehaviour, IDependencyProvider
    {
        [Provide]
        FPSCounter GetFPSCounter() => this;

        TMPro.TMP_Text _text;

        void Awake()
        {
            _text = GetComponent<TMPro.TMP_Text>();
        }

        float _fpsTimer = 0;

        void Update()
        {
            _fpsTimer -= Time.deltaTime;
            if (_fpsTimer < 0)
            {
                _text.text = string.Format("FPS: {0,3:f2}", 1 / Time.deltaTime);
                _fpsTimer = 0.1f;
            }
        }
    }
}
