using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeAds : MonoBehaviour
{
    [SerializeField]
    public List<ThemeComposition> themes;

    [SerializeField]
    public List<Image> menu;

    public Sprite lockImage;
    private Image currMark;

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
        private Sprite circle;

        public void SetCircle()
        {
            circle = status.sprite;
        }

        public ColorPack GetTheme() { return theme; }
        public void SetTheme(ColorPack p) { theme = p; }
        public void ThemeCallBack()
        {
            if (theme.active)
            {
                GameManager.instance.SetTheme(theme);
                status.enabled = true;
            }
            else
            {
                GameManager.instance.UnlockTheme(theme);
                status.sprite = circle;
            }
        }
    }

    void Start()
    {
        InitThemes();
        ChangeShopColor();
    }

    public void QuitMark(int index)
    {
        themes[index].ThemeCallBack();
        currMark.enabled = false;
        currMark = themes[index].status;
        currMark.enabled = true;
        ChangeShopColor();
    }

    private void ChangeShopColor()
    {
        List<Color> colors = GameManager.instance.GetCurrTheme().colors;
        for (int i = 0; i < menu.Count; i++)
        {
            menu[i].color = colors[i];
        }
    }

    private void InitThemes()
    {
        var t = GameManager.instance.GetThemes();
        var cT = GameManager.instance.GetCurrTheme();
        for (int i = 0; i < t.Count; i++)
        {
            var cp = themes[i];
            cp.SetCircle();
            cp.SetTheme(t[i]);
            if(t[i] == cT)
            {
                cp.status.enabled = true;
                currMark = cp.status;
            }
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
