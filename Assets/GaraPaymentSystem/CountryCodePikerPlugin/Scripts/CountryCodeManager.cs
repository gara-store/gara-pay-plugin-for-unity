using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CountryCodeManager : MonoBehaviour
{
    PaymentUiElement paymentUiElement;

    public Sprite[] flags;
    public TextAsset jsonFile;
    List<string> countryCodesList;
    public Dropdown CountryCode;
    Data code;
    CountryName name;
    string countryCode;

    private void Awake()
    {
        paymentUiElement = FindObjectOfType<PaymentUiElement>();
    }


    void Start()
    {
        jsonFile = Resources.Load("Codes/CountryCode") as TextAsset;
        if (jsonFile != null)
            FileReader();
        countryCodesList = new List<string>();
        countryCodesList.Clear();


       print(PlayerPrefs.GetString("countryCode"));
        if(PlayerPrefs.HasKey("countryCode"))
        {
            for (int i = 0; i < flags.Length; i++)
            {
                Sprite item = flags[i];

                if (item.name == PlayerPrefs.GetString("countryCode").ToLower())
                {
                   // print("Found: " + item.name + " at index " + i);
                 
                    countryStateSelected = i;
                    CountryCode.value = i;
                   // print(code.data[countryStateSelected].phone_code);

                    if(paymentUiElement)
                    {
                        paymentUiElement.PhoneCode = code.data[countryStateSelected].phone_code;
                        paymentUiElement.PhoneCode_text.text = "" + code.data[countryStateSelected].phone_code;
                    }
                }
            }
        }
       
    }


    public int countryStateSelected;
   public void dropDown_OnValueChanged()
    {
        countryStateSelected = CountryCode.value;
        //print(countryStateSelected);
        //print(code.data[countryStateSelected].phone_code);
        //print(code.data[countryStateSelected].country_code);
        //print(code.data[countryStateSelected].country_en);

        paymentUiElement.PhoneCode = code.data[countryStateSelected].phone_code;
        paymentUiElement.PhoneCode_text.text = "" + code.data[countryStateSelected].phone_code;

        PlayerPrefs.SetString("countryInfo", code.data[countryStateSelected].country_code + " : " + code.data[countryStateSelected].country_en);
        PlayerPrefs.SetString("countryCode", code.data[countryStateSelected].country_code);
    }

    //public void ConfirmerChangementCountry()
    //{
    //    print(countryStateSelected);

    //    PlayerPrefs.SetString("countryInfo", code.data[countryStateSelected].country_code + " : " + code.data[countryStateSelected].country_en);
    //    PlayerPrefs.SetString("countryCode", code.data[countryStateSelected].country_code);

    //    print(PlayerPrefs.GetString("countryCode"));

    //    GameObject.FindObjectOfType<PaymentService>().ShowCountryPaymentList();
    //}



    void FileReader()
    {
        code = JsonUtility.FromJson<Data>(jsonFile.text);
        name = JsonUtility.FromJson<CountryName>(jsonFile.text);
        flags = new Sprite[code.data.Length];
        poulateCode();
    }

    void poulateCode()
    {
        CountryCode.ClearOptions();
        // Create the option list
        List<Dropdown.OptionData> flagItems = new List<Dropdown.OptionData>();
        for (int i = 0; i < code.data.Length; i++)
        {
            Resources.Load<Sprite>("Sprites/my_sprite");
            flags[i] = Resources.Load<Sprite>("Flags/" + code.data[i].country_code.ToLower().ToString());
            string flagName = code.data[i].country_en + "   (+" + code.data[i].phone_code + ")";
            var flagOption = new Dropdown.OptionData(flagName, flags[i]);
            flagItems.Add(flagOption);
        }
        CountryCode.AddOptions(flagItems);

    }
}
[System.Serializable]
public class Data
{
    public CodeData[] data;
}

[System.Serializable]
public class CodeData
{
    public string country_code;
    public string country_en;
    public string phone_code;
    public string country_cn;
}
[System.Serializable]
public class CountryName
{
    public List<string> Name;
}