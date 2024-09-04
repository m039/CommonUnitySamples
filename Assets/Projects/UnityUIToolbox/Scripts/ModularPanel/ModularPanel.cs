using m039.Common.DependencyInjection;
using m039.UIToolbox.Adaptive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ModularPanel : MonoBehaviour, IDependencyProvider
    {
        static readonly Dictionary<Type, IPanelItemCreator> s_Creators = new()
        {
            { typeof(ToggleItem), new ToggleItemCreator() },
            { typeof(DropdownItem), new DropdownItemCreator() },
            { typeof(DropdownEnumItem), new DropdownEnumItemCreator() },
            { typeof(SliderItem), new SliderItemCreator() },
            { typeof(ButtonItem), new ButtonItemCreator() }
        };

        [Provide]
        ModularPanel GetModularPanel() => this;

        readonly List<PanelItem> _items = new();

        bool _init = false;

        readonly Dictionary<Type, Transform> _templates = new();

        readonly Dictionary<PanelItem, Transform> _transforms = new();

        Transform _content;

        Transform _handle;

        AdaptiveView _handleView;

        Transform _padding;

        bool _visibility;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            if (_init)
                return;

            _init = true;

            var templates = new (Type, string)[] {
                (typeof(ToggleItem), "ToggleTemplate"),
                (typeof(DropdownItem), "DropdownTemplate"),
                (typeof(DropdownEnumItem), "DropdownTemplate"),
                (typeof(SliderItem), "SliderTemplate"),
                (typeof(ButtonItem), "ButtonTemplate")
            };

            _content = transform.Find("Root/Body/Content");
            
            foreach (var (type, path) in templates)
            {
                var tr = _content.Find(path);
                if (tr != null)
                {
                    tr.gameObject.SetActive(false);
                    _templates.Add(type, tr);
                }
            }

            // Find Arrow Button.
            _handle = transform.Find("Root/Handle");

            _handleView = _handle.GetComponent<AdaptiveView>();
            _handleView.onAttach += (c) => {
                UpdateHandle();
            };
            
            if (_handle != null)
            {
                var button = _handle.GetComponent<Button>();
                UpdateHandle();

                button.onClick.AddListener(() =>
                {
                    SetVisibility(!_visibility);
                });
            }

            _padding = transform.Find("Root/Padding/Space");

            Close();
        }

        public void Open()
        {
            SetVisibility(true);
        }

        public void Close()
        {
            SetVisibility(false);
        }

        void SetVisibility(bool visible)
        {
            Init();
            _content.gameObject.SetActive(visible);
            _padding.gameObject.SetActive(visible);
            UpdateHandle();
            _visibility = visible;
        }

        void UpdateHandle()
        {
            var image = _handle.Find("Image");

            string label = null;

            if (_handleView.container != null)
            {
                label = _handleView.container.label;
            }

            if (label == "down_to_up")
            {
                if (_content.gameObject.activeSelf)
                {
                    image.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    image.rotation = Quaternion.Euler(0, 0, 180);
                }
            } else
            {
                // Left to Right
                if (_content.gameObject.activeSelf)
                {
                    image.rotation = Quaternion.Euler(0, 0, -90);
                }
                else
                {
                    image.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }

        public Builder CreateBuilder()
        {
            return new Builder(this);
        }

        void Update()
        {
            foreach (var p in _transforms)
            {
                if (p.Key.dirty)
                {
                    p.Value.gameObject.SetActive(p.Key.visible);
                    p.Key.dirty = false;
                }
            }
        }

        void Build()
        {
            Init();

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var type = item.GetType();
                if (!_templates.ContainsKey(type))
                {
                    Debug.LogError($"Can't find the template for {item}");
                    continue;
                }

                if (!s_Creators.ContainsKey(type))
                {
                    Debug.LogError($"Can't find the creator for {item}");
                    continue;
                }

                var creator = s_Creators[type];
                var template = _templates[type];

                var instance = creator.Create(item, template);
                instance.SetParent(_content, false);
                instance.gameObject.SetActive(true);

                _transforms.Add(item, instance);

                item.Reset();
            }
        }

        interface IPanelItemCreator
        {
            Transform Create(PanelItem item, Transform template);
        }

        interface IPanelItemCreator<T> : IPanelItemCreator where T : PanelItem
        {
            Transform Create(T item, Transform template);
        }

        class ToggleItemCreator : IPanelItemCreator<ToggleItem>
        {
            public Transform Create(ToggleItem item, Transform template)
            {
                var instance = Instantiate(template);
                var label = instance.Find("Label").GetComponent<TMPro.TMP_Text>();
                label.text = item.label;

                var toggle = instance.GetComponent<Toggle>();
                toggle.isOn = item.value;
                toggle.onValueChanged.AddListener((v) => item.onValueChanged?.Invoke(v));
                return instance;
            }

            public Transform Create(PanelItem panelItem, Transform template)
            {
                return Create((ToggleItem)panelItem, template);
            }
        }

        class DropdownItemCreator : IPanelItemCreator<DropdownItem>
        {
            public Transform Create(DropdownItem item, Transform template)
            {
                var instance = Instantiate(template);
                var label = instance.Find("Label").GetComponent<TMPro.TMP_Text>();
                label.text = item.label;
                var dropdown = instance.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();

                dropdown.ClearOptions();
                var options = new List<TMPro.TMP_Dropdown.OptionData>();
                foreach (var o in item.options)
                {
                    options.Add(new TMPro.TMP_Dropdown.OptionData
                    {
                        text = o
                    });
                }
                dropdown.AddOptions(options);
                dropdown.value = item.value;

                dropdown.onValueChanged.AddListener(v => item.onValueChanged?.Invoke(v));
                return instance;
            }

            public Transform Create(PanelItem panelItem, Transform template)
            {
                return Create((DropdownItem)panelItem, template);
            }
        }

        class DropdownEnumItemCreator : IPanelItemCreator<DropdownEnumItem>
        {
            public Transform Create(DropdownEnumItem item, Transform template)
            {
                var instance = Instantiate(template);

                var label = instance.Find("Label").GetComponent<TMPro.TMP_Text>();
                label.text = item.label;
                var dropdown = instance.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();

                dropdown.ClearOptions();
                var options = new List<TMPro.TMP_Dropdown.OptionData>();
                var enums = new List<int>();
                var value = -1;
                var i = 0;
                foreach (var e in Enum.GetValues(item.type))
                {
                    options.Add(new TMPro.TMP_Dropdown.OptionData
                    {
                        text = SplitCamelCase(e.ToString())
                    });
                    enums.Add((int)e);
                    if ((int)e == item.value)
                    {
                        value = i;
                    }
                    i++;
                }
                dropdown.AddOptions(options);
                dropdown.value = value;

                dropdown.onValueChanged.AddListener(v => item.onValueChanged?.Invoke(enums[v]));
                return instance;
            }

            public Transform Create(PanelItem panelItem, Transform template)
            {
                return Create((DropdownEnumItem)panelItem, template);
            }
        }

        class SliderItemCreator : IPanelItemCreator<SliderItem>
        {
            public Transform Create(SliderItem item, Transform template)
            {
                var instance = Instantiate(template);
                var label = instance.Find("Label").GetComponent<TMPro.TMP_Text>();
                label.text = item.label;

                var slider = instance.Find("Slider").GetComponent<Slider>();
                slider.minValue = item.min;
                slider.maxValue = item.max;
                slider.value = item.value;

                var value = instance.Find("Value").GetComponent<TMPro.TMP_Text>();
                value.text = item.value.ToString(item.valueFormat);

                slider.onValueChanged.AddListener(v =>
                {
                    value.text = v.ToString(item.valueFormat);
                    item.onValueChanged?.Invoke(v);
                });

                return instance;
            }

            public Transform Create(PanelItem panelItem, Transform template)
            {
                return Create((SliderItem)panelItem, template);
            }
        }

        class ButtonItemCreator : IPanelItemCreator<ButtonItem>
        {
            public Transform Create(ButtonItem item, Transform template)
            {
                var instance = Instantiate(template);
                var label = instance.Find("Text").GetComponent<TMPro.TMP_Text>();
                label.text = item.label;

                var button = instance.GetComponent<Button>();
                button.onClick.AddListener(() => item.onClick?.Invoke());

                return instance;
            }

            public Transform Create(PanelItem panelItem, Transform template)
            {
                return Create((ButtonItem)panelItem, template);
            }
        }

        public abstract class PanelItem {
            bool _visible;

            public bool visible
            {
                get => _visible;
                set
                {
                    _visible = value;
                    dirty = true;
                }
            }

            public bool dirty = false;

            public abstract void Reset();
        }

        public class ToggleItem : PanelItem
        {
            public bool value;

            public Action<bool> onValueChanged;

            public string label;

            public ToggleItem(bool value = false, string label = null)
            {
                this.value = value;
                this.label = label;
            }

            public override void Reset()
            {
                onValueChanged?.Invoke(value);
            }
        }

        public class DropdownItem : PanelItem
        {
            public int value = 0;
            public List<string> options = new();
            public Action<int> onValueChanged;
            public string label;

            public DropdownItem(int value = 0, string label = null)
            {
                this.value = value;
                this.label = label;
            }

            public override void Reset()
            {
                onValueChanged?.Invoke(value);
            }
        }

        public class DropdownEnumItem : PanelItem
        {
            public int value = 0;

            public Type type;

            public Action<int> onValueChanged;

            public string label;

            public DropdownEnumItem(Type type, string label = null)
            {
                this.type = type;
                this.label = label;
            }

            public override void Reset()
            {
                onValueChanged?.Invoke(value);
            }
        }

        public class SliderItem : PanelItem
        {
            public float value = 0f;

            public float min = 0f;

            public float max = 1f;

            public string label;

            public Action<float> onValueChanged;

            public string valueFormat = "0.0";

            public SliderItem(float value, float min = 0, float max = 1)
            {
                this.value = value;
                this.min = min;
                this.max = max;
            }

            public override void Reset()
            {
                onValueChanged?.Invoke(value);
            }
        }

        public class ButtonItem : PanelItem
        {
            public string label;

            public Action onClick;

            public ButtonItem(string label = null)
            {
                this.label = label;
            }

            public override void Reset()
            {
            }
        }

        public class Builder
        {
            readonly ModularPanel _panel;

            internal Builder(ModularPanel panel)
            {
                _panel = panel;
            }

            public Builder AddItem(PanelItem item)
            {
                _panel._items.Add(item);
                return this;
            }

            public void Build()
            {
                _panel.Build();
            }
        }

        public static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}
