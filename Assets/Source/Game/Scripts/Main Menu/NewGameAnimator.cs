using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Game.Audio;

public class NewGameAnimator : MonoBehaviour
{
    [Header("Root that contains all main menu elements")]
    public RectTransform mainMenuRoot;
    public GameObject initialMenu;

    [Header("Transition Screen")]
    public GameObject transitionScreen;
    public Image transitionImage;
    public TextMeshProUGUI consoleText;
    public Image scanlineOverlay;

    private float textTypeSpeed = 0.03f;
    private float lineDelay = 0.5f;
    private string gameVersion = "1.0.0";
    private float cursorBlinkSpeed = 0.5f;
    private float scanlineSpeed = 0.1f;
    private float glitchChance = 0.1f;
    private string pressAnyKeyMessage = "PRESS ANY KEY TO PROCEED";
    private int maxConsoleLines = 30; // Maximum number of lines to keep in the console

    [Header("Colors")]
    public Color bootColor = new Color(0.2f, 0.8f, 0.2f);    // Green
    public Color initColor = new Color(0.8f, 0.8f, 0.2f);    // Yellow
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f);   // Red
    public Color statusColor = new Color(0.2f, 0.6f, 0.8f);  // Blue
    public Color readyColor = new Color(0.8f, 0.8f, 0.8f);   // White
    public Color pressKeyColor = new Color(1f, 1f, 1f);      // White
    public Color systemInfoColor = new Color(0.7f, 0.7f, 0.7f); // Gray for system info

    [Header("CRT Effects")]
    public float baseCurvatureX = 0.1f;
    public float baseCurvatureY = 0.1f;
    public float baseGlitchIntensity = 0.05f;
    public float errorGlitchIntensity = 0.4f;
    public float glitchSpeed = 3.0f;

    // How far the buttons slide off‑screen (X axis)
    private float slideDistance = 700f;

    // Time each button needs to slide/fade out
    private float buttonDuration = 0.3f;

    // Delay between successive buttons
    private float delayStep = 0.1f;

    // Duration of the fade to black transition
    private float fadeDuration = 2.5f;

    private bool canProceed = false;
    private bool isCursorVisible = true;
    private float scanlineOffset = 0f;
    private string cursorChar = "_";
    private int pressKeyTextStartIndex = 0;
    private bool isTyping = false;  // Track when text is being typed
    private Material shaderMaterial;  // Reference to the shader material

    private void Start()
    {
        // Ensure transition screen is initially disabled and image is transparent
        if (transitionScreen != null)
        {
            if (transitionImage != null)
            {
                Color color = transitionImage.color;
                color.a = 0f;
                transitionImage.color = color;
            }
        }

        // Initialize console text
        if (consoleText != null)
        {
            consoleText.text = "";
            consoleText.gameObject.SetActive(false);
            
            // Simple setup with standard TextMeshPro material
            consoleText.fontSharedMaterial = new Material(consoleText.fontSharedMaterial);
            consoleText.fontSharedMaterial.renderQueue = 3000; // Set to be rendered with transparent queue
            
            // Adjust text settings for terminal look
            consoleText.textWrappingMode = TextWrappingModes.NoWrap;
            consoleText.alignment = TextAlignmentOptions.TopLeft;
            consoleText.lineSpacing = -15f;
            consoleText.fontStyle = FontStyles.Normal;
            consoleText.margin = new Vector4(10, 5, 10, 5);
        }

        // Initialize scanlines
        if (scanlineOverlay != null)
        {
            // Make sure it's deactivated but fully set up
            scanlineOverlay.gameObject.SetActive(false);
            
            // Get and store the material reference
            if (scanlineOverlay.material != null)
            {
                shaderMaterial = scanlineOverlay.material;
                shaderMaterial.SetFloat("_Offset", 0f);
                shaderMaterial.SetFloat("_GlitchIntensity", baseGlitchIntensity);
                shaderMaterial.SetFloat("_GlitchSpeed", glitchSpeed);
                shaderMaterial.SetFloat("_CurvatureX", baseCurvatureX);
                shaderMaterial.SetFloat("_CurvatureY", baseCurvatureY);
            }
        }

        // Start cursor blink coroutine
        StartCoroutine(BlinkCursor());
    }

    private void Update()
    {
        if (canProceed && Input.anyKeyDown)
        {
            // Start power-off sequence instead of loading scene directly
            StartCoroutine(MonitorPowerOffSequence());
        }

        // Update scanline effect
        if (scanlineOverlay != null && scanlineOverlay.gameObject.activeSelf && shaderMaterial != null)
        {
            scanlineOffset += Time.deltaTime * scanlineSpeed;
            if (scanlineOffset >= 1f) scanlineOffset = 0f;
            shaderMaterial.SetFloat("_Offset", scanlineOffset);
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            if (consoleText != null && consoleText.gameObject.activeSelf)
            {
                isCursorVisible = !isCursorVisible;
                
                // Only update when we're allowed to proceed (after boot sequence)
                if (canProceed)
                {
                    UpdatePressKeyVisibility();
                }
                else
                {
                    UpdateCursor();
                }
            }
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    private void UpdateCursor()
    {
        if (consoleText != null && !isTyping)  // Only update cursor when not typing
        {
            // Trim console text if needed
            TrimConsoleText();
            
            string text = consoleText.text;
            if (text.EndsWith(cursorChar))
            {
                consoleText.text = text.Substring(0, text.Length - 1);
            }
            else if (isCursorVisible)
            {
                consoleText.text += cursorChar;
            }
        }
    }

    private void UpdatePressKeyVisibility()
    {
        if (consoleText == null) return;
        
        // Get the main text content without any cursor
        string baseText = consoleText.text;
        if (baseText.EndsWith(cursorChar))
        {
            baseText = baseText.Substring(0, baseText.Length - 1);
        }
        
        // Remove any existing press key message to avoid duplication
        int pressKeyIndex = baseText.LastIndexOf(pressAnyKeyMessage);
        if (pressKeyIndex >= 0)
        {
            baseText = baseText.Substring(0, pressKeyIndex);
        }
        
        // Add the press key message
        baseText += $"<color=#{ColorUtility.ToHtmlStringRGB(pressKeyColor)}>{pressAnyKeyMessage}</color>";
        
        // Add blinking cursor
        if (isCursorVisible)
        {
            baseText += cursorChar;
        }
        
        // Update the text
        consoleText.text = baseText;
    }

    public void PlayTransition()
    {
        AudioManager.Music.FadeOutMusic(fadeDuration + 2);
        AudioManager.SFX.Play("start_new_game", 0.2f);

        // Activate transition screen and start fade to black
        if (transitionScreen != null && transitionImage != null)
        {
            // First ensure the parent object is active
            transitionScreen.SetActive(true);
            
            // Then initialize its components
            if (transitionImage != null)
            {
                // Start with transparent image
                Color color = transitionImage.color;
                color.a = 0f;
                transitionImage.color = color;
                
                // Start the fade and console sequence after fade completes
                StartCoroutine(TransitionSequence());
            }
            
            // Initialize console text but keep it hidden
            if (consoleText != null)
            {
                consoleText.gameObject.SetActive(false); // Keep it hidden until after delay
                consoleText.text = ""; // Clear any previous text
            }
            
            // Initialize scanline overlay - but keep it deactivated until after the delay
            if (scanlineOverlay != null) 
            {
                scanlineOverlay.gameObject.SetActive(false);
            }
        }

        // Get all images we want to animate
        Image[] images = mainMenuRoot.GetComponentsInChildren<Image>(true);

        // Animate all images
        for (int i = 0; i < images.Length; i++)
        {
            Transform t = images[i].transform;
            Vector3 originalPos = t.localPosition;

            Sequence seq = DOTween.Sequence();
            seq.Append(t.DOLocalMoveX(originalPos.x - 50f, buttonDuration * 0.3f)
                .SetEase(Ease.OutQuad));
            seq.Append(t.DOLocalMoveX(slideDistance, buttonDuration)
                .SetEase(Ease.InBack));
            seq.SetDelay(i * delayStep)
               .SetUpdate(true);
        }
    }

    // Handle the full transition sequence
    private IEnumerator TransitionSequence()
    {
        // First fade to black
        yield return StartCoroutine(FadeToBlack());
        
        // Add additional delay after screen is black before "turning on" the computer
        yield return new WaitForSeconds(5.0f);

        AudioManager.SFX.Play("switch_6", 0.1f);

        // Additional delay after the switch sound, making like a pc turning on effect and shit
        yield return new WaitForSeconds(0.5f);

        // Create a CRT monitor power-on effect
        if (scanlineOverlay != null)
        {
            AudioManager.Music.Play("ambient.computer_hum");
            // Activate the overlay but set alpha to 0
            scanlineOverlay.gameObject.SetActive(true);
            Color startColor = scanlineOverlay.color;
            startColor.a = 0f;
            scanlineOverlay.color = startColor;

            // Define the final subtle alpha (15/255 ≈ 0.059)
            float targetAlpha = 15f / 255f;

            // Store the default shader values in case we can't get them from the material
            float originalIntensity = 0.5f;
            float originalCurvatureX = 0.1f;
            float originalCurvatureY = 0.1f;

            // Reset shader values if material exists and get original values
            if (scanlineOverlay.material != null)
            {
                shaderMaterial = scanlineOverlay.material;
                shaderMaterial.SetFloat("_Offset", 0f);

                // Get original shader values if possible
                if (shaderMaterial != null)
                {
                    originalIntensity = shaderMaterial.GetFloat("_Intensity");
                    originalCurvatureX = shaderMaterial.GetFloat("_CurvatureX");
                    originalCurvatureY = shaderMaterial.GetFloat("_CurvatureY");
                }
            }

            // Step 1: Quick initial flash (bright white overlay)
            float flashDuration = 0.08f; // Very quick flash
            float elapsedTime = 0f;

            // Create a gentler, less intense flash
            if (transitionImage != null)
            {
                // Use a softer flash color - light gray instead of pure white
                // and reduce the alpha for less intensity
                Color flashColor = new Color(0.7f, 0.7f, 0.75f, 0.6f);
                transitionImage.color = flashColor;

                // Wait a very brief moment
                yield return new WaitForSeconds(0.05f);

                // Quickly fade the flash
                while (elapsedTime < flashDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / flashDuration);

                    // Use a smoother easing function
                    float easedT = t * t * (3f - 2f * t); // Smoother ease

                    // Fade the softer flash to black
                    transitionImage.color = Color.Lerp(
                        flashColor,
                        new Color(0f, 0f, 0f, 1f),
                        easedT
                    );

                    yield return null;
                }

                // Ensure we end at pure black after the flash fades
                transitionImage.color = new Color(0f, 0f, 0f, 1f);
            }

            // Step 2: CRT warm-up phase
            elapsedTime = 0f;

            // Start with a much higher initial alpha and glitch intensity
            float initialOverlayAlpha = 0.4f; // Initial stronger scanline effect
            float initialGlitch = baseGlitchIntensity * 3f; // High initial glitch

            // Set the initial high intensity
            Color overlayColor = scanlineOverlay.color;
            overlayColor.a = initialOverlayAlpha;
            scanlineOverlay.color = overlayColor;

            // Apply initial shader effects if material exists
            if (shaderMaterial != null)
            {
                shaderMaterial.SetFloat("_GlitchIntensity", initialGlitch);
                shaderMaterial.SetFloat("_Intensity", originalIntensity * 2.0f);
                shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX * 1.5f);
                shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY * 1.5f);

                // Play random glitch sound
                PlayRandomGlitchSound(0.7f);
            }

            // Flicker phase - mimics old CRT flickering as it warms up
            int flickerSteps = 3;
            for (int i = 0; i < flickerSteps; i++)
            {
                // Dim down
                float dimDuration = 0.05f;
                elapsedTime = 0f;
                while (elapsedTime < dimDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float dimT = Mathf.Clamp01(elapsedTime / dimDuration);

                    overlayColor = scanlineOverlay.color;
                    overlayColor.a = Mathf.Lerp(initialOverlayAlpha, initialOverlayAlpha * 0.3f, dimT);
                    scanlineOverlay.color = overlayColor;

                    yield return null;
                }

                // Play random glitch sound at the end of the dim phase
                if (Random.value < 0.7f)
                {
                    PlayRandomGlitchSound(0.5f);
                }

                // Brighten again
                float brightenDuration = 0.07f;
                elapsedTime = 0f;
                while (elapsedTime < brightenDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float brightenT = Mathf.Clamp01(elapsedTime / brightenDuration);

                    overlayColor = scanlineOverlay.color;
                    overlayColor.a = Mathf.Lerp(initialOverlayAlpha * 0.3f, initialOverlayAlpha, brightenT);
                    scanlineOverlay.color = overlayColor;

                    yield return null;
                }
            }

            // Step 3: Gradual stabilization to final value
            float stabilizeDuration = 0.7f;
            elapsedTime = 0f;

            while (elapsedTime < stabilizeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / stabilizeDuration);

                // Ease out function for more natural stabilizing
                float easedT = 1 - (1 - t) * (1 - t);

                // Update the overlay alpha - gradually reducing from initial high value
                overlayColor = scanlineOverlay.color;
                overlayColor.a = Mathf.Lerp(initialOverlayAlpha, targetAlpha, easedT);
                scanlineOverlay.color = overlayColor;

                // Gradually stabilize the glitch intensity
                if (shaderMaterial != null)
                {
                    float currentGlitch = Mathf.Lerp(initialGlitch, baseGlitchIntensity, easedT);
                    shaderMaterial.SetFloat("_GlitchIntensity", currentGlitch);

                    // Return shader properties to normal
                    if (t > 0.6f) // After 60% of the stabilization time
                    {
                        float restoreT = (t - 0.6f) / 0.4f; // Normalize remaining time

                        // Restore original intensity gradually
                        float currentIntensity = shaderMaterial.GetFloat("_Intensity");
                        shaderMaterial.SetFloat("_Intensity", Mathf.Lerp(currentIntensity, originalIntensity, restoreT));

                        // Restore original curvature gradually
                        float currentCurvatureX = shaderMaterial.GetFloat("_CurvatureX");
                        float currentCurvatureY = shaderMaterial.GetFloat("_CurvatureY");
                        shaderMaterial.SetFloat("_CurvatureX", Mathf.Lerp(currentCurvatureX, originalCurvatureX, restoreT));
                        shaderMaterial.SetFloat("_CurvatureY", Mathf.Lerp(currentCurvatureY, originalCurvatureY, restoreT));
                    }
                }

                yield return null;
            }

            // Ensure we end at exactly the target alpha
            overlayColor = scanlineOverlay.color;
            overlayColor.a = targetAlpha;
            scanlineOverlay.color = overlayColor;

            // Make sure shader params are at correct values
            if (shaderMaterial != null)
            {
                shaderMaterial.SetFloat("_GlitchIntensity", baseGlitchIntensity);
                shaderMaterial.SetFloat("_Intensity", originalIntensity);
                shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX);
                shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY);
            }
        }
        
        // Short delay after scanlines appear before text starts
        yield return new WaitForSeconds(0.5f);
        
        // Activate console text with fade-in effect
        if (consoleText != null)
        {
            consoleText.gameObject.SetActive(true);
            
            // Start with transparent text
            Color textColor = consoleText.color;
            float originalAlpha = textColor.a;
            textColor.a = 0f;
            consoleText.color = textColor;
            
            // Fade in the text
            float textFadeDuration = 0.4f;
            float elapsedTime = 0f;
            
            while (elapsedTime < textFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / textFadeDuration);
                
                // Update the color
                Color newColor = consoleText.color;
                newColor.a = Mathf.Lerp(0f, originalAlpha, t);
                consoleText.color = newColor;
                
                yield return null;
            }
            
            // Ensure we end with correct alpha
            textColor.a = originalAlpha;
            consoleText.color = textColor;
            
            // Start console sequence after fade-in is complete
            StartCoroutine(PlayConsoleSequence());
        }
    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        Color startColor = transitionImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            transitionImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        // Ensure we end at exactly 100% opacity
        transitionImage.color = endColor;

        initialMenu.SetActive(false); // disable main menu
    }

    private IEnumerator PlayConsoleSequence()
    {
        isTyping = true;  // Start typing sequence
        
        // List to keep track of all messages
        List<string> consoleHistory = new List<string>();

        // Get real system information
        string osInfo = SystemInfo.operatingSystem;
        string cpuInfo = SystemInfo.processorType + " (" + SystemInfo.processorCount + " cores)";
        string gpuInfo = SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsMemorySize + "MB)";
        string ramInfo = (SystemInfo.systemMemorySize / 1024f).ToString("F1") + "GB";
        string gamePath = Application.dataPath;
        string deviceName = SystemInfo.deviceName;

        // Define boot messages with their types and colors
        var bootMessages = new (string message, Color color, float duration, bool hasLoadingBar, bool isError, bool instantLoad)[]
        {
            ($"[BOOT] Purrine_AI-B1-08364142020", bootColor, 3f, false, false, false),
            // System info messages in gray that load instantly like Windows CMD
            ($"[SYS] OS: {osInfo}", systemInfoColor, 0f, false, false, true),
            ($"[SYS] CPU: {cpuInfo}", systemInfoColor, 0f, false, false, true),
            ($"[SYS] GPU: {gpuInfo}", systemInfoColor, 0f, false, false, true),
            ($"[SYS] RAM: {ramInfo}", systemInfoColor, 0f, false, false, true),
            ($"[SYS] DEVICE: {deviceName}", systemInfoColor, 0f, false, false, true),
            ($"[FILE] Path: {gamePath}", systemInfoColor, 0f, false, false, true),
            ($"[FILE] Instance: purrine_instance_{System.DateTime.Now.ToString("yyyyMMddHHmm")}.bin", systemInfoColor, 0f, false, false, true),
            // ($"[HASH] Verifying system integrity...", initColor, 0.8f, true, false, false),
            // ($"[SEC] Initializing security protocols...", initColor, 0.5f, true, false, false),
            ("[INIT] Loading neural networks...", initColor, 1.2f, true, false, false),
            ("[MEM] Allocating memory buffers...", initColor, 0.6f, true, false, true),
            ("[INIT] Calibrating sensors...", initColor, 0.8f, true, false, false),
            ("[DIAG] Running sensor diagnostics...", statusColor, 0.7f, true, false, true),
            ("[INIT] Initializing personality matrix...", initColor, 1.5f, true, false, false),
            ("[NET] Establishing connection to primary network...", initColor, 0.8f, true, false, true),
            ("[INIT] Establishing quantum link...", initColor, 1.0f, true, false, false),
            ("[ERROR] Quantum link unstable...", errorColor, 0.3f, false, true, false),
            ("[DIAG] Signal strength: 32%", errorColor, 0.2f, false, false, true),
            ("[DIAG] Packet loss: 78%", errorColor, 0.2f, false, false, true),
            ("[LOG] Attempting signal amplification", statusColor, 0.2f, false, false, true),
            ("[INIT] Re-establishing quantum link...", initColor, 0.7f, true, false, false),
            ("[ERROR] Failed to establish quantum link. Proceeding OFFLINE...", errorColor, 0.2f, false, true, false),
            ("[LOG] Activating fallback protocol: OFFLINE_MODE", statusColor, 0.4f, false, false, true),
            ("[MEM] Verifying memory integrity...", initColor, 0.6f, true, false, false),
            ("[MEM] Available memory: 498.2TB/512TB", statusColor, 0.2f, false, false, true),
            // ("[MEM] Cache optimized", statusColor, 0.2f, false, false, true),
            ("[INIT] Loading user preferences...", initColor, 0.5f, true, false, false),
            // ("[PERF] Graphics API: " + SystemInfo.graphicsDeviceType, systemInfoColor, 0f, false, false, true),
            //("[PERF] Quality level: " + QualitySettings.GetQualityLevel(), systemInfoColor, 0f, false, false, true),
            ("[STATUS] Version: " + gameVersion + " (Build " + System.DateTime.Now.ToString("yyyyMMddHHmm") + ")", statusColor, 0.3f, false, false, true),
            ("[STATUS] All systems nominal", statusColor, 0.4f, false, false, false),
            ("[LOG] Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), systemInfoColor, 0f, false, false, true),
            ("[READY] Completed initialization for: PURRINE_AI-B1-08364142025", readyColor, 0.6f, false, false, false)
        };

        // Clear console text at the start and remove cursor
        if (consoleText.text != null && consoleText.text.EndsWith(cursorChar))
        {
            consoleText.text = consoleText.text.Substring(0, consoleText.text.Length - 1);
        }
        else
        {
            consoleText.text = "";
        }

        // Force text to be monospaced with correct line height for a terminal feel
        consoleText.textWrappingMode = TextWrappingModes.NoWrap;  // Disable word wrapping for terminal feel
        consoleText.alignment = TextAlignmentOptions.TopLeft;  // Align to top left like a real terminal
        consoleText.lineSpacing = -15f;  // Tighten line spacing for a terminal look

        yield return new WaitForSeconds(0.5f);  // Initial pause before starting
        
        AudioManager.SFX.Play("confirm_1", 0.3f);

        foreach (var (message, color, duration, hasLoadingBar, isError, instantLoad) in bootMessages)
        {
            // Set glitch intensity based on whether this is an error message
            if (shaderMaterial != null)
            {
                // Smoothly transition glitch intensity
                float targetGlitch = isError ? errorGlitchIntensity : baseGlitchIntensity;
                StartCoroutine(TransitionGlitchIntensity(targetGlitch, isError ? 0.2f : 0.5f));

                // Add more intense random glitch on errors
                if (isError)
                {
                    TriggerGlitchEffect(errorGlitchIntensity * 1.5f, 0.15f);
                }
            }

            // Add prefix with caret
            consoleText.text += "> ";

            // Convert color to hex for rich text
            string colorHex = ColorUtility.ToHtmlStringRGB(color);

            if (instantLoad)
            {
                // For instant loading messages, just add the whole text at once
                consoleText.text += $"<color=#{colorHex}>{message}</color>\n";

                // Add to history
                consoleHistory.Add("> " + $"<color=#{colorHex}>{message}</color>");

                // Trim if needed
                if (consoleHistory.Count > maxConsoleLines)
                {
                    consoleHistory.RemoveAt(0);
                    consoleText.text = string.Join("\n", consoleHistory) + "\n";
                }

                // Short delay between instant messages for readability
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                // Store the starting message text (for history tracking)
                string messageText = "";

                // Type out each character for non-instant messages
                for (int i = 0; i < message.Length; i++)
                {
                    char c = message[i];

                    // Glitch effect chance (higher for error messages)
                    float localGlitchChance = isError ? glitchChance * 1.5f : glitchChance;
                    if (Random.value < localGlitchChance)
                    {
                        // Generate a glitch character
                        char glitchChar = (char)Random.Range(33, 126);

                        // Add glitch character in error color
                        string glitchText = $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>{glitchChar}</color>";
                        consoleText.text += glitchText;
                        yield return new WaitForSeconds(textTypeSpeed * 0.5f);

                        // Remove the glitch - safely
                        int txtLength = consoleText.text.Length;
                        if (txtLength >= glitchText.Length)
                        {
                            consoleText.text = consoleText.text.Substring(0, txtLength - glitchText.Length);
                        }
                    }

                    // Add the character with color
                    consoleText.text += $"<color=#{colorHex}>{c}</color>";
                    messageText += $"<color=#{colorHex}>{c}</color>";

                    // Play typing sound for each character (except for error messages)
                    if (!isError && !instantLoad && Random.value < 0.3f)
                    {
                        AudioManager.SFX.Play("bong_1", 0.2f);
                    }

                    // Variable typing speed with occasional brief pauses
                    float typingVariation = Random.Range(0.8f, 1.2f);

                    // Play sound effect when an error message is fully typed
                    if (i == 7 && message.StartsWith("[ERROR]"))
                    {
                        AudioManager.SFX.Play("negative_2", 0.3f);
                    }

                    // Occasional slight pause during typing (feels more like real typing)
                    if (Random.value < 0.05f && !isError)
                    {
                        yield return new WaitForSeconds(textTypeSpeed * 3f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(textTypeSpeed * typingVariation);
                    }

                    // For errors, randomly trigger small shader glitches during typing
                    if (isError && Random.value < 0.1f && shaderMaterial != null)
                    {
                        TriggerGlitchEffect(Random.Range(0.2f, 0.4f), 0.05f);
                    }
                }

                // Add loading bar if needed
                if (hasLoadingBar)
                {
                    // Store original message for the completion display
                    string originalMessage = message;

                    // Add to history before adding the loading bar
                    consoleHistory.Add("> " + messageText);

                    // Add a line break and start the loading bar on a new line
                    consoleText.text += $"\n    <color=#{colorHex}>[";
                    int barLength = 20;
                    float totalElapsedTime = 0f;
                    float targetTime = duration * 1.5f; // Make loading longer overall
                    int currentBars = 0;

                    while (currentBars < barLength)
                    {
                        // Create variable loading speeds - sometimes fast, sometimes stalled
                        float progressSpeed = Random.value < 0.2f ?
                            Random.Range(0.05f, 0.1f) :  // Slower progress (20% chance)
                            Random.Range(0.2f, 0.4f);    // Normal progress (80% chance)

                        // Occasionally add multiple bars at once (progress spike)
                        int barsToAdd = Random.value < 0.15f ?
                            Random.Range(2, 4) : // Progress spike (15% chance)
                            1;                   // Normal progress (85% chance)

                        // Ensure we don't exceed the total
                        barsToAdd = Mathf.Min(barsToAdd, barLength - currentBars);

                        if (barsToAdd > 0)
                        {
                            string barSegment = new string('=', barsToAdd);
                            consoleText.text += barSegment;
                            currentBars += barsToAdd;

                            // Calculate progress percentage (0-100)
                            int percentage = Mathf.RoundToInt((float)currentBars / barLength * 100f);

                            // Update the completion percentage
                            string textWithoutPercentage = consoleText.text;
                            consoleText.text = $"{textWithoutPercentage}] {percentage}%";

                            // Wait based on progress speed
                            yield return new WaitForSeconds(progressSpeed);

                            // Random chance for a small glitch during loading
                            if (Random.value < 0.08f && shaderMaterial != null)
                            {
                                TriggerGlitchEffect(Random.Range(0.1f, 0.2f), 0.05f);
                            }

                            // Remove the percentage for next update if not complete
                            if (currentBars < barLength)
                            {
                                consoleText.text = textWithoutPercentage;
                            }
                        }

                        // Sometimes add a brief pause (stall)
                        if (Random.value < 0.1f && currentBars < barLength)
                        {
                            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

                            // During stalls, sometimes show a system message
                            if (Random.value < 0.3f)
                            {
                                string stall = currentBars > barLength / 2 ?
                                    $"\n      <color=#888888>...{(Random.value < 0.5f ? "validating" : "processing")}</color>" :
                                    $"\n      <color=#888888>...{(Random.value < 0.5f ? "collecting" : "analyzing")}</color>";
                                consoleText.text += stall;
                                yield return new WaitForSeconds(0.3f);

                                // Remove the stall message
                                consoleText.text = consoleText.text.Substring(0, consoleText.text.Length - stall.Length);
                            }
                        }

                        // Ensure we eventually complete even if slow
                        totalElapsedTime += Time.deltaTime + progressSpeed;
                        if (totalElapsedTime >= targetTime && currentBars < barLength)
                        {
                            // Force completion if taking too long
                            string remainingBars = new string('=', barLength - currentBars);
                            consoleText.text += remainingBars;
                            currentBars = barLength;
                        }
                    }

                    // Ensure we show 100% at the end but preserving the original message
                    int endIndex = consoleText.text.LastIndexOf(']');
                    if (endIndex > 0 && endIndex < consoleText.text.Length)
                    {
                        consoleText.text = consoleText.text.Substring(0, endIndex + 1) + " 100%</color>";
                    }
                    else
                    {
                        consoleText.text += "] 100%</color>";
                    }

                    // Display a completion message alongside the original message in a cleaner way
                    consoleText.text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(readyColor)}>[OK]</color> <color=#{colorHex}>{originalMessage}</color>";

                    // Add the completion message to history
                    consoleHistory.Add($"<color=#{ColorUtility.ToHtmlStringRGB(readyColor)}>[OK]</color> <color=#{colorHex}>{originalMessage}</color>");
                }
                else
                {
                    // Add to history if no loading bar
                    consoleHistory.Add("> " + messageText);
                }

                // Add line break
                consoleText.text += "\n";

                // Trim if needed
                if (consoleHistory.Count > maxConsoleLines)
                {
                    consoleHistory.RemoveAt(0);
                    // Don't update text here as we're still in the process of typing it out
                }
            }

            // Random delay between messages with occasional "system activity" indicators
            if (!instantLoad)
            {
                float messageDelay = lineDelay + Random.Range(0f, 0.3f);
                if (Random.value < 0.2f && !isError)
                {
                    // Show a brief "thinking" indicator
                    string activity = Random.value < 0.5f ?
                        "<color=#666666>...</color>" :
                        "<color=#666666>" + new string('.', Random.Range(1, 4)) + "</color>";
                    consoleText.text += activity;
                    yield return new WaitForSeconds(messageDelay);
                    consoleText.text = consoleText.text.Substring(0, consoleText.text.Length - activity.Length);
                }
                else
                {
                    yield return new WaitForSeconds(messageDelay);
                }
            }
        }

        // Reset glitch to base level
        if (shaderMaterial != null)
        {
            StartCoroutine(TransitionGlitchIntensity(baseGlitchIntensity, 1.0f));
        }

        // Add an extra line break for spacing
        consoleText.text += "\n";
        
        // Save the index where the "Press Any Key" message would start
        // But don't add it yet - we'll let UpdatePressKeyVisibility handle this
        pressKeyTextStartIndex = consoleText.text.Length;
        
        // Enable scene transition
        canProceed = true;
        isTyping = false;  // End typing sequence

        AudioManager.SFX.Play("glass_5", 0.5f);
        
        // Force first update of the press key visibility
        UpdatePressKeyVisibility();
        
        // Return so the coroutine completes
        yield return null;
    }

    // Smoothly transition glitch intensity
    private IEnumerator TransitionGlitchIntensity(float targetIntensity, float duration)
    {
        if (shaderMaterial != null)
        {
            float startIntensity = shaderMaterial.GetFloat("_GlitchIntensity");
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                shaderMaterial.SetFloat("_GlitchIntensity", currentIntensity);
                yield return null;
            }
            
            // Ensure we end at exactly the target intensity
            shaderMaterial.SetFloat("_GlitchIntensity", targetIntensity);
        }
    }

    // Add a random glitch effect that can be triggered at specific moments
    public void TriggerGlitchEffect(float intensity = 0.5f, float duration = 0.5f)
    {
        if (shaderMaterial != null)
        {
            StartCoroutine(GlitchEffectSequence(intensity, duration));
            
            // Add chance to play a glitch sound when triggering a visual glitch
            if (Random.value < 0.7f)
            {
                PlayRandomGlitchSound(intensity / 2.0f);
            }
        }
    }
    
    private IEnumerator GlitchEffectSequence(float intensity, float duration)
    {
        if (shaderMaterial != null)
        {
            // Store original values
            float originalGlitchIntensity = shaderMaterial.GetFloat("_GlitchIntensity");
            float originalGlitchSpeed = shaderMaterial.GetFloat("_GlitchSpeed");
            
            // Set intense glitch values
            shaderMaterial.SetFloat("_GlitchIntensity", intensity);
            shaderMaterial.SetFloat("_GlitchSpeed", glitchSpeed * 2f);
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Restore original values
            shaderMaterial.SetFloat("_GlitchIntensity", originalGlitchIntensity);
            shaderMaterial.SetFloat("_GlitchSpeed", originalGlitchSpeed);
        }
    }

    // Add this helper method to trim the console text when it gets too long
    private void TrimConsoleText()
    {
        if (consoleText == null) return;
        
        string[] lines = consoleText.text.Split('\n');
        
        // If we exceed the maximum number of lines, trim the oldest ones
        if (lines.Length > maxConsoleLines)
        {
            // Keep only the most recent lines
            int linesToKeep = maxConsoleLines;
            
            // We need to find where the press key message might be
            bool hasPressKeyMessage = false;
            int pressKeyLine = -1;
            
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Contains(pressAnyKeyMessage))
                {
                    hasPressKeyMessage = true;
                    pressKeyLine = i;
                    break;
                }
            }
            
            // If we have a press key message, make sure it's kept
            if (hasPressKeyMessage && pressKeyLine >= 0)
            {
                // Ensure we include the press key message line in what we keep
                linesToKeep = Mathf.Min(maxConsoleLines, lines.Length - pressKeyLine);
            }
            
            // Build new text with only the most recent lines
            string newText = string.Join("\n", lines.Skip(lines.Length - linesToKeep).Take(linesToKeep));
            
            // Update the console text
            consoleText.text = newText;
        }
    }

    // Monitor power-off effect before scene transition
    private IEnumerator MonitorPowerOffSequence()
    {
        // Disable input during transition
        canProceed = false;
        
        // Play switch-off sound
        AudioManager.SFX.Play("switch_6", 0.2f);
        
        // Fade out computer hum sound
        AudioManager.Music.FadeOutMusic(1.0f);
        
        // Initial glitch sound
        PlayRandomGlitchSound(0.6f);
        
        // Let the sound play for a moment before effects start
        yield return new WaitForSeconds(0.1f);
        
        // Increase CRT glitch effects
        if (shaderMaterial != null)
        {
            // Store original values
            float originalGlitchIntensity = shaderMaterial.GetFloat("_GlitchIntensity");
            float originalGlitchSpeed = shaderMaterial.GetFloat("_GlitchSpeed");
            float originalCurvatureX = shaderMaterial.GetFloat("_CurvatureX");
            float originalCurvatureY = shaderMaterial.GetFloat("_CurvatureY");
            
            // Increase values for shutdown effect
            shaderMaterial.SetFloat("_GlitchIntensity", originalGlitchIntensity * 4f);
            shaderMaterial.SetFloat("_GlitchSpeed", originalGlitchSpeed * 3f);
            shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX * 2f);
            shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY * 2f);
            
            // Wait briefly with high distortion
            yield return new WaitForSeconds(0.15f);
        }

        // Add glitch lines that appear during the shutdown - but in a more organic way
        StartCoroutine(CreateGlitchLines(transitionImage.transform, 1.5f));
        
        // CRT collapse effect
        if (scanlineOverlay != null)
        {
            // Final intense distortion flash
            TriggerGlitchEffect(1.0f, 0.1f);
            
            // Quickly brighten the scanlines
            Color overlayColor = scanlineOverlay.color;
            float originalAlpha = overlayColor.a;
            float elapsedTime = 0f;
            float flashDuration = 0.1f;
            
            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / flashDuration);
                
                overlayColor.a = Mathf.Lerp(originalAlpha, 0.4f, t);
                scanlineOverlay.color = overlayColor;
                yield return null;
            }
        }
        
        // Horizontal line collapse (classic CRT power off)
        if (consoleText != null && transitionImage != null)
        {
            // Hide the console text
            consoleText.gameObject.SetActive(false);
            
            // Create the shrinking white line effect
            RectTransform canvasRect = transitionImage.GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            
            // Create a white line
            GameObject lineObj = new GameObject("PowerOffLine");
            lineObj.transform.SetParent(transitionImage.transform, false);
            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = Color.white;
            
            // Set up the line's rect transform
            RectTransform lineRect = lineImage.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 0.5f);
            lineRect.anchorMax = new Vector2(1, 0.5f);
            lineRect.sizeDelta = new Vector2(0, screenHeight * 0.1f); // Start at 10% of screen height
            
            // Quick brightness flash before collapse
            float elapsedTime = 0f;
            float flashDuration = 0.05f;
            
            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / flashDuration);
                
                lineRect.sizeDelta = new Vector2(0, Mathf.Lerp(screenHeight * 0.1f, screenHeight, t));
                yield return null;
            }
            
            // Set to full height
            lineRect.sizeDelta = new Vector2(0, screenHeight);
            
            // Brief pause at full brightness
            yield return new WaitForSeconds(0.05f);
            
            // Collapse to a thin line
            elapsedTime = 0f;
            float collapseDuration = 0.2f;
            
            while (elapsedTime < collapseDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / collapseDuration);
                // Use easing for more natural collapse
                float easedT = t * t;
                
                lineRect.sizeDelta = new Vector2(0, Mathf.Lerp(screenHeight, 2f, easedT));
                yield return null;
            }
            
            // Set to final thin line
            lineRect.sizeDelta = new Vector2(0, 2f);
            
            // Final flicker and fade out
            elapsedTime = 0f;
            float fadeOutDuration = 0.2f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeOutDuration);
                
                // Add slight flicker
                if (Random.value < 0.3f)
                {
                    lineImage.enabled = !lineImage.enabled;
                }
                
                // Fade out
                Color lineColor = lineImage.color;
                lineColor.a = Mathf.Lerp(1f, 0f, t);
                lineImage.color = lineColor;
                
                yield return null;
            }
            
            // Make sure line is gone
            Destroy(lineObj);
            
            // Brief pause before final blackout
            yield return new WaitForSeconds(0.1f);
            
            // Final complete blackout
            if (transitionImage != null)
            {
                transitionImage.color = Color.black;
            }
        }
        
        // Make sure everything is disabled before scene transition
        if (scanlineOverlay != null)
        {
            scanlineOverlay.gameObject.SetActive(false);
        }
        
        // Brief pause in darkness
        yield return new WaitForSeconds(1.0f);
        
        // Now load the next scene
        SceneManager.LoadScene("IntroAndTutorial");
    }

    // Create thin white lines that appear and disappear randomly to simulate monitor glitching
    // in a more natural, less constant way
    private IEnumerator CreateGlitchLines(Transform parent, float duration)
    {
        // List to track created lines for easy cleanup
        List<GameObject> glitchLines = new List<GameObject>();
        
        // Get canvas dimensions for positioning
        RectTransform canvasRect = parent.GetComponent<RectTransform>();
        float screenHeight = canvasRect.rect.height;
        float screenWidth = canvasRect.rect.width;
        
        // Multiple phases of the glitch effect
        float totalTime = 0;
        
        // Define the glitch intensity over time
        // 0-0.3: Initial subtle glitches
        // 0.3-0.7: Increasing intensity
        // 0.7-1.0: Peak intensity with bursts
        // 1.0-1.5: Gradual fadeout with occasional strong glitches
        
        while (totalTime < duration)
        {
            totalTime += Time.deltaTime;
            float normalizedTime = totalTime / duration; // 0 to 1 over the course of the effect
            
            // Determine the current phase
            float baseChance = 0;
            float burstChance = 0;
            
            // Phase 1: Initial subtle glitches (0-20% of duration)
            if (normalizedTime < 0.2f)
            {
                baseChance = 0.01f; // Very rare glitches
                burstChance = 0.005f; // Almost no bursts
                
                // Occasionally create a single line for a brief moment
                if (Random.value < baseChance && glitchLines.Count < 1)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.3f);
                }
            }
            // Phase 2: Building up (20-50% of duration)
            else if (normalizedTime < 0.5f)
            {
                // Gradually increase chance as we progress
                float progress = (normalizedTime - 0.2f) / 0.3f; // 0 to 1 within this phase
                baseChance = Mathf.Lerp(0.01f, 0.05f, progress);
                burstChance = Mathf.Lerp(0.005f, 0.02f, progress);
                
                // Create occasional lines
                if (Random.value < baseChance && glitchLines.Count < 3)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.5f);
                }
                
                // Occasionally create a burst of 2-3 lines
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(2, 4);
                    
                    // Chance to play a glitch sound for the burst
                    if (Random.value < 0.3f)
                    {
                        PlayRandomGlitchSound(0.4f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.4f);
                        yield return new WaitForSeconds(0.02f); // Slight delay between lines in burst
                    }
                }
            }
            // Phase 3: Major glitching (50-70% of duration)
            else if (normalizedTime < 0.7f)
            {
                baseChance = 0.08f; // Frequent individual glitches
                burstChance = 0.04f; // More common bursts
                
                // Create regular lines
                if (Random.value < baseChance && glitchLines.Count < 5)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.7f);
                }
                
                // Create bursts of 2-4 lines
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(2, 5);
                    
                    // Higher chance to play a glitch sound for the burst
                    if (Random.value < 0.5f)
                    {
                        PlayRandomGlitchSound(0.5f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.6f);
                        yield return new WaitForSeconds(0.01f); // Quick burst
                    }
                }
                
                // Occasionally trigger more intense shader glitch during this phase
                if (Random.value < 0.03f && shaderMaterial != null)
                {
                    TriggerGlitchEffect(Random.Range(0.5f, 1.0f), 0.1f);
                }
            }
            // Phase 4: Peak intensity (70-85% of duration)
            else if (normalizedTime < 0.85f)
            {
                baseChance = 0.1f; // Very frequent individual glitches
                burstChance = 0.06f; // Common bursts
                
                // Create regular lines
                if (Random.value < baseChance && glitchLines.Count < 8)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.8f);
                }
                
                // Create larger bursts of 3-6 lines
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(3, 7);
                    
                    // Almost always play a glitch sound for the burst at peak intensity
                    if (Random.value < 0.8f)
                    {
                        PlayRandomGlitchSound(0.7f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.7f);
                        yield return new WaitForSeconds(0.01f); // Quick burst
                    }
                }
            }
            // Phase 5: Fadeout with occasional strong bursts (85-100% of duration)
            else
            {
                float progress = (normalizedTime - 0.85f) / 0.15f; // 0 to 1 within this phase
                baseChance = Mathf.Lerp(0.1f, 0.02f, progress); // Decreasing chance
                burstChance = Mathf.Lerp(0.06f, 0.01f, progress); // Decreasing bursts
                
                // Still create individual lines, but less often
                if (Random.value < baseChance && glitchLines.Count < 6)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.6f);
                }
                
                // Occasionally still have a strong burst
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(4, 8); // Can be larger bursts
                    
                    // Chance of one final big glitch sound
                    if (Random.value < 0.4f)
                    {
                        PlayRandomGlitchSound(0.8f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.5f);
                        yield return new WaitForSeconds(0.01f);
                    }
                    
                    // Chance of one final major glitch/flash
                    if (Random.value < 0.3f && shaderMaterial != null)
                    {
                        TriggerGlitchEffect(1.2f, 0.15f);
                    }
                }
            }
            
            // Update existing lines
            UpdateGlitchLines(glitchLines, normalizedTime);
            
            // Clean up faded/expired lines
            CleanupGlitchLines(glitchLines);
            
            yield return null;
        }
        
        // Final cleanup
        foreach (GameObject line in glitchLines.ToArray())
        {
            Destroy(line);
        }
        glitchLines.Clear();
    }
    
    // Helper method to create a single glitch line
    private void CreateGlitchLine(Transform parent, List<GameObject> linesList, float screenHeight, float screenWidth, float maxLifetime)
    {
        GameObject lineObj = new GameObject("GlitchLine_" + Random.Range(0, 1000));
        lineObj.transform.SetParent(parent, false);
        Image lineImage = lineObj.AddComponent<Image>();
        
        // Store the lifetime in a component we can check later
        GlitchLineData data = lineObj.AddComponent<GlitchLineData>();
        data.CreationTime = Time.time;
        data.Lifetime = Random.Range(0.1f, maxLifetime);
        
        // Randomize opacity based on intensity (more intense = more opaque)
        lineImage.color = new Color(1f, 1f, 1f, Random.Range(0.5f, 0.95f));
        
        RectTransform lineRect = lineImage.GetComponent<RectTransform>();
        
        // Horizontal line spanning a portion of the width
        lineRect.anchorMin = new Vector2(0, 0);
        lineRect.anchorMax = new Vector2(1, 0);
        
        // Position randomly on screen
        float yPos = Random.Range(0.1f, 0.9f);
        lineRect.anchoredPosition = new Vector2(0, screenHeight * yPos);
        
        // Very thin lines (1-3 pixels)
        lineRect.sizeDelta = new Vector2(0, Random.Range(1f, 3f));
        
        // Random width 
        float leftOffset = Random.value < 0.3f ? 0 : Random.Range(0, screenWidth * 0.4f);
        float rightOffset = Random.value < 0.3f ? 0 : Random.Range(0, screenWidth * 0.4f);
        lineRect.offsetMin = new Vector2(leftOffset, lineRect.offsetMin.y);
        lineRect.offsetMax = new Vector2(-rightOffset, lineRect.offsetMax.y);
        
        // Sometimes play a glitch sound when creating a line
        if (Random.value < 0.1f)
        {
            PlayRandomGlitchSound(0.3f);
        }
        
        // Add to list for tracking
        linesList.Add(lineObj);
    }
    
    // Helper method to update existing glitch lines
    private void UpdateGlitchLines(List<GameObject> linesList, float normalizedTime)
    {
        foreach (GameObject line in linesList.ToArray())
        {
            if (line == null) continue;
            
            // Get the data component
            GlitchLineData data = line.GetComponent<GlitchLineData>();
            if (data == null) continue;
            
            // Calculate age
            float age = Time.time - data.CreationTime;
            float lifetimeProgress = age / data.Lifetime;
            
            if (lifetimeProgress < 1.0f)
            {
                // Different behavior at different stages of line lifetime
                if (lifetimeProgress < 0.1f)
                {
                    // Quick fade in
                    Image img = line.GetComponent<Image>();
                    if (img != null)
                    {
                        Color c = img.color;
                        float targetAlpha = c.a; // Store original target
                        c.a = Mathf.Lerp(0, targetAlpha, lifetimeProgress / 0.1f);
                        img.color = c;
                    }
                }
                else if (lifetimeProgress > 0.7f)
                {
                    // Fade out at end of life
                    Image img = line.GetComponent<Image>();
                    if (img != null)
                    {
                        Color c = img.color;
                        float startAlpha = c.a; // Use current alpha as base
                        c.a = Mathf.Lerp(startAlpha, 0, (lifetimeProgress - 0.7f) / 0.3f);
                        img.color = c;
                    }
                }
                
                // Random flickering
                if (Random.value < 0.1f)
                {
                    line.SetActive(!line.activeSelf);
                }
                
                // Move lines occasionally, more likely during intense phases
                float moveChance = normalizedTime < 0.7f ? 0.05f : 0.1f;
                if (Random.value < moveChance)
                {
                    RectTransform rt = line.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        Vector2 currentPos = rt.anchoredPosition;
                        rt.anchoredPosition = new Vector2(currentPos.x, currentPos.y + Random.Range(-4f, 4f));
                    }
                }
            }
        }
    }
    
    // Helper method to clean up faded or expired lines
    private void CleanupGlitchLines(List<GameObject> linesList)
    {
        for (int i = linesList.Count - 1; i >= 0; i--)
        {
            if (i >= linesList.Count) continue; // Safety check
            
            GameObject line = linesList[i];
            if (line == null)
            {
                linesList.RemoveAt(i);
                continue;
            }
            
            GlitchLineData data = line.GetComponent<GlitchLineData>();
            if (data == null)
            {
                linesList.RemoveAt(i);
                Destroy(line);
                continue;
            }
            
            // Remove if lifetime exceeded
            float age = Time.time - data.CreationTime;
            if (age > data.Lifetime)
            {
                linesList.RemoveAt(i);
                Destroy(line);
            }
        }
    }
    
    // Simple class to store glitch line data
    private class GlitchLineData : MonoBehaviour
    {
        public float CreationTime;
        public float Lifetime;
    }

    // Helper method to play a random glitch sound effect
    private void PlayRandomGlitchSound(float volume = 0.5f)
    {
        int soundNumber = Random.Range(1, 5); // Random number between 1 and 4
        string soundName = "glitch_" + soundNumber;
        AudioManager.SFX.Play(soundName, volume);
    }
}
