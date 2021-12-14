using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeAds : MonoBehaviour
{
    [SerializeField]
    public List<ThemeComposition> themes;

    public Sprite lockImage;

    [System.Serializable]
    public struct ThemeComposition
    {
        [SerializeField]
        public Image status;
        [SerializeField]
        public Text themeName;
        [SerializeField]
        public List<Image> Samples;
    }

    void Start()
    {
        InitThemes();
    }

    private void InitThemes()
    {
        var t = GameManager.instance.GetThemes();
        for (int i = 0; i < t.Count; i++)
        {
            if (!t[i].active)
                themes[i].status.sprite = lockImage;
            themes[i].themeName.text = t[i].colorPackName;
            for (int j = 0; i < themes[i].Samples.Count; j++)
            {
                themes[i].Samples[j].color = t[i].colors[j];
            }
        }
    }
}
