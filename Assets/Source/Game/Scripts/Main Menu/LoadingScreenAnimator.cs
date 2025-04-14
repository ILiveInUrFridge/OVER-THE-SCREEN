using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Game.Audio;

public class LoadingScreenAnimator : MonoBehaviour
{
    [Header("Stripe Scrolling")]
    public RectTransform stripeGroup;
    public float stripeScrollSpeed = 100f; // Adjust as needed
    
    [Header("Loading Bar Parent")]
    [Tooltip("The parent that has the black/white BG. We'll move this.")]
    public RectTransform loadingBarParent; 

    [Header("Fill Image (Child)")]
    [Tooltip("Child image with Filled type, used for the color portion.")]
    public Image logoMask; 
    public CanvasGroup logoCanvasGroup; // Optional fade/glow if attached to loadingBarParent

    [Header("Texts")]
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI copyrightText;

    [Header("Main Menu")]
    public GameObject mainMenuRoot;

    [Header("Debug Options")]
    [Tooltip("When checked, skips the loading animation in editor (has no effect in builds)")]
    [SerializeField] private bool skipInEditor = false;

    // Hard-coded 4K positions (Canvas center is (0,0)):
    // Positions for the entire "loadingBarParent":
    private Vector2 BAR_ONSCREEN_POS = new Vector2(0f, 250f); 
    private Vector2 BAR_START_POS    = new Vector2(0f, 1700f); // Off-screen above
    private Vector2 BAR_FINAL_CENTER = new Vector2(0f, 0f);    // If you want to move it to (0,0) at the end

    // Text on-screen positions
    private Vector2 TEXT_ONSCREEN_POS  = new Vector2(0f, -560f);
    private Vector2 COPYRIGHT_ONSCREEN = new Vector2(0f, -1000f);

    // Text off-screen below
    private Vector2 TEXT_START_POS = new Vector2(0f, -2200f);
    private Vector2 TEXT_EXIT_POS  = new Vector2(0f, -2200f);

    // Stripe exit ratio (x:y ratio for movement)
    private Vector2 WHITE_STRIPE_RATIO = new Vector2(4305f, 2477f).normalized;    // Up and right direction
    private Vector2 PINK_STRIPE_RATIO  = new Vector2(-4305f, -2477f).normalized;  // Down and left direction
    private float STRIPE_MOVE_DISTANCE = 6000f; // Doubled distance for stripe movement
    private float STRIPE_DELAY = 0.05f; // Delay between each stripe's movement

    // Timings
    private float barSlideDuration    = 0.8f;  // Move bar on screen
    private float textSlideDuration   = 0.5f;  // Move text on/off screen
    private float loadingFillDuration = 14.5f;  // 2.0→16.5 in timeline
    private float stripeExitDuration  = 0.4f; // Duration for stripe exit
    private float logoFadeDuration    = 0.2f; // Duration for logo fade
    private float logoGlowDuration    = 0.4f; // Duration for logo glow effect

    private void Start()
    {
        // Hide the main menu initially
        if (mainMenuRoot) mainMenuRoot.SetActive(false);

        // Check if we should skip in editor
        #if UNITY_EDITOR
        if (skipInEditor)
        {
            // Still need to play the menu music
            if (AudioManager.Music != null)
            {
                AudioManager.Music.Play("main_menu_bgm");
            }
            
            // Skip the animation and go straight to main menu
            if (mainMenuRoot) mainMenuRoot.SetActive(true);
            gameObject.SetActive(false);
            return;
        }
        #endif

        // Position items off-screen
        SetupInitialPositions();

        // Start our "fake loading" sequence
        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        // Indefinite horizontal scroll on the stripe group
        if (stripeGroup != null)
        {
            stripeGroup.anchoredPosition += Vector2.left * (stripeScrollSpeed * Time.unscaledDeltaTime);
        }
    }

    private void SetupInitialPositions()
    {
        // 1) Move the entire bar (black/white BG + child fill) off-screen
        loadingBarParent.anchoredPosition = BAR_START_POS;

        // 2) Fill starts at 0
        logoMask.fillAmount = 0f;

        // 3) Text off-screen
        loadingText.rectTransform.anchoredPosition = TEXT_START_POS;
        copyrightText.rectTransform.anchoredPosition = TEXT_START_POS;

        // 4) Reset canvas group alpha (if you have one on the parent)
        if (logoCanvasGroup) logoCanvasGroup.alpha = 1f;
    }

    IEnumerator PlayIntroSequence()
    {
        if (AudioManager.Music != null)
        {
            AudioManager.Music.Play("main_menu_bgm");
        }

        // 0.0s: Start
        yield return new WaitForSecondsRealtime(0.3f);
        // 0.3s: Stripes are scrolling in background

        // [0.3s→1.1s] Slide the bar onto screen
        loadingBarParent
            .DOAnchorPos(BAR_ONSCREEN_POS, barSlideDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Force update even when timescale is changed
        yield return new WaitForSecondsRealtime(barSlideDuration);
        // 1.1s: Bar is now on screen

        // [1.1s→1.6s] Text slides up
        loadingText.rectTransform
                   .DOAnchorPos(TEXT_ONSCREEN_POS, textSlideDuration)
                   .SetEase(Ease.OutSine)
                   .SetUpdate(true);

        copyrightText.rectTransform
                     .DOAnchorPos(COPYRIGHT_ONSCREEN, textSlideDuration)
                     .SetEase(Ease.OutSine)
                     .SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);
        // 1.6s: Text is now visible

        // [1.6s→2.0s] Small buffer
        yield return new WaitForSecondsRealtime(0.4f);
        // 2.0s: Begin filling the logo

        // [2.0s→15.5s] Fill the logo mask over 13.5s
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + loadingFillDuration)
        {
            float elapsed = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(elapsed / loadingFillDuration);

            // The child's fill
            logoMask.fillAmount = t;

            loadingText.text = $"LOADING - {Mathf.RoundToInt(t * 100)}%";
            yield return null;
        }

