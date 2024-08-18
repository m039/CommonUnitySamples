using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor.UI;
using UnityEditor;
#endif

namespace Game
{
    public class SamplesMenuButton : Button
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _Text;

        [SerializeField]
        Color _NormalColor = Color.black;

        [SerializeField]
        Color _HighlightedColor = Color.white;

        #endregion

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (_Text == null)
                return;

            Color color;

            switch (state)
            {
                case SelectionState.Disabled:
                    color = _NormalColor;
                    break;
                case SelectionState.Highlighted:
                    color = _HighlightedColor;
                    break;
                case SelectionState.Pressed:
                    color = _HighlightedColor;
                    break;
                case SelectionState.Selected:
                    color = _NormalColor;
                    break;
                default:
                case SelectionState.Normal:
                    color = _NormalColor;
                    break;
            }

            _Text.color = color;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SamplesMenuButton), true)]
    [CanEditMultipleObjects]
    public class SamplesMenuButtonEditor: ButtonEditor
    {
        SerializedProperty _textProperty;

        SerializedProperty _normalColorProperty;

        SerializedProperty _highlightedColorProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            _textProperty = serializedObject.FindProperty("_Text");
            _normalColorProperty = serializedObject.FindProperty("_NormalColor");
            _highlightedColorProperty = serializedObject.FindProperty("_HighlightedColor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_textProperty);
            EditorGUILayout.PropertyField(_normalColorProperty);
            EditorGUILayout.PropertyField(_highlightedColorProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
