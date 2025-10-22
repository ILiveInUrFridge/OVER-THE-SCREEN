using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UIButtonHoverPolish : 
    MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ISubmitHandler
{
    [Header("Selection")]
    public bool selectionKeepsHover = false;   // ⬅️ new: default false
    public bool playPressedBump = true;        // optional little press anim
    [Range(0.8f, 1f)] public float pressedScale = 0.97f;
    [Range(0.05f, 0.25f)] public float pressedDuration = 0.08f;

    public enum StickyMode { BothAxes, XOnly, YOnly, DirectionalBySlide } // extend toward slideOffset sign(s)

    [Header("Slide")]
    public Vector2 slideOffset = new Vector2(20f, 0f);
    [Range(0.05f, 0.5f)] public float duration = 0.18f;
    public Ease ease = Ease.OutQuad;
    public bool overshoot = true;
    [Range(0f, 0.5f)] public float overshootPct = 0.2f;

    [Header("Polish")]
    public bool useScale = true;
    [Range(1f, 1.1f)] public float hoverScale = 1.03f;
    public bool useTilt = true;
    [Range(0f, 10f)] public float hoverZRotation = 3f;

    [Header("Sticky Hover")]
    [Tooltip("Pixels to extend as a hover buffer.")]
    public float stickyPadding = 16f;
    public StickyMode stickyMode = StickyMode.DirectionalBySlide;
    [Tooltip("Delay before hover-in/out to prevent flicker.")]
    public float enterIntentDelay = 0.06f, exitGraceSeconds = 0.08f;
    [Tooltip("Require this element to be topmost under cursor to count as hovered.")]
    public bool requireTopmost = true;

    RectTransform _rt;
    Canvas _canvas;
    Camera _uiCam;
    GraphicRaycaster _raycaster;
    Vector2 _basePos;

    Sequence _seq;
    bool _pointerInside;
    bool _hoverPlaying;
    bool _isSelected;
    float _enterTimer, _exitTimer;

    static readonly List<RaycastResult> _raycastBuffer = new List<RaycastResult>(16);

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _basePos = _rt.anchoredPosition;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _uiCam = _canvas.worldCamera;

        _raycaster = _canvas ? _canvas.GetComponent<GraphicRaycaster>() : null;
    }

    void OnDisable()
    {
        _seq?.Kill(); _seq = null;
        _hoverPlaying = false;
        _pointerInside = false;
        _isSelected = false;

        _rt.anchoredPosition = _basePos;
        _rt.localScale = Vector3.one;
        _rt.localRotation = Quaternion.identity;
    }

    public void OnPointerEnter(PointerEventData _) { _pointerInside = true;  _enterTimer = 0f; _exitTimer = 0f; }
    public void OnPointerExit (PointerEventData _) { _pointerInside = false; /* sticky handled in Update */ }
    public void OnSelect(BaseEventData _)   { _isSelected = true; if (selectionKeepsHover) PlayHoverIn(immediate:true); }
    public void OnDeselect(BaseEventData _) { _isSelected = false; /* Update() handles exit */ }
    
    public void OnPointerDown(PointerEventData _) { if (playPressedBump) PressBumpIn(); }
    public void OnPointerUp  (PointerEventData _) { if (playPressedBump) PressBumpOut(); }

    void PressBumpIn()
    {
        _seq?.Kill(false);
        float target = pressedScale;
        DOTween.Kill(_rt, complete:false);
        _rt.DOScale(target, pressedDuration).SetUpdate(true).SetEase(Ease.OutQuad);
    }

    void PressBumpOut()
    {
        float target = _hoverPlaying ? hoverScale : 1f;
        _rt.DOScale(target, pressedDuration).SetUpdate(true).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (!selectionKeepsHover)
        {
            // Clear selection so hover doesn't stick after click
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
            _isSelected = false;

            // If cursor isn’t in sticky zone, exit after the click
            if (!IsPointerInStickyZoneDirectional()) PlayHoverOut();
        }
    }

    public void OnSubmit(BaseEventData _)
    {
        // Controller/keyboard “press”
        if (playPressedBump) DOTween.Sequence().SetUpdate(true)
            .Append(_rt.DOScale(pressedScale, pressedDuration).SetEase(Ease.OutQuad))
            .Append(_rt.DOScale(_hoverPlaying ? hoverScale : 1f, pressedDuration).SetEase(Ease.OutQuad));
    }

    void Update()
    {
        bool insideSticky = _isSelected || IsPointerInStickyZoneDirectional();

        if (insideSticky && (!requireTopmost || IsTopmostUnderPointer()))
        {
            _exitTimer = 0f;
            if (!_hoverPlaying)
            {
                _enterTimer += Time.unscaledDeltaTime;
                if (_enterTimer >= enterIntentDelay) PlayHoverIn();
            }
        }
        else
        {
            _enterTimer = 0f;
            if (_hoverPlaying && !_isSelected)
            {
                _exitTimer += Time.unscaledDeltaTime;
                if (_exitTimer >= exitGraceSeconds) PlayHoverOut();
            }
        }
    }

    bool IsPointerInStickyZoneDirectional()
    {
        // Convert rect to screen space
        Vector3[] corners = new Vector3[4];
        _rt.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(_uiCam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(_uiCam, corners[2]);

        float leftPad = 0, rightPad = 0, bottomPad = 0, topPad = 0;

        switch (stickyMode)
        {
            case StickyMode.BothAxes:
                leftPad = rightPad = bottomPad = topPad = stickyPadding; break;

            case StickyMode.XOnly:
                leftPad = rightPad = stickyPadding; break;

            case StickyMode.YOnly:
                bottomPad = topPad = stickyPadding; break;

            case StickyMode.DirectionalBySlide:
                if (slideOffset.x > 0) rightPad = stickyPadding;
                else if (slideOffset.x < 0) leftPad = stickyPadding;
                if (slideOffset.y > 0) topPad = stickyPadding;
                else if (slideOffset.y < 0) bottomPad = stickyPadding;
                break;
        }

        Rect r = Rect.MinMaxRect(min.x - leftPad, min.y - bottomPad,
                                 max.x + rightPad, max.y + topPad);

        return r.Contains(Input.mousePosition);
    }

    bool IsTopmostUnderPointer()
    {
        if (EventSystem.current == null) return true;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _raycastBuffer.Clear();

        // Use global raycast so results come from ALL canvases in correct z/sorting order
        EventSystem.current.RaycastAll(ped, _raycastBuffer);

        if (_raycastBuffer.Count == 0) return false;

        // First result is the true topmost UI under the cursor
        var top = _raycastBuffer[0].gameObject.transform;
        return top == transform || top.IsChildOf(transform);
    }

    void PlayHoverIn(bool immediate = false)
    {
        _hoverPlaying = true;
        _seq?.Kill();
        _seq = DOTween.Sequence().SetUpdate(true);

        Vector2 final = _basePos + slideOffset;
        if (overshoot)
        {
            Vector2 over = _basePos + slideOffset * (1f + Mathf.Clamp01(overshootPct));
            _seq.Append(_rt.DOAnchorPos(over, duration * 0.7f).SetEase(ease));
            _seq.Append(_rt.DOAnchorPos(final, duration * 0.3f).SetEase(Ease.OutSine));
        }
        else _seq.Append(_rt.DOAnchorPos(final, duration).SetEase(ease));

        if (useScale) _seq.Join(_rt.DOScale(hoverScale, duration * 0.8f).SetEase(Ease.OutQuad));

        if (useTilt)
        {
            float z = hoverZRotation;
            if (Mathf.Approximately(z, 0f) && slideOffset != Vector2.zero)
                z = Mathf.Sign(slideOffset.x != 0 ? slideOffset.x : slideOffset.y) * 2.5f;

            _seq.Join(_rt.DOLocalRotate(new Vector3(0, 0, z), duration * 0.8f).SetEase(Ease.OutQuad));
        }

        if (immediate) _seq.Complete(true);
    }

    void PlayHoverOut()
    {
        _hoverPlaying = false;
        _seq?.Kill();
        _seq = DOTween.Sequence().SetUpdate(true);

        _seq.Append(_rt.DOAnchorPos(_basePos, duration).SetEase(Ease.InOutQuad));
        if (useScale) _seq.Join(_rt.DOScale(1f, duration).SetEase(Ease.InOutQuad));
        if (useTilt)  _seq.Join(_rt.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.InOutQuad));
    }
}