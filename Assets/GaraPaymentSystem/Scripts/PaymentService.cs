using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PaymentMethod
{
    public string serviceCode = "";
    public string telecom = "";
    public string type = "";
    public bool isCashOut = true;
    public string iconUrl = "";
    public bool isOtpRequired = true; 
}

[System.Serializable]
public class CountryData
{
    public string agencyCode;
    public string provider;
    public bool isActive;
    public Country country;
    public List<PaymentMethod> paymentMethod;
    public BankCardOperator bankCardOperator;
}

[System.Serializable]
public class Country
{
    public string countryCode;
    public string name;
    public string indicatif;
    public Currency currency;
    public bool isAfricanCountry;
}

[System.Serializable]
public class Currency
{
    public string code;
    public string name;
    public string symbole;
    public float usdunit;
}

[System.Serializable]
public class BankCardOperator
{
    public string provider;
    public string iconUrl;
}

[System.Serializable]
public class CountryPaymentListData
{
    public List<CountryData> PaymentList = new List<CountryData>();
}

[System.Serializable]
public class PaymentList
{
    public List<PaymentMethod> PaymentLists = new List<PaymentMethod>();
}
public class PaymentService : MonoBehaviour
{
    PaymentUiElement paymentUiElement;
    public CountryPaymentListData CountryPaymentList = new CountryPaymentListData();
    public PaymentList cashOutList = new PaymentList();
    public CountryData CountryData = new CountryData();


    //don't touch, or you'll ruin everything LOL
    string front, end;

	
	string token = "";
  


    private void Awake()
    {
     
        paymentUiElement = FindObjectOfType<PaymentUiElement>();

        front = "{" + "\"PaymentList" + "\"" + ":";
		end = "}";
	}


    private void Start()
	{
        token = paymentUiElement.TOKEN;
    }



	public void ShowCountryPaymentList()
	{
         StartCoroutine(getCountryMobileMoneyListWithProvider());

      //  GetComponent<PaymentSystem>().payData.countryCode = PlayerPrefs.GetString("countryCode");
        GetComponent<NewPaymentSystem>().payData.countryCode = PlayerPrefs.GetString("countryCode");
    }


     IEnumerator getCountryMobileMoneyListWithProvider()
    {
        paymentUiElement.LoadingEffect.SetActive(true);

      //  yield return new WaitForSeconds(1f);

        string uri = "https://75272b11ec.execute-api.eu-west-3.amazonaws.com/api/mpayment/agency/agencylist/country?countryCode=" + PlayerPrefs.GetString("countryCode");
       
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("Authorization", token);

            yield return webRequest.SendWebRequest();
            Debug.Log(webRequest.downloadHandler.text);
            string response = webRequest.downloadHandler.text; ;
       
            print(response);

            /// <PACKING>
            string patch_FirstCharacter = response.Insert(0, front);
            patch_FirstCharacter = patch_FirstCharacter.Insert(patch_FirstCharacter.Length, end);
            /// </PACKING>

            // Deserialize JSON
            CountryPaymentList = JsonUtility.FromJson<CountryPaymentListData>(patch_FirstCharacter);

            // Delete old child objects
            foreach (Transform child in paymentUiElement.paymentPoz)
            {
                GameObject.Destroy(child.gameObject);
            }

            // Empty cashOutList
            cashOutList.PaymentLists.Clear();


            // Add new objects
            foreach (var countryData in CountryPaymentList.PaymentList)
            {
                foreach (var item in countryData.paymentMethod)
                {
                    if (item.isCashOut)
                    {
                        cashOutList.PaymentLists.Add(item);
                        GameObject g = Instantiate(paymentUiElement.paymentObjPrefab, paymentUiElement.paymentPoz);


                        g.GetComponent<paymentObjPrefab>().paymentInfo.iconUrl = item.iconUrl;
                        g.GetComponent<paymentObjPrefab>().paymentInfo.isCashOut = item.isCashOut;
                        g.GetComponent<paymentObjPrefab>().paymentInfo.isOtpRequired = item.isOtpRequired;
                        g.GetComponent<paymentObjPrefab>().paymentInfo.serviceCode = item.serviceCode;
                        g.GetComponent<paymentObjPrefab>().paymentInfo.telecom = item.telecom;
                        g.GetComponent<paymentObjPrefab>().paymentInfo.type = item.type;
                    }
                }
            }

            if (cashOutList.PaymentLists.Count <= 0)
            {
                //No means of payment available in your country !
                paymentUiElement.Alerte_NoPaymentMethodAvailable.SetActive(true);
                paymentUiElement.Panel_ListPayment.SetActive(false);
                paymentUiElement.LoadingEffect.SetActive(false);
            }
            else
            {
                paymentUiElement.Alerte_NoPaymentMethodAvailable.SetActive(false);
                paymentUiElement.LoadingEffect.SetActive(false);
            }


            if (CountryPaymentList.PaymentList.Count > 0)
            {
                CountryData = CountryPaymentList.PaymentList[0];
              //  GetComponent<PaymentSystem>().payData.provider = CountryData.provider;
                GetComponent<NewPaymentSystem>().payData.provider = CountryData.provider;
            }
        }

        yield return new WaitForSeconds(1);
    }



}
