using System;
using System.Collections.Generic;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LethalHUD.CustomHUD;

internal static class CustomFrames
{
    private static Image[] _lastFrames;
    private static Image[] _lastIcons;
    private static HUDManager _hud;
    private static string _activeStyle;
    private static bool _isApplying;
    private static readonly List<SlotVisual> _visuals = new();

    internal static void OnHUDEnable(HUDManager hud)
    {
        ReleaseVisuals();
        _hud = hud;
        _lastFrames = null;
        _lastIcons = null;
        _activeStyle = null;
    }

    internal static void OnHUDDisable(HUDManager hud)
    {
        if (!ReferenceEquals(_hud, hud)) return;
        ReleaseVisuals();
        _hud = null;
        _lastFrames = null;
        _lastIcons = null;
        _activeStyle = null;
    }

    internal static void Tick(HUDManager hud)
    {
        if (hud == null || !hud.isActiveAndEnabled) return;
        if (!ReferenceEquals(_hud, hud)) OnHUDEnable(hud);
        Apply(Plugins.ConfigEntries.CustomInventoryFrames.Value);
        foreach (SlotVisual visual in _visuals) visual.Sync();
    }

    internal static void Apply(string style)
    {
        if (_isApplying) return;
        HUDManager hud = HUDManager.Instance;
        if (hud == null || !hud.isActiveAndEnabled) return;
        if (!ReferenceEquals(_hud, hud)) OnHUDEnable(hud);

        Image[] frames = hud.itemSlotIconFrames;
        Image[] icons = hud.itemSlotIcons;
        bool topologyChanged = !SameImages(_lastFrames, frames) || !SameImages(_lastIcons, icons);
        if (!topologyChanged && _activeStyle == style) return;

        _isApplying = true;
        try
        {
            ReleaseVisuals();
            _lastFrames = frames == null ? null : (Image[])frames.Clone();
            _lastIcons = icons == null ? null : (Image[])icons.Clone();
            _activeStyle = style;

            if (style != "Default" && frames != null && icons != null)
            {
                if (Plugins.SlotPrefabs.TryGetValue(style, out var entry) && entry.Asset != null)
                {
                    LHSlotRefs template = entry.Asset.GetComponent<LHSlotRefs>();
                    if (template == null || template.frame == null || template.icon == null)
                        Loggers.Warning($"CustomFrames: style '{style}' needs LHSlotRefs.frame and icon; retaining the original HUD.");
                    else
                    {
                        int count = Math.Min(frames.Length, icons.Length);
                        for (int i = 0; i < count; i++)
                        {
                            if (frames[i] == null || icons[i] == null || frames[i] == icons[i]) continue;

                            if (frames[i].GetComponent<Mask>() != null || icons[i].GetComponent<Mask>() != null) continue;
                            SlotVisual visual = new(frames[i], icons[i]);
                            _visuals.Add(visual);
                            visual.Build(entry.Asset, i);
                        }
                    }
                }
                else
                    Loggers.Warning($"CustomFrames: style '{style}' is unavailable; retaining the original HUD.");
            }

            if (topologyChanged)
            {
                InventoryFrames.ResetCache();
                ScrapValueDisplay.RefreshSlots();
            }
        }
        catch (Exception ex)
        {
            ReleaseVisuals();
            Loggers.Warning($"CustomFrames: could not apply '{style}'; retaining the original HUD. {ex}");
        }
        finally
        {
            _isApplying = false;
        }
    }

    private static bool SameImages(Image[] previous, Image[] current)
    {
        if (previous == null || current == null) return ReferenceEquals(previous, current);
        if (previous.Length != current.Length) return false;
        for (int i = 0; i < current.Length; i++)
            if (!ReferenceEquals(previous[i], current[i])) return false;
        return true;
    }

    private static void ReleaseVisuals()
    {
        foreach (SlotVisual visual in _visuals) visual.Dispose();
        _visuals.Clear();
    }

