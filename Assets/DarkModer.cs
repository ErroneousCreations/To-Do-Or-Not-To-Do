using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DarkModer : MonoBehaviour
{
    public Image[] myImages;
    public TMPro.TMP_Text[] myTexts;
    public Color lightCol, darkCol;
    bool darkmode;

    private void Update()
    {
        if(PlayerPrefs.GetInt("DarkMode", 0) == 1 && !darkmode)
        {
            darkmode = true;
            if(myImages.Length > 0)
            {
                foreach (var item in myImages)
                {
                    item.color = darkCol;
                }
            }

            if (myTexts.Length > 0)
            {
                foreach (var item in myTexts)
                {
                    item.color = darkCol;
                }
            }
        }

        if (PlayerPrefs.GetInt("DarkMode", 0) == 0 && darkmode)
        {
            darkmode = false;
            if (myImages.Length > 0)
            {
                foreach (var item in myImages)
                {
                    item.color = lightCol;
                }
            }

            if (myTexts.Length > 0)
            {
                foreach (var item in myTexts)
                {
                    item.color = lightCol;
                }
            }
        }
    }
}
