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
            // Load the next scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
        AudioManager.Music.FadeOutMusic(fadeDuration + 1);
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
        yield return new WaitForSeconds(2.0f);
        
        initialMenu.SetActive(false); // disable main menu

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

    // This coroutine will try to force activate the components after a brief delay
    private IEnumerator ForceActivateComponents()
    {
        // Wait for a frame to let initial setup complete
        yield return null;
        
        // Only force activation if they should be active at this point
        // Don't force scanlines or console text as they should appear with delay
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
}
