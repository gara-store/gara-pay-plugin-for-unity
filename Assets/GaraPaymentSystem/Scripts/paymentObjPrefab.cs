using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class paymentObjPrefab : MonoBehaviour
{
    PaymentUiElement paymentUiElement;
    public Image icon;
    public Text PaymentMethodName;

    [System.Serializable]
    public class PaymentList
    {
        public string serviceCode = "";
        public string telecom = "";
        public string type = "";
        public bool isCashOut = true;
        public string iconUrl = "";
        public bool isOtpRequired = true;
    }
    public PaymentList paymentInfo;
    private void Awake()
    {
        paymentUiElement = FindObjectOfType<PaymentUiElement>();
    }
    void Start()
    {
        StartCoroutine(DisplayTelecomImage());

        //  GetComponent<Button>().onClick.AddListener(Onclick_ConfirmAchatProcess);

        GetComponent<Button>().onClick.AddListener(showNumberPanel);

        //Rename Object
        gameObject.name = paymentInfo.telecom;
        PaymentMethodName.text = paymentInfo.telecom;
    }

    void showNumberPanel()
    {
        paymentUiElement.ActualPaymentObjectSelected = gameObject;


        //paymentUiElement.Panel_ListPayment.SetActive(false);
        //paymentUiElement.Panel_Number.SetActive(true);

        //Update Sprite
        paymentObjPrefab[] pop = FindObjectsOfType<paymentObjPrefab>();
        for (int i = 0; i < pop.Length; i++)
        {
            if (pop[i].gameObject.name == paymentUiElement.ActualPaymentObjectSelected.name)
            {
                pop[i].gameObject.GetComponent<Image>().sprite = paymentUiElement.SelectedMethode;
            }
            else
            {
                pop[i].gameObject.GetComponent<Image>().sprite = paymentUiElement.DeselectedMethode;
            }
        }
    }

    public void ConfirmMethod()
    {
        paymentUiElement.Show_Panel_Number();

        if (paymentInfo.isOtpRequired)
        {
            paymentUiElement.ConfirmOTP.onClick.AddListener(Onclick_ConfirmAchatProcess_With_OTP);
            paymentUiElement.Panel_OTP.SetActive(true);
        }
    }


    IEnumerator DisplayTelecomImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(paymentInfo.iconUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);
        else
        {
            Texture2D img = ((DownloadHandlerTexture)request.downloadHandler).texture;
            icon.sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(.5f, .5f));
        }
        Debug.Log("image display succesfull");
    }


    public void Onclick_ConfirmAchatProcess()
    {
        print("Onclick_ConfirmAchatProcess");

        if (paymentInfo.isOtpRequired)
        {
            Debug.LogError("is Otp Required");
            //NEED OTP CODE TO COMPLETE PURCHASE

            //enable OTP panel
            paymentUiElement.Panel_OTP.SetActive(true);
            paymentUiElement.Panel_ListPayment.SetActive(false);

           // GameObject.FindObjectOfType<PaymentSystem>().payData.serviceCode = paymentInfo.serviceCode;
            //GameObject.FindObjectOfType<PaymentSystem>().payData.paymentMethodType = paymentInfo.type;

            GameObject.FindObjectOfType<NewPaymentSystem>().payData.serviceCode = paymentInfo.serviceCode;
            GameObject.FindObjectOfType<NewPaymentSystem>().payData.paymentMethodType = paymentInfo.type;


        }
        else
        {

            print("LaunchPayment");
            // NO OTP CODE REQUIRED FOR PURCHASE
         //   GameObject.FindObjectOfType<PaymentSystem>().payData.serviceCode = paymentInfo.serviceCode;
         //   GameObject.FindObjectOfType<PaymentSystem>().payData.paymentMethodType = paymentInfo.type;

            GameObject.FindObjectOfType<NewPaymentSystem>().payData.serviceCode = paymentInfo.serviceCode;
            GameObject.FindObjectOfType<NewPaymentSystem>().payData.paymentMethodType = paymentInfo.type;

           // paymentUiElement.ConfirmRESUME_Loader.SetActive(false);

           GameObject.FindObjectOfType<NewPaymentSystem>().PurchaseItem();
        }
    }

    void Onclick_ConfirmAchatProcess_With_OTP()
    {
        paymentUiElement.Panel_OTP.SetActive(false);

        //Add the OTP
        GameObject.FindObjectOfType<NewPaymentSystem>().payData.otp = paymentUiElement.OTP_Inputfield.text;

        //launch payment
        GameObject.FindObjectOfType<NewPaymentSystem>().PurchaseItem();
    }
}
