using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Translator : MonoBehaviour
{
    [HideInInspector]
    [Multiline]
    public string inFrench;

    [Multiline]
    public string inEnglish;

    TextMeshProUGUI me;
    Text myText;
    string stckFR;

    private void Awake()
    {
        if (GetComponent<TextMeshProUGUI>()!=null)
        {
            me = GetComponent<TextMeshProUGUI>();
            stckFR = me.text.ToString();
        }
        else
        {
            myText = GetComponent<Text>();
            stckFR = myText.text.ToString();
        }
        inFrench = stckFR;
    }
    void Start()
    {
      
    }

    private void OnEnable()
    {
        CancelInvoke();

        translateTexte();
    }
    public void translateTexte()
    {
     
        if (PlayerPrefs.GetInt("lang") == 0)
        {
            //English
            if (GetComponent<TextMeshProUGUI>() != null)
            {
                me.text = inEnglish;
            }
            else
            {
                myText.text = inEnglish;
            }
        }
        else
        {
            //Francais
            if (GetComponent<TextMeshProUGUI>() != null)
            {
                me.text = inFrench;
            }
            else
            {
                myText.text = inFrench;
            }
        }
    }
}
