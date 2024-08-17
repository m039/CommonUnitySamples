using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class SamplesMenuController : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        bool _Main = true;

        [SerializeField]
        RectTransform _Frame;

        [SerializeField]
        RectTransform _Buttons;

        [SerializeField]
        RectTransform _ButtonTemplate;

        [SerializeField]
        RectTransform _CloseButton;

        [SerializeField]
        RectTransform _SamplesButton;

        #endregion

        void Start()
        {
            CreateButtons();

            if (_Main)
            {
                _Frame.gameObject.SetActive(true);
                _CloseButton.gameObject.SetActive(false);
                _SamplesButton.gameObject.SetActive(false);
            }
            else
            {
                _Frame.gameObject.SetActive(false);
                _CloseButton.gameObject.SetActive(true);
                _SamplesButton.gameObject.SetActive(true);
            }
        }

        void CreateButtons()
        {
            var buttons = new List<(string, string)>()
            {
                ("GOAP", "GOAPSample"),
                ("Behaviour Tree", "BehaviourTreeSample"),
                ("State Machine", "StateMachineSample"),
                ("Pathfinding", "PathfindingSample")
            };

            foreach (Transform child in _Buttons)
            {
                if (child == _ButtonTemplate)
                    continue;

                Destroy(child.gameObject);
            }

            foreach (var (name, scene) in buttons)
            {
                var button = Instantiate(_ButtonTemplate);
                button.SetParent(_Buttons, false);
                button.gameObject.SetActive(true);

                var text = button.Find("Text").GetComponent<TMPro.TMP_Text>();
                text.text = name;

                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnButtonClicked(scene);
                });
            }

            _ButtonTemplate.gameObject.SetActive(false);
        }

        void OnButtonClicked(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        public void OnCloseButtonClicked()
        {
            _Frame.gameObject.SetActive(false);
        }

        public void OnSamplesClicked()
        {
            _Frame.gameObject.SetActive(!_Frame.gameObject.activeSelf);
        }
    }
}
