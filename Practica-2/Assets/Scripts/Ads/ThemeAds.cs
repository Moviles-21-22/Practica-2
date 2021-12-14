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
        [SerializeField]
        public Button button;
        private ColorPack theme;

        public ColorPack GetTheme() { return theme; }
        public void SetTheme(ColorPack p) { theme = p; }
        public void ThemeCallBack()
        {
            if (theme.active)
            {
                GameManager.instance.SetTheme(theme);
            }
            else
            {
                GameManager.instance.UnlockTheme(theme);
            }
        }
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
            var cp = themes[i];
            cp.SetTheme(t[i]);
            themes[i].button.onClick.AddListener(() => cp.ThemeCallBack());
            themes[i] = cp;

            if (!t[i].active)
            {
                themes[i].status.sprite = lockImage;
                themes[i].status.rectTransform.sizeDelta.Set(50.0f,50.0f);
                themes[i].status.enabled = true;
            }
            themes[i].themeName.text = t[i].colorPackName;

            for (int j = 0; j < themes[i].Samples.Count; j++)
            {
                themes[i].Samples[j].color = t[i].colors[j];
            }
        }
    }
}
