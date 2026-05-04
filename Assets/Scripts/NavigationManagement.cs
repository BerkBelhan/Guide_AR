using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class NavigationManagement : MonoBehaviour
{
    public UIDocument uiSettingsDocument;
    public AudioMixer audioMixer;

    private VisualElement settingsPanel;

    private Slider musicSlider;
    private Slider sfxSlider;
    private DropdownField languageDropdown;

    private bool settingsOpen = false;

    void OnEnable()
    {
        var setting = uiSettingsDocument.rootVisualElement;

        Debug.Log("settings root: " + setting);

        if (setting == null)
        {
            Debug.LogError("Settings UI is NULL. Check UIDocument + UXML assignment.");
            return;
        }

        settingsPanel = setting.Q<VisualElement>("settings-panel");
        var closeSettingsButton = setting.Q<Button>("close-Sbutton");
        var quitButton = setting.Q<Button>("quitmenu-button");

        musicSlider = setting.Q<Slider>("music-slider");
        sfxSlider = setting.Q<Slider>("sfx-slider");

        languageDropdown = setting.Q<DropdownField>("language-setting");

        closeSettingsButton.clicked += HideSettings;
        quitButton.clicked += OnQuitMenu;

        settingsPanel.style.display = DisplayStyle.None;

        SetupAudioSliders();
        SetupLanguage();
    }

    // ---------------- INPUT (QUEST 3) ----------------

    void Update()
    {
        // A BUTTON -> OPEN SETTINGS
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ShowSettings();
        }

        // B BUTTON -> CLOSE SETTINGS / BACK
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            OnBackPressed();
        }
    }

    void OnBackPressed()
    {
        if (settingsOpen)
        {
            HideSettings();
        }
    }

    // ---------------- SETTINGS UI ----------------

    void ShowSettings()
    {
        if (settingsPanel == null) return;

        settingsPanel.style.display = DisplayStyle.Flex;
        settingsOpen = true;
    }

    void HideSettings()
    {
        if (settingsPanel == null) return;

        settingsPanel.style.display = DisplayStyle.None;
        settingsOpen = false;
    }

    // ---------------- AUDIO ----------------

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

    void OnQuitMenu()
    {
        SceneManager.LoadScene(0);
    }

    // ---------------- LANGUAGE ----------------

    void SetupLanguage()
    {
        if (languageDropdown == null) return;

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

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (lang == "English" && locale.Identifier.Code == "en")
                LocalizationSettings.SelectedLocale = locale;

            if (lang == "Türkçe" && locale.Identifier.Code.StartsWith("tr"))
                LocalizationSettings.SelectedLocale = locale;
        }
    }
}