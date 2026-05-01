using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class MainMenuManagement : MonoBehaviour
{
    public UIDocument uiMainDocument;
    public UIDocument uiSettingsDocument;
    public UIDocument uiCreditsDocument;

    public AudioMixer audioMixer;

    private VisualElement settingsPanel;
    private VisualElement creditsPanel;

    private Slider musicSlider;
    private Slider sfxSlider;

    private DropdownField languageDropdown;

    private bool wasBPressed;

    void OnEnable()
    {
        // Get roots
        var root = uiMainDocument.rootVisualElement;
        var setting = uiSettingsDocument.rootVisualElement;
        var credit = uiCreditsDocument.rootVisualElement;

        // Debug
        Debug.Log("main root: " + root);
        Debug.Log("settings root: " + setting);
        Debug.Log("credit root: " + credit);

        // Safety checks (prevents crashes)
        if (root == null || setting == null || credit == null)
        {
            Debug.LogError("One or more UI roots are NULL. Make sure all UIDocuments are ACTIVE and have UXML assigned.");
            return;
        }

        // Buttons (main menu)
        var startButton = root.Q<Button>("start-button");
        var settingsButton = root.Q<Button>("settings-button");
        var creditsButton = root.Q<Button>("credits-button");
        var quitButton = root.Q<Button>("quit-button");

        // Panels + close buttons
        creditsPanel = credit.Q<VisualElement>("credits-panel");
        var closeCreditsButton = credit.Q<Button>("close-Cbutton");

        settingsPanel = setting.Q<VisualElement>("settings-panel");
        var closeSettingsButton = setting.Q<Button>("close-Sbutton");

        musicSlider = setting.Q<Slider>("music-slider");
        sfxSlider = setting.Q<Slider>("sfx-slider");

        languageDropdown = setting.Q<DropdownField>("language-setting");

        // Hook events
        startButton.clicked += OnStartNavigation;
        quitButton.clicked += OnQuit;

        settingsButton.clicked += ShowSettings;
        closeSettingsButton.clicked += HideSettings;

        creditsButton.clicked += ShowCredits;
        closeCreditsButton.clicked += HideCredits;

        // Hide panels initially
        creditsPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;

        SetupAudioSliders();

        SetupLanguage();
    }

    void SetupLanguage()
    {
        if (languageDropdown == null)
        {
            Debug.LogWarning("Language dropdown not found.");
            return;
        }

        languageDropdown.choices = new System.Collections.Generic.List<string>
        {
            "English",
            "Türkçe"
        };

        languageDropdown.value = "English";

        languageDropdown.RegisterValueChangedCallback(evt =>
        {
            SetLanguage(evt.newValue);
        });
    }

    void SetLanguage(string lang)
    {
        StartCoroutine(SetLocaleRoutine(lang));
    }

    System.Collections.IEnumerator SetLocaleRoutine(string lang)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in locales)
        {
            if (lang == "English" && locale.Identifier.Code == "en")
            {
                LocalizationSettings.SelectedLocale = locale;
                yield break;
            }

            if (lang == "Türkçe" && locale.Identifier.Code == "tr")
            {
                LocalizationSettings.SelectedLocale = locale;
                yield break;
            }
        }

        Debug.LogWarning("Locale not found for: " + lang);
    }

    void SetupAudioSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = 0.8f;

            musicSlider.RegisterValueChangedCallback(evt =>
            {
                audioMixer.SetFloat("musicVolume", ToDb(evt.newValue));
            });

            audioMixer.SetFloat("musicVolume", ToDb(musicSlider.value));
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = 0.8f;

            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                audioMixer.SetFloat("sfxVolume", ToDb(evt.newValue));
            });

            audioMixer.SetFloat("sfxVolume", ToDb(sfxSlider.value));
        }
    }

    float ToDb(float value)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }

    void OnStartNavigation()
    {
        SceneManager.LoadScene(1);
    }

    void ShowCredits()
    {
        creditsPanel.style.display = DisplayStyle.Flex;
    }

    void HideCredits()
    {
        creditsPanel.style.display = DisplayStyle.None;
    }

    void ShowSettings()
    {
        settingsPanel.style.display = DisplayStyle.Flex;
    }

    void HideSettings()
    {
        settingsPanel.style.display = DisplayStyle.None;
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnBackPressed()
    {
        // If settings open -> close it
        if (settingsPanel != null && settingsPanel.style.display == DisplayStyle.Flex)
        {
            HideSettings();
            return;
        }

        // If credits open -> close it
        if (creditsPanel != null &&
            creditsPanel.style.display == DisplayStyle.Flex)
        {
            HideCredits();
            return;
        }

        // Already in main menu -> optional: do nothing or quit prompt
    }

    void Update()
    {
        // Quest 3 / Oculus right controller "B" button = buttonEast
        var gamepad = Gamepad.current;

        if (gamepad == null)
            return;

        bool bPressed = gamepad.buttonEast.isPressed;

        // detect rising edge (press once)
        if (bPressed && !wasBPressed)
        {
            OnBackPressed();
        }

        wasBPressed = bPressed;
    }

}