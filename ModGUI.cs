using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using MelonLoader;

namespace Kebab_Mod_Menu_Count
{
    public static class ModGUI
    {
        // ==================================================================
        // ---------------------------- CONSTANTS --------------------------
        // Everything you might want to change manually is here.
        // ==================================================================

        // ---- Texts ----
        private const string PanelTitle = "MOD SETTINGS";
        private const string MenuToggleLabel = "Unlimited menu items";
        private const string MenuInputLabel = "Menu limit (if not unlimited)";
        private const string HandToggleLabel = "Unlimited hand items";
        private const string HandInputLabel = "Hand limit (if not unlimited)";
        private const string ApplyButtonLabel = "Apply";
        private const string AppliedStatusText =
            "Applied! Return to main menu and re-enter to apply the hand item limit.";

        // ---- Canvas / Scaler ----
        private const int CanvasSortingOrder = 1000;
        private const float ReferenceResolutionWidth = 1920f;
        private const float ReferenceResolutionHeight = 1080f;

        // ---- Panel ----
        private const float PanelWidth = 380f;
        private const float PanelOffsetX = -24f;
        private const float PanelOffsetY = -24f;
        private const int PanelPaddingLeft = 20;
        private const int PanelPaddingRight = 20;
        private const int PanelPaddingTop = 18;
        private const int PanelPaddingBottom = 18;
        private const float PanelSpacing = 14f;
        private const int PanelCornerRadius = 18;

        // ---- Fonts ----
        private const int TitleFontSize = 16;
        private const int LabelFontSize = 14;
        private const int MutedLabelFontSize = 12;
        private const int ButtonFontSize = 15;

        // ---- Divider ----
        private const float DividerHeight = 2f;
        private const float DividerAlpha = 0.35f;

        // ---- Checkbox ----
        private const float CheckboxSize = 20f;
        private const int CheckboxCornerRadius = 6;
        private const int CheckmarkCornerRadius = 4;
        private const float ToggleRowSpacing = 10f;

        // ---- Inputs ----
        private const float InputFieldHeight = 32f;
        private const int InputFieldCornerRadius = 8;
        private const float InputGroupSpacing = 4f;

        // ---- Button ----
        private const float ButtonWidth = 140f;
        private const float ButtonHeight = 38f;
        private const int ButtonCornerRadius = 10;

        // ---- Status text ----
        private const float StatusMinHeight = 32f;

        // ---- Sprite generator ----
        private const int RoundedSpriteTextureSize = 64;

        // ---------- Color palette ----------
        private static readonly Color PanelBg = new Color(0.11f, 0.12f, 0.15f, 0.95f);
        private static readonly Color Accent = new Color32(0x25, 0xAD, 0xE4, 0xFF);
        private static readonly Color FieldBg = new Color(0.20f, 0.21f, 0.25f);
        private static readonly Color ButtonBg = new Color32(0x25, 0xAD, 0xE4, 0xFF);
        private static readonly Color TextMain = Color.white;
        private static readonly Color TextMuted = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color ButtonTextColor = new Color(0.05f, 0.05f, 0.05f);

        // ==================================================================

        private static GameObject _canvasRoot;
        private static InputField _menuInput;
        private static InputField _handInput;
        private static Toggle _menuToggle;
        private static Toggle _handToggle;
        private static Text _statusText;
        private static bool _isOpen;
        private static Font _defaultFont;

        private static readonly Dictionary<Color, Sprite> _spriteCache = new Dictionary<Color, Sprite>();

        public static void Build()
        {
            _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureEventSystem();

            _canvasRoot = new GameObject("KebabModGUI_Canvas");
            GameObject.DontDestroyOnLoad(_canvasRoot);

            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;

            var scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceResolutionWidth, ReferenceResolutionHeight);

            _canvasRoot.AddComponent<GraphicRaycaster>();

            BuildPanel(_canvasRoot.transform);

            LoadCurrentValues();
            _canvasRoot.SetActive(false);
        }