    internal static void UpdateShaderColor()
    {
        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow);
        foreach (SlotVisual visual in _visuals) visual.SetShaderColor(color);
    }

    internal static void ForwardSlotBool(Animator source, string name, bool value)
    {
        int hash = Animator.StringToHash(name);
        foreach (SlotVisual visual in _visuals) visual.SetBool(source, hash, value);
    }

    internal static void ForwardSlotTrigger(Animator source, string name, bool reset)
    {
        int hash = Animator.StringToHash(name);
        foreach (SlotVisual visual in _visuals) visual.SetTrigger(source, hash, reset);
    }

    private sealed class SlotVisual
    {
        private readonly Image _sourceFrame;
        private readonly Image _sourceIcon;
        private readonly List<LHSlotMeshSuppressor> _suppressors = new();
        private GameObject _root;
        private RectTransform _transform;
        private LHSlotRefs _refs;
        private Material _material;
        private Animator _sourceAnimator;
        private bool _sourceHasSelectedSlot;
        private readonly List<SlotAnimator> _animators = new();
        private static readonly int SelectedSlotHash = Animator.StringToHash("selectedSlot");
        private bool _hasSynced;
        private Color _lastFrameColor;
        private Color _lastIconColor;
        private Sprite _lastIconSprite;
        private bool _lastIconEnabled;
        private bool _lastPreserveAspect;

        internal SlotVisual(Image frame, Image icon)
        {
            _sourceFrame = frame;
            _sourceIcon = icon;
        }

        internal void Build(GameObject prefab, int index)
        {
            _root = new GameObject($"LethalHUD_SlotVisual_{index}", typeof(RectTransform), typeof(LayoutElement));
            _root.GetComponent<LayoutElement>().ignoreLayout = true;
            _transform = (RectTransform)_root.transform;
            _transform.SetParent(_sourceFrame.transform.parent, false);
            GameObject slot = Object.Instantiate(prefab, _transform);
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localScale = Vector3.one;
            slot.SetActive(true);
            _refs = slot.GetComponent<LHSlotRefs>();
            if (_refs == null || _refs.frame == null || _refs.icon == null)
                throw new InvalidOperationException("Instantiated slot is missing its frame or icon.");

            _refs.BindSourceIcon(_sourceIcon);
            _sourceAnimator = _sourceFrame.GetComponent<Animator>();
            if (_sourceAnimator != null && _sourceAnimator.runtimeAnimatorController != null)
            {
                foreach (AnimatorControllerParameter parameter in _sourceAnimator.parameters)
                    if (parameter.nameHash == SelectedSlotHash && parameter.type == AnimatorControllerParameterType.Bool)
                        _sourceHasSelectedSlot = true;
            }

            foreach (Animator animator in slot.GetComponentsInChildren<Animator>(true))
                if (animator.runtimeAnimatorController != null) _animators.Add(new SlotAnimator(animator));

            foreach (Graphic graphic in slot.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            if (_refs.frame.material != null)
            {
                _material = Object.Instantiate(_refs.frame.material);
                _refs.frame.material = _material;
                SetShaderColor(HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow));
            }

            Sync();
            Suppress(_sourceFrame);
            Suppress(_sourceIcon);
            Transform fade = _sourceFrame.transform.Find("fadeIcon");
            if (fade != null && fade.TryGetComponent(out Image fadeImage) && fadeImage.GetComponent<Mask>() == null)
                Suppress(fadeImage);
        }

        private void Suppress(Image image)
        {
            LHSlotMeshSuppressor suppressor = image.GetComponent<LHSlotMeshSuppressor>();
            if (suppressor == null) suppressor = image.gameObject.AddComponent<LHSlotMeshSuppressor>();
            if (_suppressors.Contains(suppressor)) return;
            _suppressors.Add(suppressor);
            suppressor.SetSuppressed(true);
        }

        internal void Sync()
        {
            if (_root == null) return;
            if (_sourceFrame == null || _sourceIcon == null || _refs == null)
            {
                Dispose();
                return;
            }

            RectTransform source = _sourceFrame.rectTransform;
            if (_transform.parent != source.parent) _transform.SetParent(source.parent, false);
            _transform.anchorMin = source.anchorMin;
            _transform.anchorMax = source.anchorMax;
            _transform.pivot = source.pivot;
            _transform.sizeDelta = source.sizeDelta;
            _transform.localPosition = source.localPosition;
            _transform.localRotation = Quaternion.identity;
            _transform.localScale = Vector3.one;

            bool visible = _sourceFrame.isActiveAndEnabled;
            if (_root.activeSelf != visible) _root.SetActive(visible);

            Color frameColor = _sourceFrame.color;
            if (!_hasSynced || !SameRGB(frameColor, _lastFrameColor))
                _refs.frame.color = new Color(frameColor.r, frameColor.g, frameColor.b, _refs.frame.color.a);
            _lastFrameColor = frameColor;

            Sprite iconSprite = _sourceIcon.overrideSprite;
            bool iconEnabled = _sourceIcon.enabled && _sourceIcon.gameObject.activeInHierarchy;
            Color iconColor = _sourceIcon.color;
            if (!_hasSynced || iconSprite != _lastIconSprite) _refs.icon.sprite = iconSprite;
            if (!_hasSynced || iconEnabled != _lastIconEnabled) _refs.icon.enabled = iconEnabled;
            if (!_hasSynced || !SameRGB(iconColor, _lastIconColor))
                _refs.icon.color = new Color(iconColor.r, iconColor.g, iconColor.b, _refs.icon.color.a);
            if (!_hasSynced || _sourceIcon.preserveAspect != _lastPreserveAspect)
                _refs.icon.preserveAspect = _sourceIcon.preserveAspect;
            _lastIconSprite = iconSprite;
            _lastIconEnabled = iconEnabled;
            _lastIconColor = iconColor;
            _lastPreserveAspect = _sourceIcon.preserveAspect;
            _hasSynced = true;

            if (_sourceAnimator != null && _sourceHasSelectedSlot)
                SetBool(_sourceAnimator, SelectedSlotHash, _sourceAnimator.GetBool(SelectedSlotHash));
            _refs.RefreshVisuals();
        }

        private static bool SameRGB(Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b;

        internal void SetBool(Animator source, int hash, bool value)
        {
            if (source == null || source != _sourceAnimator) return;
            foreach (SlotAnimator target in _animators)
                if (target.HasParameter(hash, AnimatorControllerParameterType.Bool)) target.Animator.SetBool(hash, value);
        }

        internal void SetTrigger(Animator source, int hash, bool reset)
        {
            if (source == null || source != _sourceAnimator) return;
            foreach (SlotAnimator target in _animators)
            {
                if (!target.HasParameter(hash, AnimatorControllerParameterType.Trigger)) continue;
                if (reset) target.Animator.ResetTrigger(hash);
                else target.Animator.SetTrigger(hash);
            }
        }

        private sealed class SlotAnimator
        {
            internal readonly Animator Animator;
            private readonly AnimatorControllerParameter[] _parameters;

            internal SlotAnimator(Animator animator)
            {
                Animator = animator;
                _parameters = animator.parameters;
            }

            internal bool HasParameter(int hash, AnimatorControllerParameterType type)
            {
                if (Animator == null) return false;
                foreach (AnimatorControllerParameter parameter in _parameters)
                    if (parameter.nameHash == hash && parameter.type == type) return true;
                return false;
            }
        }

        internal void SetShaderColor(Color color)
        {
            if (_material != null && _material.HasProperty("_FlowColor"))
                _material.SetColor("_FlowColor", color);
        }

        internal void Dispose()
        {
            _animators.Clear();
            foreach (LHSlotMeshSuppressor suppressor in _suppressors)
                if (suppressor != null) suppressor.SetSuppressed(false);
            _suppressors.Clear();
            if (_root != null)
            {
                _root.SetActive(false);
                Object.Destroy(_root);
                _root = null;
            }
            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }
    }
}