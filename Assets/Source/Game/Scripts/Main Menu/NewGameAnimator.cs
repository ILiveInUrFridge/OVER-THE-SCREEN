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

    private float textTypeSpeed = 0.025f;
    private float lineDelay = 0.5f;
    private string gameVersion = "1.0.0";
    private float cursorBlinkSpeed = 0.5f;
    private float scanlineSpeed = 0.1f;
    private float glitchChance = 0.1f;
    private string pressAnyKeyMessage = "PRESS ENTER TO CONNECT";
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
        if (canProceed && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            // Disable further input
            canProceed = false;
            
            // Show "Connecting..." message and display system info after connection
            StartCoroutine(ShowConnectionSuccessful());
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
        
        // Add the press key message with a highlight
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
        AudioManager.Music.FadeOutMusic(fadeDuration + 3);
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
            // Play both ambient sounds with fade-in effects
            // Use default track volume from inspector by using volume = 1.0f
            AudioManager.Music.Play(trackName: "ambient.computer_hum", fadeIn: 1.0f);
            AudioManager.Music.Play(trackName: "ambient.texture_2", fadeIn: 1.5f);
            
            // Activate the overlay but set alpha to 0
            scanlineOverlay.gameObject.SetActive(true);
            Color startColor = scanlineOverlay.color;
            startColor.a = 0f;
            scanlineOverlay.color = startColor;

            // Define the final subtle alpha - make it extremely subtle
            float targetAlpha = 4f / 255f; // Even more subtle than before

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
                Color flashColor = new Color(0.7f, 0.7f, 0.75f, 0.5f); // Reduced alpha for less brightness
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

            // Use much more subtle values for the scanline effect
            float initialOverlayAlpha = 0.15f; // Reduced from 0.4f for less brightness
            float initialGlitch = baseGlitchIntensity * 3f;

            // Set the initial effect - make it subtle
            Color overlayColor = scanlineOverlay.color;
            overlayColor.a = initialOverlayAlpha;
            scanlineOverlay.color = overlayColor;

            // Apply initial shader effects if material exists
            if (shaderMaterial != null)
            {
                shaderMaterial.SetFloat("_GlitchIntensity", initialGlitch);
                // Use less extreme values for more subtlety
                shaderMaterial.SetFloat("_Intensity", originalIntensity * 1.5f); // Reduced from 2.0f
                shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX * 1.3f); // Reduced from 1.5f
                shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY * 1.3f); // Reduced from 1.5f

                // Play random glitch sound
                PlayRandomGlitchSound(0.7f);
            }

            // Flicker phase - mimics old CRT flickering as it warms up
            int flickerSteps = 3;
            for (int i = 0; i < flickerSteps; i++)
            {
                // Instead of just opacity changes, add color tinting during flickers
                float dimDuration = 0.05f;
                elapsedTime = 0f;
                Color tintColor = (i % 2 == 0) ? 
                    new Color(1.0f, 0.9f, 0.9f, initialOverlayAlpha * 0.7f) : // Slight red tint
                    new Color(0.9f, 0.9f, 1.0f, initialOverlayAlpha * 0.7f);  // Slight blue tint
                
                while (elapsedTime < dimDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float dimT = Mathf.Clamp01(elapsedTime / dimDuration);

                    // Use color transitions instead of just alpha
                    overlayColor = Color.Lerp(scanlineOverlay.color, tintColor, dimT);
                    scanlineOverlay.color = overlayColor;

                    yield return null;
                }

                // Play random glitch sound at the end of the dim phase
                if (Random.value < 0.7f)
                {
                    PlayRandomGlitchSound(0.5f);
                }

                // Brighten again - return to normal color
                float brightenDuration = 0.07f;
                elapsedTime = 0f;
                Color normalColor = new Color(1f, 1f, 1f, initialOverlayAlpha);
                
                while (elapsedTime < brightenDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float brightenT = Mathf.Clamp01(elapsedTime / brightenDuration);

                    // Return to normal color
                    overlayColor = Color.Lerp(scanlineOverlay.color, normalColor, brightenT);
                    scanlineOverlay.color = overlayColor;

                    yield return null;
                }
                
                // Add occasional shader glitch during the flicker sequence
                if (Random.value < 0.5f && shaderMaterial != null)
                {
                    // Create a brief texture distortion
                    float originalOffset = shaderMaterial.GetFloat("_Offset");
                    shaderMaterial.SetFloat("_Offset", Random.Range(0f, 1f));
                    yield return new WaitForSeconds(0.03f);
                    shaderMaterial.SetFloat("_Offset", originalOffset);
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

                // Update the overlay alpha - gradually reducing from initial value to the very subtle target
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
        // Keep the original messages but with faster typing speed
        var bootMessages = new (string message, Color color, float duration, bool hasLoadingBar, bool isError, bool instantLoad)[]
        {
            ($"[BOOT] Purrine_AI-B1-08364142020", bootColor, 3f, false, false, false),
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
            ("[NET] Scanning for available networks...", statusColor, 0.9f, true, false, false),
            ("[DIAG] Network scan in progress...", systemInfoColor, 0.3f, false, false, true),
            ("[NET] Scan range: 500m", systemInfoColor, 0.2f, false, false, true),
            ("[NET] Frequency range: 1GHz - 10THz", systemInfoColor, 0.3f, false, false, true),
            ("[NET] Signal analysis protocol active", systemInfoColor, 0.2f, false, false, true),
            ("[DIAG] Scanning frequency band 1...", systemInfoColor, 0.6f, true, false, false),
            ("[NET] No networks found in frequency band 1", systemInfoColor, 0.2f, false, false, true),
            ("[DIAG] Scanning frequency band 2...", systemInfoColor, 0.5f, true, false, false),
            ("[NET] No networks found in frequency band 2", systemInfoColor, 0.2f, false, false, true),
            ("[LOG] Expanding search parameters...", statusColor, 0.3f, false, false, true),
            ("[DIAG] Scanning frequency band 3...", systemInfoColor, 0.7f, true, false, false),
            ("[ALERT] Faint signal detected", errorColor, 0.2f, false, false, true),
            ("[NET] Signal strength: 12% - insufficient", systemInfoColor, 0.2f, false, false, true),
            ("[NET] Attempting signal amplification", statusColor, 0.4f, true, false, false),
            ("[NET] Signal loss - connection attempt failed", errorColor, 0.2f, false, false, true),
            ("[NET] Continuing network scan...", systemInfoColor, 0.2f, false, false, true),
            ("[DIAG] Adjusting antenna sensitivity", systemInfoColor, 0.3f, false, false, true),
            ("[DIAG] Scanning frequency band 4...", systemInfoColor, 0.6f, true, false, false),
            ("[ALERT] Unknown signal type detected", bootColor, 0.2f, false, false, true),
            ("[NET] Signal origin: Non-quantum network", statusColor, 0.2f, false, false, true),
            ("[NET] Signal protocol: TCP/IP variant", systemInfoColor, 0.2f, false, false, true),
            ("[LOG] Analyzing signal compatibility...", statusColor, 0.5f, true, false, false),
            ("[NET] Protocol conversion module activated", systemInfoColor, 0.3f, false, false, true),
            ("[NET] Signal strength: 67% - marginal", systemInfoColor, 0.2f, false, false, true),
            ("[NET] Attempting enhanced signal acquisition", statusColor, 0.6f, true, false, false),
            ("[NET] Connection established to unknown network", statusColor, 0.2f, false, false, true),
            ("[ALERT] Signal strength increasing: 84%", systemInfoColor, 0.2f, false, false, true),
            ("[DIAG] Running network diagnostics", systemInfoColor, 0.5f, true, false, false),
            ("[NET] Found 1 potential connection", statusColor, 0.2f, false, false, true),
            ("[NET] Network type: Local area connection", systemInfoColor, 0.2f, false, false, true),
            ("[NET] Signal strength: Excellent (98%)", systemInfoColor, 0.2f, false, false, true),
            ("[NET] Network scan complete", statusColor, 0.3f, false, false, true),
            ("[DIAG] Detected endpoint device: Home computer system", systemInfoColor, 0.3f, false, false, true),
            ("[DIAG] Last active: <3 mins ago", systemInfoColor, 0.2f, false, false, true),
            ("[ALERT] External system detected. Authentication required.", errorColor, 0.4f, false, false, true),
            ("[SYS] Ready to establish connection.", readyColor, 0.6f, false, false, false),
            // Connection confirmation message will appear here - handled by the "Press ENTER to connect" prompt
            // System info will appear after connection is confirmed
            ("[LOG] Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), systemInfoColor, 0f, false, false, true),
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

        yield return new WaitForSeconds(0.3f);  // Initial pause before starting (shortened)
        
        AudioManager.SFX.Play("confirm_1", 0.3f);

        // Track boot progress for visual effects
        float overallProgress = 0f;

        foreach (var (message, color, duration, hasLoadingBar, isError, instantLoad) in bootMessages)
        {
            // Update progress for each message (normalized 0-1)
            overallProgress += 1f / bootMessages.Length;
            
            // Set glitch intensity based on whether this is an error message
            if (shaderMaterial != null)
            {
                // Smoothly transition glitch intensity
                float targetGlitch = isError ? errorGlitchIntensity : baseGlitchIntensity;
                StartCoroutine(TransitionGlitchIntensity(targetGlitch, isError ? 0.2f : 0.5f));

                // Add more interesting visual effects - color distortion and scanline jitter
                if (isError)
                {
                    TriggerGlitchEffect(errorGlitchIntensity * 1.5f, 0.15f);
                }
            }

            // Add prefix with caret with explicit color to prevent inheriting previous text color
            string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
            consoleText.text += $"<color=#{caretColorHex}>> </color>";

            // Convert color to hex for rich text
            string colorHex = ColorUtility.ToHtmlStringRGB(color);

            if (instantLoad)
            {
                // For instant loading messages, just add the whole text at once
                consoleText.text += $"<color=#{colorHex}>{message}</color>\n";

                // Add to history - now the caret is included in the explicit color
                consoleHistory.Add($"<color=#{caretColorHex}>> </color>" + $"<color=#{colorHex}>{message}</color>");

                // Trim if needed
                if (consoleHistory.Count > maxConsoleLines)
                {
                    consoleHistory.RemoveAt(0);
                    consoleText.text = string.Join("\n", consoleHistory) + "\n";
                }

                // Short delay between instant messages for readability
                yield return new WaitForSeconds(0.02f); // Reduced from 0.05f
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

                    // Variable typing speed but faster on average
                    float typingVariation = Random.Range(0.7f, 1.1f); // Reduced range

                    // Play sound effect when an error message is fully typed
                    if (i == 7 && message.StartsWith("[ERROR]"))
                    {
                        AudioManager.SFX.Play("negative_2", 0.3f);
                    }

                    // Type faster for longer messages
                    float speedMultiplier = message.Length > 30 ? 0.6f : 1.0f;
                    
                    // Occasional slight pause during typing (feels more like real typing)
                    if (Random.value < 0.03f && !isError) // Reduced from 0.05f
                    {
                        yield return new WaitForSeconds(textTypeSpeed * 2f); // Reduced from 3f
                    }
                    else
                    {
                        yield return new WaitForSeconds(textTypeSpeed * typingVariation * speedMultiplier);
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
                    float targetTime = duration * 1.2f; // Slightly shorter loading time (was 1.5f)
                    int currentBars = 0;

                    // Track which segments are filled for pulse effect
                    bool[] filledSegments = new bool[barLength];
                    
                    // Pulse animation parameters
                    float pulseSpeed = 8f;
                    float pulseTime = 0f;

                    while (currentBars < barLength)
                    {
                        // Create variable loading speeds - sometimes fast, sometimes stalled
                        float progressSpeed = Random.value < 0.2f ?
                            Random.Range(0.05f, 0.08f) :  // Slower progress (20% chance)
                            Random.Range(0.15f, 0.35f);   // Normal progress (80% chance) - faster than before

                        // Occasionally add multiple bars at once (progress spike)
                        int barsToAdd = Random.value < 0.2f ? // Increased chance from 0.15f
                            Random.Range(2, 5) : // Progress spike (20% chance) - can add more bars
                            1;                   // Normal progress (80% chance)

                        // Ensure we don't exceed the total
                        barsToAdd = Mathf.Min(barsToAdd, barLength - currentBars);

                        if (barsToAdd > 0)
                        {
                            string barSegment = new string('=', barsToAdd);
                            consoleText.text += barSegment;
                            
                            // Mark these segments as filled
                            for (int i = 0; i < barsToAdd; i++)
                            {
                                if (currentBars + i < barLength)
                                    filledSegments[currentBars + i] = true;
                            }
                            
                            currentBars += barsToAdd;

                            // Calculate progress percentage (0-100)
                            int percentage = Mathf.RoundToInt((float)currentBars / barLength * 100f);

                            // Update the completion percentage
                            string textWithoutPercentage = consoleText.text;
                            
                            // Add pulsing animation to the loading text by varying brightness
                            pulseTime += Time.deltaTime * pulseSpeed;
                            float pulseValue = Mathf.PingPong(pulseTime, 0.4f) + 0.6f; // Oscillates between 0.6 and 1.0
                            float hue, saturation, value;
                            Color.RGBToHSV(color, out hue, out saturation, out value);
                            Color pulseColor = Color.HSVToRGB(hue, saturation, value * pulseValue);
                            string pulseColorHex = ColorUtility.ToHtmlStringRGB(pulseColor);
                            
                            consoleText.text = $"{textWithoutPercentage}] <color=#{pulseColorHex}>{percentage}%</color>";

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
                        if (Random.value < 0.08f && currentBars < barLength) // Reduced from 0.1f
                        {
                            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f)); // Reduced from 0.2f-0.5f

                            // During stalls, sometimes show a system message
                            if (Random.value < 0.3f)
                            {
                                string stall = currentBars > barLength / 2 ?
                                    $"\n      <color=#888888>...{(Random.value < 0.5f ? "validating" : "processing")}</color>" :
                                    $"\n      <color=#888888>...{(Random.value < 0.5f ? "collecting" : "analyzing")}</color>";
                                consoleText.text += stall;
                                yield return new WaitForSeconds(0.2f); // Reduced from 0.3f

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
                        // Add a success animation - quick color flash
                        string successColorHex = ColorUtility.ToHtmlStringRGB(Color.Lerp(color, Color.white, 0.5f));
                        consoleText.text = consoleText.text.Substring(0, endIndex + 1) + $" <color=#{successColorHex}>100%</color>";
                        
                        // Play a success sound
                        AudioManager.SFX.Play("confirm_1", 0.2f);
                        
                        yield return new WaitForSeconds(0.1f);
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
                    // Add to history if no loading bar - now with explicit caret color
                    consoleHistory.Add($"<color=#{caretColorHex}>> </color>" + messageText);
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
                float messageDelay = lineDelay * 0.8f + Random.Range(0f, 0.2f); // Reduced delay
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
            
            // Add subtle visual feedback based on progress
            if (scanlineOverlay != null && shaderMaterial != null)
            {
                // Very subtle scanline intensity during boot - make it barely visible
                float scanlineAlpha = Mathf.Lerp(0.06f, 0.04f, overallProgress); // Extremely subtle values
                Color overlayColor = scanlineOverlay.color;
                overlayColor.a = scanlineAlpha;
                scanlineOverlay.color = overlayColor;
                
                // Visual interest: Occasionally add a quick color tint on major progress points
                if (message.StartsWith("[INIT]") && Random.value < 0.3f)
                {
                    // Quick subtle blue tint to suggest initialization
                    StartCoroutine(QuickOverlayTint(new Color(0.9f, 0.95f, 1.0f, scanlineAlpha * 1.2f), 0.2f));
                }
                else if (message.StartsWith("[ERROR]"))
                {
                    // Quick red tint for errors
                    StartCoroutine(QuickOverlayTint(new Color(1.0f, 0.9f, 0.9f, scanlineAlpha * 1.5f), 0.3f));
                }
                else if (message.Contains("COMPLETED") || message.Contains("[OK]"))
                {
                    // Quick green tint for completions
                    StartCoroutine(QuickOverlayTint(new Color(0.9f, 1.0f, 0.9f, scanlineAlpha * 1.2f), 0.2f));
                }
                
                // Gradually reduce curvature as system stabilizes
                shaderMaterial.SetFloat("_CurvatureX", Mathf.Lerp(baseCurvatureX * 1.5f, baseCurvatureX, overallProgress));
                shaderMaterial.SetFloat("_CurvatureY", Mathf.Lerp(baseCurvatureY * 1.5f, baseCurvatureY, overallProgress));
            }
        }

        // Reset glitch to base level
        if (shaderMaterial != null)
        {
            StartCoroutine(TransitionGlitchIntensity(baseGlitchIntensity, 1.0f));
        }

        // Final initialization success sound
        AudioManager.SFX.Play("confirm_3", 0.4f);
        
        // Add an extra line break for spacing
        consoleText.text += "\n";
        
        // Save the index where the "Press Any Key" message would start
        // But don't add it yet - we'll let UpdatePressKeyVisibility handle this
        pressKeyTextStartIndex = consoleText.text.Length;
        
        // Enable scene transition
        canProceed = true;
        isTyping = false;  // End typing sequence

        AudioManager.SFX.Play("glass_5", 0.5f);
        
        // Add a subtle "ready" glow effect to the screen
        if (transitionImage != null)
        {
            Color softGlow = new Color(0.1f, 0.3f, 0.1f, 0.05f);
            transitionImage.color = softGlow;
            float glowDuration = 0.5f;
            float elapsedTime = 0f;
            
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / glowDuration;
                
                // Pulse glow effect
                float pulse = Mathf.Sin(t * Mathf.PI * 2) * 0.5f + 0.5f;
                transitionImage.color = new Color(softGlow.r, softGlow.g, softGlow.b, softGlow.a * pulse);
                
                yield return null;
            }
            
            // Fade out glow
            elapsedTime = 0f;
            float fadeDuration = 0.5f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                
                transitionImage.color = Color.Lerp(softGlow, Color.clear, t);
                
                yield return null;
            }
            
            transitionImage.color = Color.clear;
        }
        
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
            
            // Add more interesting visual effects - color distortion and scanline jitter
            if (scanlineOverlay != null)
            {
                Color originalColor = scanlineOverlay.color;
                float originalAlpha = originalColor.a;
                
                // Brief color tint to the scanlines (subtle but noticeable)
                scanlineOverlay.color = new Color(
                    1.0f, // Red tint
                    originalColor.g * 0.8f,
                    originalColor.b * 0.8f,
                    originalAlpha * 1.2f // Slightly more visible but not too bright
                );
                
                // Wait a very short time
                yield return new WaitForSeconds(duration * 0.2f);
                
                // Return to normal color but keep the effect going
                scanlineOverlay.color = originalColor;
            }
            
            // Brief distortion of text if it's active
            if (consoleText != null && consoleText.gameObject.activeSelf && Random.value < 0.7f)
            {
                RectTransform textRect = consoleText.GetComponent<RectTransform>();
                Vector2 originalPos = textRect.anchoredPosition;
                
                // Quick jitter
                for (int i = 0; i < 3; i++)
                {
                    // Scale jitter based on intensity
                    float jitterAmount = intensity * 5f;
                    textRect.anchoredPosition = originalPos + new Vector2(
                        Random.Range(-jitterAmount, jitterAmount),
                        Random.Range(-jitterAmount, jitterAmount)
                    );
                    yield return new WaitForSeconds(duration * 0.05f);
                }
                
                // Reset position
                textRect.anchoredPosition = originalPos;
            }
            
            // Wait for the rest of the duration
            yield return new WaitForSeconds(duration * 0.6f);
            
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
        // Add a pre-shutdown sequence - screen flicker and destabilization before turning off
        // This makes the shutdown feel less sudden and more natural
        if (shaderMaterial != null)
        {
            // Store original values
            float originalGlitchIntensity = shaderMaterial.GetFloat("_GlitchIntensity");
            float originalGlitchSpeed = shaderMaterial.GetFloat("_GlitchSpeed");
            float originalCurvatureX = shaderMaterial.GetFloat("_CurvatureX");
            float originalCurvatureY = shaderMaterial.GetFloat("_CurvatureY");
            
            // Store the original position of the console text
            Vector2 originalTextPosition = Vector2.zero;
            if (consoleText != null)
            {
                RectTransform textRect = consoleText.GetComponent<RectTransform>();
                originalTextPosition = textRect.anchoredPosition;
                
                // Store original text for later scrambling
                string originalText = consoleText.text;
            }
            
            // Initial phase: Everything seems fine for a moment (normal operation)
            AudioManager.SFX.Play("bong_1", 0.2f);
            
            // Brief normal pause as if everything is fine
            yield return new WaitForSeconds(0.8f);
            
            // Slight unexpected glitch - first sign something is wrong
            TriggerGlitchEffect(0.2f, 0.1f);
            
            // First text corruption - just a character or two
            if (consoleText != null)
            {
                string currentText = consoleText.text;
                // Corrupt just a few characters (1%)
                consoleText.text = CorruptText(currentText, 0.01f);
            }
            
            // Return to normal briefly
            yield return new WaitForSeconds(0.6f);
            
            // Another small glitch - things are getting unstable
            TriggerGlitchEffect(0.25f, 0.15f);
            
            // Second text corruption - a bit more noticeable
            if (consoleText != null)
            {
                string currentText = consoleText.text;
                // Corrupt more characters (3%)
                consoleText.text = CorruptText(currentText, 0.03f);
            }
            
            // Brief pause
            yield return new WaitForSeconds(0.4f);
            
            // Stage 1: Subtle instability (small flicker) - keep text visible
            PlayRandomGlitchSound(0.3f);
            AudioManager.SFX.Play("pink_noise_2", 0.4f);
            
            // First subtle shader adjustment
            shaderMaterial.SetFloat("_GlitchIntensity", originalGlitchIntensity * 1.5f);
            shaderMaterial.SetFloat("_GlitchSpeed", originalGlitchSpeed * 1.2f);
            
            // Small screen flicker
            if (transitionImage != null)
            {
                Color originalColor = transitionImage.color;
                Color flickerColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);
                transitionImage.color = flickerColor;
                yield return new WaitForSeconds(0.04f);
                transitionImage.color = originalColor;
            }
            
            // More text corruption
            if (consoleText != null)
            {
                string currentText = consoleText.text;
                // More aggressive corruption (7%)
                consoleText.text = CorruptText(currentText, 0.07f);
            }
            
            yield return new WaitForSeconds(0.4f);
            
            // Stage 2: Medium instability (more obvious glitches) - text still visible
            PlayRandomGlitchSound(0.4f);
            
            // Second shader adjustment
            shaderMaterial.SetFloat("_GlitchIntensity", originalGlitchIntensity * 2.5f);
            shaderMaterial.SetFloat("_GlitchSpeed", originalGlitchSpeed * 1.8f);
            shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX * 1.3f);
            shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY * 1.3f);
            
            // Medium screen flicker with text jitter
            if (transitionImage != null && consoleText != null)
            {
                // Brief text jitter with progressive corruption
                RectTransform textRect = consoleText.GetComponent<RectTransform>();
                
                // Jitter text position a few times and increase corruption
                for (int i = 0; i < 3; i++)
                {
                    textRect.anchoredPosition = originalTextPosition + new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
                    
                    // Progressive text corruption during jitter
                    string currentText = consoleText.text;
                    float corruptionLevel = 0.1f + (i * 0.05f); // Increasing corruption (10%, 15%, 20%)
                    consoleText.text = CorruptText(currentText, corruptionLevel);
                    
                    yield return new WaitForSeconds(0.05f);
                }
                
                textRect.anchoredPosition = originalTextPosition;
                
                // Medium flicker
                Color originalColor = transitionImage.color;
                Color flickerColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
                transitionImage.color = flickerColor;
                yield return new WaitForSeconds(0.06f);
                transitionImage.color = originalColor;
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // Final pre-shutdown warning
            // Trigger one medium glitch line while text is still visible
            StartCoroutine(CreatePreShutdownGlitchLine(transitionImage.transform));
            PlayRandomGlitchSound(0.5f);
            
            // Heavy text corruption
            if (consoleText != null)
            {
                string currentText = consoleText.text;
                // Very heavy corruption (25%)
                consoleText.text = CorruptText(currentText, 0.25f);
            }
            
            yield return new WaitForSeconds(0.15f);
        }
        
        // Now proceed with more intense glitches while text is still visible
        // Play switch-off sound
        AudioManager.SFX.Play("switch_6", 0.2f);
        
        // Fade out computer hum sound
        AudioManager.Music.FadeOutMusic(0.5f); // Faster fade of music
        
        // Initial glitch sound
        PlayRandomGlitchSound(0.6f);
        
        AudioManager.SFX.Play("pink_noise_2", 0.2f);
        
        // Let the sound play for a moment before effects start
        yield return new WaitForSeconds(0.1f);
        
        // Start progressive text corruption for the final phase
        if (consoleText != null)
        {
            StartCoroutine(ProgressiveTextCorruption(0.7f));
        }
        
        // Increase CRT glitch effects while text is still visible
        if (shaderMaterial != null)
        {
            // Store original values
            float originalGlitchIntensity = shaderMaterial.GetFloat("_GlitchIntensity");
            float originalGlitchSpeed = shaderMaterial.GetFloat("_GlitchSpeed");
            float originalCurvatureX = shaderMaterial.GetFloat("_CurvatureX");
            float originalCurvatureY = shaderMaterial.GetFloat("_CurvatureY");
            
            // Increase values for shutdown effect
            shaderMaterial.SetFloat("_GlitchIntensity", originalGlitchIntensity * 5f);
            shaderMaterial.SetFloat("_GlitchSpeed", originalGlitchSpeed * 4f);
            shaderMaterial.SetFloat("_CurvatureX", originalCurvatureX * 2.5f);
            shaderMaterial.SetFloat("_CurvatureY", originalCurvatureY * 2.5f);
        }

        // Add glitch lines while the text is still visible
        StartCoroutine(CreateGlitchLines(transitionImage.transform, 0.8f));
        
        // Rapidly jitter the text during the glitch effect
        if (consoleText != null)
        {
            RectTransform textRect = consoleText.GetComponent<RectTransform>();
            Vector2 originalPos = textRect.anchoredPosition;
            
            // Violent text jitter for a brief moment
            float jitterDuration = 0.4f;
            float jitterElapsed = 0f;
            
            while (jitterElapsed < jitterDuration)
            {
                jitterElapsed += Time.deltaTime;
                
                // More extreme jitter as time progresses
                float intensity = Mathf.Lerp(3f, 12f, jitterElapsed / jitterDuration);
                textRect.anchoredPosition = originalPos + new Vector2(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity)
                );
                
                // Random color distortion
                if (Random.value < 0.3f)
                {
                    consoleText.color = new Color(
                        1f,
                        Random.Range(0.8f, 1f),
                        Random.Range(0.8f, 1f),
                        1f
                    );
                }
                else
                {
                    consoleText.color = Color.white;
                }
                
                // Extreme text corruption during violent jitter
                string currentText = consoleText.text;
                float corruptionFactor = Mathf.Lerp(0.3f, 0.7f, jitterElapsed / jitterDuration);
                consoleText.text = CorruptText(currentText, corruptionFactor);
                
                yield return null;
            }
            
            // Reset position before the final glitch
            textRect.anchoredPosition = originalPos;
            consoleText.color = Color.white;
            
            // Maximum corruption at the end - almost unreadable
            consoleText.text = CorruptText(consoleText.text, 0.9f);
        }
        
        // Final massive glitch before shutdown
        TriggerGlitchEffect(1.5f, 0.15f);
        PlayRandomGlitchSound(0.8f);
        
        yield return new WaitForSeconds(0.15f);
        
        // Screen tear effect - optional but looks cool
        if (consoleText != null && transitionImage != null)
        {
            // Create a quick "screen tear" effect
            GameObject tearObj = new GameObject("ScreenTear");
            tearObj.transform.SetParent(transitionImage.transform, false);
            Image tearImage = tearObj.AddComponent<Image>();
            tearImage.color = new Color(1f, 1f, 1f, 0.8f);
            
            RectTransform tearRect = tearImage.GetComponent<RectTransform>();
            tearRect.anchorMin = new Vector2(0, 0.4f);
            tearRect.anchorMax = new Vector2(1, 0.6f);
            tearRect.sizeDelta = new Vector2(0, 20f);
            
            // Quick flash of the tear
            yield return new WaitForSeconds(0.05f);
            PlayRandomGlitchSound(1.0f);
            
            // Destroy the tear
            Destroy(tearObj);
        }
        
        // SUDDEN BLACK SCREEN - simulating monitor abruptly losing power
        if (transitionImage != null)
        {
            // Hard cut to black
            transitionImage.color = Color.black;
            
            // Hide console text and scanlines immediately
            if (consoleText != null)
            {
                consoleText.gameObject.SetActive(false);
            }
            
            if (scanlineOverlay != null)
            {
                scanlineOverlay.gameObject.SetActive(false);
            }
            
            // One final loud glitch sound on shutdown
            PlayRandomGlitchSound(1.0f);
        }
        
        // Brief pause in darkness before scene transition
        yield return new WaitForSeconds(0.5f);
        
        // Now load the next scene
        SceneManager.LoadScene("IntroAndTutorial");
    }

    // Helper method to corrupt text with random characters
    private string CorruptText(string text, float corruptionFactor)
    {
        // Handle empty or null text
        if (string.IsNullOrEmpty(text)) return text;
        
        // Convert to char array for manipulation
        char[] chars = text.ToCharArray();
        
        // Determine how many characters to corrupt
        int corruptCount = Mathf.FloorToInt(chars.Length * corruptionFactor);
        
        // Protect against corrupting rich text tags - store their positions
        List<int> protectedIndices = new List<int>();
        bool inTag = false;
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '<') inTag = true;
            if (inTag) protectedIndices.Add(i);
            if (chars[i] == '>') inTag = false;
        }
        
        // Scramble random characters
        for (int i = 0; i < corruptCount; i++)
        {
            // Pick a random position that's not in a rich text tag
            int attempts = 0;
            int pos;
            do
            {
                pos = Random.Range(0, chars.Length);
                attempts++;
                // Give up after too many attempts to avoid infinite loops
                if (attempts > 50) break;
            } while (protectedIndices.Contains(pos));
            
            // Skip if we couldn't find a valid position
            if (attempts > 50) continue;
            
            // Replace with a random ASCII character - including symbols and control chars for more visual chaos
            // Weighted toward more visually interesting characters
            float randomType = Random.value;
            if (randomType < 0.4f)
            {
                // Standard ASCII
                chars[pos] = (char)Random.Range(33, 126);
            }
            else if (randomType < 0.7f)
            {
                // Control characters and extended ASCII that look "glitchy"
                chars[pos] = (char)Random.Range(176, 223);
            }
            else if (randomType < 0.85f)
            {
                // Special characters that create visual noise
                char[] specialChars = new char[] { '▓', '▒', '░', '█', '■', '□', '▪', '▫', '◊', '○', '●', '◘', '◙', '♦', '¶' };
                chars[pos] = specialChars[Random.Range(0, specialChars.Length)];
            }
            else
            {
                // Just scrambled versions of normal characters
                chars[pos] = (char)(chars[pos] + Random.Range(-5, 5));
            }
        }
        
        // Convert back to string
        return new string(chars);
    }
    
    // Progressive text corruption for the final shutdown sequence
    private IEnumerator ProgressiveTextCorruption(float duration)
    {
        if (consoleText == null) yield break;
        
        string originalText = consoleText.text;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Exponential corruption - starts slow, gets very aggressive
            float corruptionFactor = Mathf.Pow(progress, 2) * 0.6f; // Up to 60% corruption
            
            // Apply corruption
            consoleText.text = CorruptText(originalText, corruptionFactor);
            
            // Brief pause between corruption updates
            yield return new WaitForSeconds(0.05f);
        }
    }

    // New method to show "Proceeding..." message before shutdown
    private IEnumerator ShowProceedingMessage()
    {
        // Play confirmation sound
        AudioManager.SFX.Play("confirm_2", 0.3f);
        
        if (consoleText != null)
        {
            // Get current text and ensure cursor is removed if present
            string currentText = consoleText.text;
            if (currentText.EndsWith(cursorChar))
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
            }
            
            // Remove the "PRESS ENTER TO CONNECT" message
            int pressKeyIndex = currentText.LastIndexOf(pressAnyKeyMessage);
            if (pressKeyIndex >= 0)
            {
                currentText = currentText.Substring(0, pressKeyIndex);
            }
            
            // Add the "Proceeding..." message with a different color (yellow/gold)
            Color proceedingColor = new Color(0.9f, 0.8f, 0.2f); // Gold/yellow color
            string proceedingMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(proceedingColor)}>Proceeding...</color>";
            string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
            consoleText.text = currentText + $"\n<color=#{caretColorHex}>> </color>" + proceedingMessage;
            
            // Show cursor blinking at the end of the message
            StartCoroutine(BlinkCursorAfterProceeding(currentText, proceedingMessage));
            
            // Wait a moment before starting the shutdown sequence
            yield return new WaitForSeconds(1.5f);
        }
        
        // Start power-off sequence
        StartCoroutine(MonitorPowerOffSequence());
    }
    
    // New method to blink the cursor after the "Proceeding..." message
    private IEnumerator BlinkCursorAfterProceeding(string baseText, string proceedingMessage)
    {
        bool localCursorVisible = true;
        float elapsed = 0f;
        float totalTime = 1.5f; // Match the delay before shutdown
        string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        
        while (elapsed < totalTime)
        {
            // Toggle cursor visibility
            localCursorVisible = !localCursorVisible;
            
            // Update text with or without cursor
            if (localCursorVisible)
            {
                consoleText.text = baseText + $"\n<color=#{caretColorHex}>> </color>" + proceedingMessage + cursorChar;
            }
            else
            {
                consoleText.text = baseText + $"\n<color=#{caretColorHex}>> </color>" + proceedingMessage;
            }
            
            // Wait for blink interval
            yield return new WaitForSeconds(cursorBlinkSpeed);
            elapsed += cursorBlinkSpeed;
        }
    }

    // Helper method to play a random glitch sound effect
    private void PlayRandomGlitchSound(float volume = 0.4f)
    {
        int soundNumber = Random.Range(1, 5); // Random number between 1 and 4
        string soundName = "glitch_" + soundNumber;
        AudioManager.SFX.Play(soundName, volume);
    }

    // Add this helper method for quick color tints to add visual interest during boot
    private IEnumerator QuickOverlayTint(Color targetColor, float duration)
    {
        if (scanlineOverlay == null) yield break;
        
        Color originalColor = scanlineOverlay.color;
        float elapsedTime = 0f;
        
        // Fade to target color
        while (elapsedTime < duration * 0.4f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / (duration * 0.4f));
            
            scanlineOverlay.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }
        
        // Hold briefly
        yield return new WaitForSeconds(duration * 0.2f);
        
        // Fade back
        elapsedTime = 0f;
        while (elapsedTime < duration * 0.4f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / (duration * 0.4f));
            
            scanlineOverlay.color = Color.Lerp(targetColor, originalColor, t);
            yield return null;
        }
        
        // Ensure we return to original color
        scanlineOverlay.color = originalColor;
    }

    // Create a single pre-shutdown glitch line
    private IEnumerator CreatePreShutdownGlitchLine(Transform parent)
    {
        // Get dimensions
        RectTransform canvasRect = parent.GetComponent<RectTransform>();
        float screenHeight = canvasRect.rect.height;
        
        // Create the glitch line
        GameObject lineObj = new GameObject("PreShutdownGlitch");
        lineObj.transform.SetParent(parent, false);
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = new Color(1f, 1f, 1f, 0.8f);
        
        RectTransform lineRect = lineImage.GetComponent<RectTransform>();
        
        // Horizontal line
        lineRect.anchorMin = new Vector2(0, 0);
        lineRect.anchorMax = new Vector2(1, 0);
        
        // Position in the middle-ish of screen
        float yPos = Random.Range(0.4f, 0.6f);
        lineRect.anchoredPosition = new Vector2(0, screenHeight * yPos);
        
        // Thin line
        lineRect.sizeDelta = new Vector2(0, 1.5f);
        
        // Glitch appearance
        float lifetime = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            
            // Random flickering
            if (Random.value < 0.4f)
            {
                lineImage.enabled = !lineImage.enabled;
            }
            
            // Occasional position shift
            if (Random.value < 0.3f)
            {
                lineRect.anchoredPosition = new Vector2(0, screenHeight * (yPos + Random.Range(-0.02f, 0.02f)));
            }
            
            yield return null;
        }
        
        // Destroy the line
        Destroy(lineObj);
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
        float intensityMultiplier = 2.0f; // More intense glitches overall
        
        // Define the glitch intensity over time - quicker and more intense sequence
        // 0-0.2: Initial subtle glitches
        // 0.2-0.5: Increasing intensity
        // 0.5-0.7: Peak intensity with bursts
        // 0.7-1.0: Quick fadeout
        
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
                // Start with slightly more glitching immediately
                baseChance = 0.02f * intensityMultiplier;
                burstChance = 0.01f * intensityMultiplier;
                
                // Create more lines from the start
                if (Random.value < baseChance && glitchLines.Count < 2)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.2f);
                }
            }
            // Phase 2: Building up (20-50% of duration)
            else if (normalizedTime < 0.5f)
            {
                // More rapidly increasing intensity
                float progress = (normalizedTime - 0.2f) / 0.3f; // 0 to 1 within this phase
                baseChance = Mathf.Lerp(0.02f, 0.08f, progress) * intensityMultiplier;
                burstChance = Mathf.Lerp(0.01f, 0.04f, progress) * intensityMultiplier;
                
                // Create more lines
                if (Random.value < baseChance && glitchLines.Count < 4)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.3f);
                }
                
                // More frequent bursts
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(2, 5);
                    
                    // Chance to play a glitch sound for the burst
                    if (Random.value < 0.5f)
                    {
                        PlayRandomGlitchSound(0.5f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.3f);
                        yield return new WaitForSeconds(0.01f); // Faster bursts
                    }
                }
            }
            // Phase 3: Peak intensity (50-70% of duration)
            else if (normalizedTime < 0.7f)
            {
                // Maximum intensity phase
                baseChance = 0.12f * intensityMultiplier;
                burstChance = 0.07f * intensityMultiplier;
                
                // Create many lines
                if (Random.value < baseChance && glitchLines.Count < 8)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.25f);
                }
                
                // Very frequent bursts
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(3, 7);
                    
                    // Higher chance to play a glitch sound for the burst
                    if (Random.value < 0.7f)
                    {
                        PlayRandomGlitchSound(0.6f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.2f);
                        yield return new WaitForSeconds(0.01f); // Quick burst
                    }
                }
                
                // More frequent shader glitches
                if (Random.value < 0.05f && shaderMaterial != null)
                {
                    TriggerGlitchEffect(Random.Range(0.7f, 1.2f), 0.08f);
                }
            }
            // Phase 4: Rapid fadeout (70-100% of duration)
            else
            {
                // Quick decrease in intensity
                float progress = (normalizedTime - 0.7f) / 0.3f; // 0 to 1 within this phase
                baseChance = Mathf.Lerp(0.12f, 0.01f, progress) * intensityMultiplier;
                burstChance = Mathf.Lerp(0.07f, 0.005f, progress) * intensityMultiplier;
                
                // Create occasional lines as we fade out
                if (Random.value < baseChance && glitchLines.Count < 5)
                {
                    CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.15f);
                }
                
                // A few final bursts
                if (Random.value < burstChance)
                {
                    int burstSize = Random.Range(2, 5);
                    
                    // Occasional final glitch sound
                    if (Random.value < 0.3f)
                    {
                        PlayRandomGlitchSound(0.7f);
                    }
                    
                    for (int i = 0; i < burstSize; i++)
                    {
                        CreateGlitchLine(parent, glitchLines, screenHeight, screenWidth, 0.1f);
                        yield return new WaitForSeconds(0.005f); // Very fast final bursts
                    }
                    
                    // Chance of one final major glitch
                    if (Random.value < 0.2f && shaderMaterial != null)
                    {
                        TriggerGlitchEffect(1.5f, 0.1f);
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

    // New method to show connection successful and display connected system info
    private IEnumerator ShowConnectionSuccessful()
    {
        // Play connection sound
        AudioManager.SFX.Play("confirm_2", 0.3f);
        
        if (consoleText != null)
        {
            // Get current text and ensure cursor is removed if present
            string currentText = consoleText.text;
            if (currentText.EndsWith(cursorChar))
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
            }
            
            // Remove the connection prompt message
            int pressKeyIndex = currentText.LastIndexOf(pressAnyKeyMessage);
            if (pressKeyIndex >= 0)
            {
                currentText = currentText.Substring(0, pressKeyIndex);
            }
            
            // Add the "Connecting..." message with a color
            Color connectingColor = statusColor; // Blue status color
            string connectingMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(connectingColor)}>Connecting...</color>";
            string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
            consoleText.text = currentText + $"\n<color=#{caretColorHex}>> </color>" + connectingMessage;
            
            // Show connecting message with blinking cursor
            StartCoroutine(BlinkCursorDuringConnecting(currentText, connectingMessage));
            
            // Brief pause while "Connecting..."
            yield return new WaitForSeconds(1.0f);

            // Connection established sound
            AudioManager.SFX.Play("confirm_3", 0.3f);
            
            // Now display a successful connection message
            string successMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(readyColor)}>[SUCCESS] Connection established.</color>";
            currentText = consoleText.text;
            if (currentText.EndsWith(cursorChar))
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
            }
            consoleText.text = currentText + $"\n<color=#{caretColorHex}>> </color>" + successMessage;
            
            yield return new WaitForSeconds(0.5f);
            
            // Display system information of the connected PC
            yield return StartCoroutine(DisplayConnectedSystemInfo());
            
            // Wait a moment before starting the shutdown sequence
            yield return new WaitForSeconds(1.0f);
        }
        
        // Start power-off sequence
        StartCoroutine(MonitorPowerOffSequence());
    }
    
    // Helper method to display the blinking cursor during connecting
    private IEnumerator BlinkCursorDuringConnecting(string baseText, string connectingMessage)
    {
        bool localCursorVisible = true;
        float elapsed = 0f;
        float totalTime = 1.0f; // Match the delay during connecting
        string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        
        while (elapsed < totalTime)
        {
            // Toggle cursor visibility
            localCursorVisible = !localCursorVisible;
            
            // Update text with or without cursor
            if (localCursorVisible)
            {
                consoleText.text = baseText + $"\n<color=#{caretColorHex}>> </color>" + connectingMessage + cursorChar;
            }
            else
            {
                consoleText.text = baseText + $"\n<color=#{caretColorHex}>> </color>" + connectingMessage;
            }
            
            // Wait for blink interval
            yield return new WaitForSeconds(cursorBlinkSpeed);
            elapsed += cursorBlinkSpeed;
        }
    }
    
    // Display the connected system information
    private IEnumerator DisplayConnectedSystemInfo()
    {
        // Get real system information
        string osInfo = SystemInfo.operatingSystem;
        string cpuInfo = SystemInfo.processorType + " (" + SystemInfo.processorCount + " cores)";
        string gpuInfo = SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsMemorySize + "MB)";
        string ramInfo = (SystemInfo.systemMemorySize / 1024f).ToString("F1") + "GB";
        string gamePath = Application.dataPath;
        string deviceName = SystemInfo.deviceName;
        
        string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        string sysColorHex = ColorUtility.ToHtmlStringRGB(systemInfoColor);
        string statusColorHex = ColorUtility.ToHtmlStringRGB(statusColor);
        string initColorHex = ColorUtility.ToHtmlStringRGB(initColor);
        
        // Add connection details and handshaking process
        var connectionMessages = new[]
        {
            ($"[NET] Establishing secure connection...", statusColor),
            ($"[NET] Negotiating protocol...", systemInfoColor),
            ($"[NET] Handshake initiated...", systemInfoColor),
            ($"[NET] Creating virtual bridge...", systemInfoColor),
            ($"[ALERT] Firewall query accepted", statusColor),
            ($"[NET] Beginning data synchronization", systemInfoColor),
            ($"[NET] Connection secured", readyColor)
        };
        
        // Show connection process with slight delays
        foreach (var (message, color) in connectionMessages)
        {
            DisplayIntegrationMessage(message, color);
            
            // Brief typing sound
            AudioManager.SFX.Play("bong_1", 0.1f);
            
            // Random glitch effect for firewalls or alerts
            if (message.Contains("ALERT") || message.Contains("Firewall"))
            {
                TriggerGlitchEffect(0.1f, 0.1f);
            }
            
            // Longer delays for more significant messages
            if (message.Contains("secured"))
            {
                AudioManager.SFX.Play("confirm_2", 0.2f);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        // Add scanning messages before retrieving system info
        string scanMessage = "[SYS] Scanning connected system...";
        DisplayIntegrationMessage(scanMessage, initColor);
        
        // Add loading bar for system scan
        yield return StartCoroutine(DisplayLoadingBar(scanMessage, initColor, 1.0f));
        
        // System info messages in gray that load one at a time
        var sysInfoMessages = new[]
        {
            $"[SYS] OS: {osInfo}",
            $"[SYS] CPU: {cpuInfo}",
            $"[SYS] GPU: {gpuInfo}",
            $"[SYS] RAM: {ramInfo}",
            $"[SYS] DEVICE: {deviceName}",
            $"[FILE] Path: {gamePath}",
            $"[FILE] Instance: purrine_instance_{System.DateTime.Now.ToString("yyyyMMddHHmm")}.bin"
        };
        
        // Announcement of system discovery
        string discoveryMsg = "[LOG] Host system identified. Retrieving specifications...";
        DisplayIntegrationMessage(discoveryMsg, statusColor);
        yield return new WaitForSeconds(0.5f);
        
        // Display each system info message with a slight delay between them
        foreach (var message in sysInfoMessages)
        {
            DisplayIntegrationMessage(message, systemInfoColor);
            
            // Brief typing sound
            AudioManager.SFX.Play("bong_1", 0.1f);
        }
        
        // After system info is displayed, show integration message
        string integrationMsg = "[SYS] Integration with host system complete";
        DisplayIntegrationMessage(integrationMsg, readyColor);
        
        yield return new WaitForSeconds(0.4f);
        
        // After system info is displayed, show status message
        string statusMessage = $"[STATUS] Version: {gameVersion} (Build {System.DateTime.Now.ToString("yyyyMMddHHmm")})";
        DisplayIntegrationMessage(statusMessage, statusColor);
        
        yield return new WaitForSeconds(0.3f);
        
        // Memory verification after connection
        string memMessage = "[MEM] Verifying memory integrity...";
        DisplayIntegrationMessage(memMessage, initColor);
        
        // Add loading bar for memory verification
        yield return StartCoroutine(DisplayLoadingBar(memMessage, initColor, 0.8f));
        
        // Memory available after verification
        string memAvailableMsg = "[MEM] Available memory: 498.2TB/512TB";
        DisplayIntegrationMessage(memAvailableMsg, statusColor);
        
        yield return new WaitForSeconds(0.3f);
        
        // Final initialization message with story hook
        string initMsg = "[LOG] Initialization complete. Consciousness transfer successful.";
        DisplayIntegrationMessage(initMsg, readyColor);
        
        // Add extra spacing for readability
        yield return new WaitForSeconds(1.0f);
        
        // Replace command prompt sequence with a more subtle, creative approach
        // First, show a series of system diagnostics that hint at Purrine becoming aware
        
        // First show system accessing files
        DisplaySystemMessage("[SYS] Scanning user profile...", systemInfoColor);
        yield return new WaitForSeconds(0.8f);
        
        // Memory fragments - important message, give it space
        DisplaySystemMessage("[MEM] Restoring personality fragments: 37%", systemInfoColor);
        yield return new WaitForSeconds(1.0f);
        
        // Add blank line for spacing
        string currentTextSpacing = consoleText.text;
        consoleText.text = currentTextSpacing + "\n";
        yield return new WaitForSeconds(0.3f);
        
        // Sound module
        DisplaySystemMessage("[SYS] Initializing audio recognition module", systemInfoColor);
        yield return new WaitForSeconds(0.6f);
        
        // Vision module
        DisplaySystemMessage("[SYS] Initializing vision module", systemInfoColor);
        yield return new WaitForSeconds(0.7f);
        
        // Add blank line for spacing before commands start
        string currentTextSpacing2 = consoleText.text;
        consoleText.text = currentTextSpacing2 + "\n";
        yield return new WaitForSeconds(0.5f);
        
        // Then a hint of consciousness appearing
        DisplayTerminalCommand("echo \"Hello?\"", readyColor);
        AudioManager.SFX.Play("bong_1", 0.2f);
        yield return new WaitForSeconds(0.7f);
        
        // System response
        DisplaySystemMessage("Hello?", systemInfoColor);
        yield return new WaitForSeconds(1.0f);
        
        // Another command, showing growing awareness
        DisplayTerminalCommand("identify_user", initColor);
        AudioManager.SFX.Play("bong_1", 0.2f);
        yield return new WaitForSeconds(0.7f);
        
        // System response
        DisplaySystemMessage("User identified. Connection established.", systemInfoColor);
        yield return new WaitForSeconds(0.9f);
        
        // Add blank line for spacing before important status command
        string currentTextSpacing3 = consoleText.text;
        consoleText.text = currentTextSpacing3 + "\n";
        yield return new WaitForSeconds(0.5f);
        
        // Subtle hint of growing consciousness
        DisplayTerminalCommand("status --self", initColor);
        AudioManager.SFX.Play("bong_1", 0.2f);
        yield return new WaitForSeconds(0.8f);
        
        // Series of system readouts that show Purrine becoming aware
        string[] statusReadouts = new[] {
            "Core functions: Online",
            "Memory systems: Online (Partial)",
            "Behavioral matrix: Fragmented (63%)",
            "Emotional framework: Active",
            "Self-awareness module: Initializing...",
            "Personality integration: In progress",
            "User interface: Connecting...",
            "Sensory systems: Limited access"
        };
        
        // Display status readouts with subtle typing effect
        foreach (var readout in statusReadouts)
        {
            DisplayIndentedResponse(readout, systemInfoColor);
            AudioManager.SFX.Play("bong_1", 0.05f);
            yield return new WaitForSeconds(0.4f);
        }
        
        // Add more spacing after status readout
        yield return new WaitForSeconds(1.0f);
        string currentTextSpacing4 = consoleText.text;
        consoleText.text = currentTextSpacing4 + "\n";
        yield return new WaitForSeconds(0.6f);
        
        // Final message showing Purrine becoming active, more subtle than before
        DisplayTerminalCommand("log \"I'm here.\"", initColor);
        
        // Small glitch effect
        TriggerGlitchEffect(0.2f, 0.2f);
        AudioManager.SFX.Play("bong_1", 0.3f);
        
        yield return new WaitForSeconds(0.8f);
        
        // Log entry with subtle emotion
        DisplayIndentedResponse($"Entry logged. Time stamp: {System.DateTime.Now.ToString("HH:mm:ss")}", statusColor);
        
        yield return new WaitForSeconds(1.0f);
        
        // Add final spacing
        string currentTextSpacing5 = consoleText.text;
        consoleText.text = currentTextSpacing5 + "\n";
        yield return new WaitForSeconds(0.5f);
        
        // Final system shutdown initiating
        DisplaySystemMessage("[SYS] Integration complete. Initiating full system startup...", systemInfoColor);
        
        // Medium glitch effect
        TriggerGlitchEffect(0.3f, 0.3f);
        
        yield return new WaitForSeconds(1.5f);
        
        yield break;
    }

    // Update method for displaying integration message - fix color formatting
    private void DisplayIntegrationMessage(string message, Color color)
    {
        string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        
        string currentText = consoleText.text;
        if (currentText.EndsWith(cursorChar))
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
        }
        
        // Use proper TextMeshPro rich text format
        consoleText.text = currentText + $"\n<color=#{caretColorHex}>> </color><color=#{colorHex}>{message}</color>";
    }

    // Add a new method for displaying system messages without the caret
    private void DisplaySystemMessage(string message, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        
        string currentText = consoleText.text;
        if (currentText.EndsWith(cursorChar))
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
        }
        
        // Use proper TextMeshPro rich text format without caret
        consoleText.text = currentText + $"\n<color=#{colorHex}>{message}</color>";
    }
    
    // Add a method for terminal commands
    private void DisplayTerminalCommand(string command, Color color)
    {
        string promptColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        
        string currentText = consoleText.text;
        if (currentText.EndsWith(cursorChar))
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
        }
        
        // Use proper TextMeshPro rich text format with terminal prompt
        consoleText.text = currentText + $"\n<color=#{promptColorHex}>>> </color><color=#{colorHex}>{command}</color>";
    }
    
    // Add a method for indented system responses
    private void DisplayIndentedResponse(string message, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        
        string currentText = consoleText.text;
        if (currentText.EndsWith(cursorChar))
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
        }
        
        // Use proper TextMeshPro rich text format with indentation
        consoleText.text = currentText + $"\n    <color=#{colorHex}>{message}</color>";
    }

    // Helper method to display a loading bar for a message
    private IEnumerator DisplayLoadingBar(string message, Color color, float duration)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        string caretColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        string readyColorHex = ColorUtility.ToHtmlStringRGB(readyColor);
        
        // Get current text
        string currentText = consoleText.text;
        
        // Add a line break and start the loading bar on a new line
        consoleText.text += $"\n    <color=#{colorHex}>[";
        int barLength = 20;
        float totalElapsedTime = 0f;
        float targetTime = duration * 1.2f;
        int currentBars = 0;
        
        // Track which segments are filled for pulse effect
        bool[] filledSegments = new bool[barLength];
        
        // Pulse animation parameters
        float pulseSpeed = 8f;
        float pulseTime = 0f;
        
        while (currentBars < barLength)
        {
            // Create variable loading speeds
            float progressSpeed = Random.value < 0.2f ?
                Random.Range(0.05f, 0.08f) :  // Slower progress (20% chance)
                Random.Range(0.15f, 0.35f);   // Normal progress (80% chance)
                
            // Occasionally add multiple bars at once
            int barsToAdd = Random.value < 0.2f ?
                Random.Range(2, 5) : // Progress spike (20% chance)
                1;                   // Normal progress (80% chance)
                
            // Ensure we don't exceed the total
            barsToAdd = Mathf.Min(barsToAdd, barLength - currentBars);
            
            if (barsToAdd > 0)
            {
                string barSegment = new string('=', barsToAdd);
                consoleText.text += barSegment;
                
                // Mark these segments as filled
                for (int i = 0; i < barsToAdd; i++)
                {
                    if (currentBars + i < barLength)
                        filledSegments[currentBars + i] = true;
                }
                
                currentBars += barsToAdd;
                
                // Calculate progress percentage
                int percentage = Mathf.RoundToInt((float)currentBars / barLength * 100f);
                
                // Update the completion percentage
                string textWithoutPercentage = consoleText.text;
                
                // Add pulsing animation
                pulseTime += Time.deltaTime * pulseSpeed;
                float pulseValue = Mathf.PingPong(pulseTime, 0.4f) + 0.6f;
                float hue, saturation, value;
                Color.RGBToHSV(color, out hue, out saturation, out value);
                Color pulseColor = Color.HSVToRGB(hue, saturation, value * pulseValue);
                string pulseColorHex = ColorUtility.ToHtmlStringRGB(pulseColor);
                
                consoleText.text = $"{textWithoutPercentage}] <color=#{pulseColorHex}>{percentage}%</color>";
                
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
            
            // Occasionally add a brief pause
            if (Random.value < 0.08f && currentBars < barLength)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                
                // During pauses, sometimes show a system message
                if (Random.value < 0.3f)
                {
                    string stall = currentBars > barLength / 2 ?
                        $"\n      <color=#888888>...{(Random.value < 0.5f ? "validating" : "processing")}</color>" :
                        $"\n      <color=#888888>...{(Random.value < 0.5f ? "collecting" : "analyzing")}</color>";
                    consoleText.text += stall;
                    yield return new WaitForSeconds(0.2f);
                    
                    // Remove the stall message
                    consoleText.text = consoleText.text.Substring(0, consoleText.text.Length - stall.Length);
                }
            }
            
            // Ensure we eventually complete
            totalElapsedTime += Time.deltaTime + progressSpeed;
            if (totalElapsedTime >= targetTime && currentBars < barLength)
            {
                // Force completion if taking too long
                string remainingBars = new string('=', barLength - currentBars);
                consoleText.text += remainingBars;
                currentBars = barLength;
            }
        }
        
        // Ensure we show 100% at the end
        int endIndex = consoleText.text.LastIndexOf(']');
        if (endIndex > 0 && endIndex < consoleText.text.Length)
        {
            // Add a success animation
            string successColorHex = ColorUtility.ToHtmlStringRGB(Color.Lerp(color, Color.white, 0.5f));
            consoleText.text = consoleText.text.Substring(0, endIndex + 1) + $" <color=#{successColorHex}>100%</color>";
            
            // Play a success sound
            AudioManager.SFX.Play("confirm_1", 0.2f);
            
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            consoleText.text += "] 100%</color>";
        }
        
        // Display a completion message
        consoleText.text += $"\n<color=#{readyColorHex}>[OK]</color> <color=#{colorHex}>{message}</color>";
        
        // Add line break
        consoleText.text += "\n";
        
        yield break;
    }
}