        private static void BuildPanel(Transform canvasTransform)
        {
            // Root panel — screen corner (top right)
            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasTransform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(PanelOffsetX, PanelOffsetY);
            panelRect.sizeDelta = new Vector2(PanelWidth, 0);

            var panelImg = panel.AddComponent<Image>();
            panelImg.sprite = GetRoundedSprite(PanelBg, PanelCornerRadius);
            panelImg.type = Image.Type.Sliced;

            var panelLayout = panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(PanelPaddingLeft, PanelPaddingRight, PanelPaddingTop, PanelPaddingBottom);
            panelLayout.spacing = PanelSpacing;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            var panelFitter = panel.AddComponent<ContentSizeFitter>();
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            CreateLabel(panel, PanelTitle, TitleFontSize, Accent, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Thin divider
            var divider = new GameObject("Divider");
            divider.transform.SetParent(panel.transform, false);
            var divRect = divider.AddComponent<RectTransform>();
            var divLayout = divider.AddComponent<LayoutElement>();
            divLayout.preferredHeight = DividerHeight;
            var divImg = divider.AddComponent<Image>();
            divImg.color = new Color(Accent.r, Accent.g, Accent.b, DividerAlpha);

            // "Menu" section
            _menuToggle = CreateToggleRow(panel, MenuToggleLabel);
            _menuInput = CreateLabeledInput(panel, MenuInputLabel);

            // "Hand" section
            _handToggle = CreateToggleRow(panel, HandToggleLabel);
            _handInput = CreateLabeledInput(panel, HandInputLabel);

            // Apply button
            CreateButtonRow(panel, ApplyButtonLabel, Apply);

            // Status after Apply
            _statusText = CreateLabel(panel, "", MutedLabelFontSize, TextMuted, TextAnchor.MiddleCenter, FontStyle.Italic);
            var statusLayout = _statusText.gameObject.AddComponent<LayoutElement>();
            statusLayout.minHeight = StatusMinHeight;
        }

        private static void EnsureEventSystem()
        {
            var existing = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (existing != null) return;

            var esGo = new GameObject("KebabModGUI_EventSystem");
            GameObject.DontDestroyOnLoad(esGo);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        public static void ToggleGUI()
        {
            _isOpen = !_isOpen;
            _canvasRoot.SetActive(_isOpen);

            if (_isOpen)
            {
                LoadCurrentValues();
                _statusText.text = "";
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private static void LoadCurrentValues()
        {
            _menuToggle.isOn = ModSettings.MenuUnlimited;
            _menuInput.text = ModSettings.MenuCustomValue.ToString();
            _handToggle.isOn = ModSettings.HandUnlimited;
            _handInput.text = ModSettings.HandCustomValue.ToString();
        }

        private static void Apply()
        {
            ModSettings.MenuUnlimited = _menuToggle.isOn;
            if (int.TryParse(_menuInput.text, out int menuVal))
                ModSettings.MenuCustomValue = Mathf.Max(1, menuVal);

            ModSettings.HandUnlimited = _handToggle.isOn;
            if (int.TryParse(_handInput.text, out int handVal))
                ModSettings.HandCustomValue = Mathf.Max(1, handVal);

            MelonLogger.Msg("[KebabMod] Applied. Menu=" + ModSettings.MaxMenuItems +
                             " Hand=" + ModSettings.MaxHandItems);

            _statusText.text = AppliedStatusText;
        }

        // ---------- Rounded sprite generator ----------

        private static Sprite GetRoundedSprite(Color color, int cornerRadius)
        {
            if (_spriteCache.TryGetValue(color, out var cached))
                return cached;

            const int size = RoundedSpriteTextureSize;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 1f;
                    int cx = -1, cy = -1;

                    if (x < cornerRadius && y < cornerRadius) { cx = cornerRadius; cy = cornerRadius; }
                    else if (x >= size - cornerRadius && y < cornerRadius) { cx = size - cornerRadius; cy = cornerRadius; }
                    else if (x < cornerRadius && y >= size - cornerRadius) { cx = cornerRadius; cy = size - cornerRadius; }
                    else if (x >= size - cornerRadius && y >= size - cornerRadius) { cx = size - cornerRadius; cy = size - cornerRadius; }

                    if (cx >= 0)
                    {
                        float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                        alpha = dist <= cornerRadius ? 1f : 0f;
                    }

                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
            }
            tex.Apply();

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius)
            );

            _spriteCache[color] = sprite;
            return sprite;
        }

        // ---------- UI factories ----------

        private static Text CreateLabel(GameObject parent, string text, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent.transform, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = _defaultFont;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = anchor;
            t.color = color;
            return t;
        }

        private static Toggle CreateToggleRow(GameObject parent, string label)
        {
            var row = new GameObject("ToggleRow_" + label);
            row.transform.SetParent(parent.transform, false);

            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = ToggleRowSpacing; // <- spacing between checkbox and text
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            // IMPORTANT: childControlWidth = true, otherwise the checkbox ignores LayoutElement
            // and renders with default RectTransform size (100x100) — causing the "large checkbox" issue.
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;

            // Checkbox
            var boxGo = new GameObject("Checkbox");
            boxGo.transform.SetParent(row.transform, false);
            var boxLayout = boxGo.AddComponent<LayoutElement>();
            boxLayout.preferredWidth = CheckboxSize;
            boxLayout.preferredHeight = CheckboxSize;
            boxLayout.minWidth = CheckboxSize;
            boxLayout.minHeight = CheckboxSize;
            boxLayout.flexibleWidth = 0;

            var boxImg = boxGo.AddComponent<Image>();
            boxImg.sprite = GetRoundedSprite(FieldBg, CheckboxCornerRadius);
            boxImg.type = Image.Type.Sliced;

            var toggle = boxGo.AddComponent<Toggle>();

            var checkGo = new GameObject("Checkmark");
            checkGo.transform.SetParent(boxGo.transform, false);
            var checkRect = checkGo.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.sprite = GetRoundedSprite(Accent, CheckmarkCornerRadius);
            checkImg.type = Image.Type.Sliced;

            toggle.targetGraphic = boxImg;
            toggle.graphic = checkImg;
            toggle.isOn = true;

            // Label text
            var labelText = CreateLabel(row, label, LabelFontSize, TextMain, TextAnchor.MiddleLeft);
            var labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;

            return toggle;
        }

        private static InputField CreateLabeledInput(GameObject parent, string labelText)
        {
            var group = new GameObject("InputGroup_" + labelText);
            group.transform.SetParent(parent.transform, false);
            var groupLayout = group.AddComponent<VerticalLayoutGroup>();
            groupLayout.spacing = InputGroupSpacing;
            groupLayout.childControlWidth = true;
            groupLayout.childForceExpandWidth = true;
            groupLayout.childControlHeight = true;

            CreateLabel(group, labelText, MutedLabelFontSize, TextMuted, TextAnchor.MiddleLeft);

            var fieldGo = new GameObject("InputField");
            fieldGo.transform.SetParent(group.transform, false);
            var fieldLayout = fieldGo.AddComponent<LayoutElement>();
            fieldLayout.preferredHeight = InputFieldHeight;

            var fieldImg = fieldGo.AddComponent<Image>();
            fieldImg.sprite = GetRoundedSprite(FieldBg, InputFieldCornerRadius);
            fieldImg.type = Image.Type.Sliced;

            var input = fieldGo.AddComponent<InputField>();
            input.contentType = InputField.ContentType.IntegerNumber;
            input.targetGraphic = fieldImg;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(fieldGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 4);
            textRect.offsetMax = new Vector2(-10, -4);
            var text = textGo.AddComponent<Text>();
            text.font = _defaultFont;
            text.fontSize = LabelFontSize;
            text.color = TextMain;
            text.alignment = TextAnchor.MiddleLeft;

            input.textComponent = text;

            return input;
        }

        private static void CreateButtonRow(GameObject parent, string label, Action onClick)
        {
            // Wrapper row: THIS is the one that stretches to the full panel width
            // (due to the parent's childForceExpandWidth), not the button itself.
            var rowGo = new GameObject("ButtonRow_" + label);
            rowGo.transform.SetParent(parent.transform, false);

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            // IMPORTANT: set both to false — otherwise the button inside will also stretch full width.
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            var rowSize = rowGo.AddComponent<LayoutElement>();
            rowSize.preferredHeight = ButtonHeight;

            // Button
            var buttonGo = new GameObject("Button_" + label);
            buttonGo.transform.SetParent(rowGo.transform, false);

            // Since rowLayout.childControlWidth = false, the button's size is determined
            // by its own RectTransform.sizeDelta — set it explicitly.
            var buttonRect = buttonGo.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var buttonLayout = buttonGo.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = ButtonWidth;
            buttonLayout.preferredHeight = ButtonHeight;
            buttonLayout.flexibleWidth = 0;

            var img = buttonGo.AddComponent<Image>();
            img.sprite = GetRoundedSprite(ButtonBg, ButtonCornerRadius);
            img.type = Image.Type.Sliced;

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick()));

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = _defaultFont;
            text.fontSize = ButtonFontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = ButtonTextColor;
            text.alignment = TextAnchor.MiddleCenter;
        }
    }
}