using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LocalizationBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        int index;
        if (PlayerPrefs.HasKey("LANG"))
        {
            index = PlayerPrefs.GetInt("LANGUAGE");
        }
        else index = 1;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}