using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    void Start()
    {
        StartCoroutine(InitDropdown());
    }

    IEnumerator InitDropdown()
    {
        // Đợi Localization init xong
        yield return LocalizationSettings.InitializationOperation;

        // Set dropdown theo locale hiện tại
        int index = LocalizationSettings.AvailableLocales.Locales
            .IndexOf(LocalizationSettings.SelectedLocale);

        dropdown.SetValueWithoutNotify(index);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    void OnLanguageChanged(int index)
    {
        StartCoroutine(ChangeLanguage(index));
    }

    IEnumerator ChangeLanguage(int index)
    {
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt("LANGUAGE", index);
        Char.Instance.txtLevel.SetText(Noti.Get("level_format", Char.Instance.level));
    }
}
