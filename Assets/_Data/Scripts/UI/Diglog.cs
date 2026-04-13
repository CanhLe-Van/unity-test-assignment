using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Diglog : MonoBehaviour
{
    public Text contentText;
    public Text titleText;
    public void Show(bool ishow)
    {
        this.gameObject.SetActive(ishow);
    }

    public void UpdateDialog(string title,string text)
    {
        if (titleText)
            titleText.text = title.ToString();
        if (contentText)
            contentText.text = text.ToString();
    }
}