        // 15.5s: Loading complete
        logoMask.fillAmount = 1f;
        loadingText.text = "COMPLETE!";

        yield return new WaitForSecondsRealtime(0.5f);
        // 16.0s: Start moving bar to center

        // [16.0s→16.5s] Move the logo loading bar to center with easing
        loadingBarParent
            .DOAnchorPos(BAR_FINAL_CENTER, 0.5f)
            .SetEase(Ease.OutBack, 1.2f)
            .SetUpdate(true);

        // [16.0s→16.5s] Move texts off-screen with easing
        loadingText.rectTransform
                   .DOAnchorPos(TEXT_EXIT_POS, textSlideDuration)
                   .SetEase(Ease.InOutQuad)
                   .SetUpdate(true);
        copyrightText.rectTransform
                     .DOAnchorPos(TEXT_EXIT_POS, textSlideDuration)
                     .SetEase(Ease.InOutQuad)
                     .SetUpdate(true);
        yield return new WaitForSecondsRealtime(textSlideDuration);
        // 16.5s: Text is off-screen, bar is centered

        // [16.5s→17.5s] Wait for beat drop
        yield return new WaitForSecondsRealtime(1.0f);
        // 17.5s: Ready for beat drop

        // ========== BEAT DROP (17.5s) ==========
        // Reveal main menu immediately on beat drop
        if (mainMenuRoot) mainMenuRoot.SetActive(true);

        // Add CanvasGroup to loading bar parent if it doesn't have one
        CanvasGroup loadingBarCanvasGroup = loadingBarParent.GetComponent<CanvasGroup>();
        if (loadingBarCanvasGroup == null)
        {
            loadingBarCanvasGroup = loadingBarParent.gameObject.AddComponent<CanvasGroup>();
        }

        // [17.5s→18.5s] Button animation
        Transform[] buttons = mainMenuRoot.GetComponentsInChildren<Transform>();
        float buttonDelay = 0.1f;
        float buttonDuration = 0.3f;
        float startX = 500f; // Start position off-screen to the right

        foreach (Transform button in buttons)
        {
            if (button.GetComponent<Button>() != null)
            {
                button.localPosition += new Vector3(startX, 0, 0);
                button.DOLocalMoveX(0, buttonDuration)
                      .SetEase(Ease.OutBack)
                      .SetDelay(buttonDelay)
                      .SetUpdate(true);
                buttonDelay += 0.1f;
            }
        }

        // [17.5s→17.9s] Stripes exit animation
        List<Tween> stripeTweens = new List<Tween>();
        float currentDelay = 0f;
        
        // First handle white stripes
        foreach (Transform child in stripeGroup)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt != null && rt.name.Contains("White"))
            {
                Vector2 startPos = rt.anchoredPosition;
                Vector2 targetPos = startPos + (WHITE_STRIPE_RATIO * STRIPE_MOVE_DISTANCE);
                
                var tween = rt.DOAnchorPos(targetPos, stripeExitDuration)
                             .SetEase(Ease.InOutQuad)
                             .SetDelay(currentDelay)
                             .SetUpdate(true)
                             .OnComplete(() => rt.gameObject.SetActive(false));
                stripeTweens.Add(tween);
                currentDelay += STRIPE_DELAY;
            }
        }
        
        // Reset delay for pink stripes
        currentDelay = 0f;
        
        // Then handle pink stripes
        foreach (Transform child in stripeGroup)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt != null && rt.name.Contains("Pink"))
            {
                Vector2 startPos = rt.anchoredPosition;
                Vector2 targetPos = startPos + (PINK_STRIPE_RATIO * STRIPE_MOVE_DISTANCE);
                
                var tween = rt.DOAnchorPos(targetPos, stripeExitDuration)
                             .SetEase(Ease.InOutQuad)
                             .SetDelay(currentDelay)
                             .SetUpdate(true)
                             .OnComplete(() => rt.gameObject.SetActive(false));
                stripeTweens.Add(tween);
                currentDelay += STRIPE_DELAY;
            }
        }

        // [17.9s→18.3s] Wait for animations
        yield return new WaitForSecondsRealtime(logoGlowDuration);

        // [18.3s→18.7s] Fade out the loading bar
        loadingBarCanvasGroup.DOFade(0f, logoFadeDuration)
                            .SetEase(Ease.InQuad)
                            .SetUpdate(true);

        // Wait for all animations to complete - approximately 19.1s total
        yield return new WaitForSecondsRealtime(Mathf.Max(logoFadeDuration, stripeExitDuration + (currentDelay - STRIPE_DELAY)));

        // Finally, hide the loading screen altogether
        gameObject.SetActive(false);
    }
}