using EasyUI.Toast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaymentUiElement : MonoBehaviour
{
    public string TOKEN;

    public string PhoneCode;
    public Text PhoneCode_text;

   // public GameObject PaymentPrefab;

    [Header("PANEL")]
    public GameObject Panel_CountrySelection;
    public GameObject Panel_Number;
    public GameObject Panel_OTP;
    public GameObject Panel_ListPayment;
    public GameObject Panel_ProductItem;
    public GameObject Panel_RESUME;
    public GameObject ConfirmRESUME_Loader;

    [Header("BUTTONS")]
    public Button ConfirmCountry;
    public Button ConfirmNumber;
    public Button ConfirmOTP;
    public Button CancelPaymentButton;
    public Button ConfirmMethod;
    public Button ConfirmRESUME;
  

    [Header("INPUTFIELD")]
    public InputField Number_InputField;
    public InputField OTP_Inputfield;

    [Header("PAYMENT OBJECT")]
    public Transform paymentPoz;
    public GameObject paymentObjPrefab;
    public GameObject ActualPaymentObjectSelected;
    public Sprite SelectedMethode;
    public Sprite DeselectedMethode;
    


    [Header("PANEL ALERT")]
    public GameObject Alerte_NoPaymentMethodAvailable;
    public GameObject Alerte_WaitingForValidation;
    public GameObject Alerte_PaymentFail;
    public GameObject Alerte_PaymentSuccess;
    public GameObject LoadingEffect;

    [Header("PANEL RESUME")]
    public Text resume_Game_name;
    public Text resume_Pack_name;
    public Text resume_Amount;
    public Text resume_PhoneNumber;
    public Image resume_Country_Flag;
    public Dropdown CountryDropdown;
    public Image resume_Operator_Icon;
    public Text resume_Operator_Name;

    [Header("OTHER")]
    public Dropdown dropdownLanguage;
    public int languageIndex;
    public void ShowResume()
    {
        Panel_RESUME.SetActive(true);
        resume_Game_name.text = Application.productName;
        resume_Pack_name.text = FindObjectOfType<ProductItemGeneration>().actualItemSelected.itemPublicName;
        resume_Amount.text = FindObjectOfType<ProductItemGeneration>().actualItemSelected.itemPrice + " " + FindObjectOfType<ProductItemGeneration>().actualItemSelected.Currency;
       
        resume_Country_Flag.sprite = CountryDropdown.captionImage.sprite;

        resume_Operator_Icon.sprite = ActualPaymentObjectSelected.GetComponent<paymentObjPrefab>().icon.sprite;
        resume_Operator_Name.text = ActualPaymentObjectSelected.GetComponent<paymentObjPrefab>().PaymentMethodName.text;
    }

    void Start()
    {
        ConfirmMethod.onClick.AddListener(ConfirmMethodSelected);

        Panel_ProductItem.SetActive(true);
        Panel_CountrySelection.SetActive(true);
        ConfirmCountry.onClick.AddListener(Show_Payment_List_Number);
        CancelPaymentButton.onClick.AddListener(CancelPayment);

        //Toast.Show("Test d'un popup ! ", 3, ToastColor.Red, ToastPosition.TopCenter);

        if (string.IsNullOrEmpty(TOKEN))
        {
            Debug.LogError("Add token in PaymentUiElement script, variable : TOKEN");
            if (PlayerPrefs.GetInt("lang") == 0)
            {
                Toast.Show("The token is empty !", 10, ToastColor.Red, ToastPosition.TopCenter);
            }
            else
            {
                Toast.Show("LE token est vide ", 10, ToastColor.Red, ToastPosition.TopCenter);
            }
        }

    }


    public void ShowConfirmRESUME_Loader()
    {
        StartCoroutine(ConfirmRESUME_LoaderAlert());
    }
    IEnumerator ConfirmRESUME_LoaderAlert()
    {
        ConfirmRESUME_Loader.SetActive(true);
        yield return new WaitForSeconds(15f);
        ConfirmRESUME_Loader.SetActive(false);
    }


    public void SelectLanguage()
    {
        languageIndex = dropdownLanguage.value;
        PlayerPrefs.SetInt("lang", languageIndex);

        Translator[] translate = FindObjectsOfType<Translator>();

        for (int i = 0; i < translate.Length; i++) 
        {
            translate[i].translateTexte();
        }

    }


    void ConfirmMethodSelected()
    {
        if(ActualPaymentObjectSelected!=null)
        {
            ActualPaymentObjectSelected.GetComponent<paymentObjPrefab>().ConfirmMethod();
        }
        else
        {
            if (PlayerPrefs.GetInt("lang") == 0)
            {
                Toast.Show("Choose a payment method", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
            else
            {
                Toast.Show("Choisissez un moyen de paiement", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
        }
    }



    void Show_Payment_List_Number()
    {
        Panel_CountrySelection.SetActive(false);
        FindObjectOfType<PaymentService>().ShowCountryPaymentList();
        Panel_ListPayment.SetActive(true);
    }

    public void Show_Panel_Number()
    {
        Panel_ListPayment.SetActive(false);
        Panel_CountrySelection.SetActive(false);

        Panel_Number.SetActive(true);
    }

    public void NumberIsOk()
    {
        Panel_CountrySelection.SetActive(false);
        Panel_Number.SetActive(false);
        Panel_ListPayment.SetActive(false);
    }

    public void WaitingForPaymentValidation()
    {
        Panel_CountrySelection.SetActive(false);
        Panel_Number.SetActive(false);
        Panel_OTP.SetActive(false);
        Panel_ListPayment.SetActive(false);

        Alerte_WaitingForValidation.SetActive(true);
    }

    public void Payment_Success()
    {
        Panel_CountrySelection.SetActive(false);
        Panel_Number.SetActive(false);
        Panel_OTP.SetActive(false);
        Panel_ListPayment.SetActive(false);
        Alerte_WaitingForValidation.SetActive(false);
        Alerte_PaymentFail.SetActive(false);

        Alerte_PaymentSuccess.SetActive(true);

    }
    public void Payment_Failed()
    {
        Panel_CountrySelection.SetActive(false);
        Panel_Number.SetActive(false);
        Panel_OTP.SetActive(false);
        Panel_ListPayment.SetActive(false);
        Alerte_WaitingForValidation.SetActive(false);
        Alerte_PaymentSuccess.SetActive(false);

        Alerte_PaymentFail.SetActive(true);

    }

    public void CancelPayment()
    {
       // FindObjectOfType<PaymentSystem>().Cancel_checkpayment();
        FindObjectOfType<NewPaymentSystem>().CancelAll();

        Panel_CountrySelection.SetActive(true);

        Panel_Number.SetActive(false);
        Panel_OTP.SetActive(false);
        Panel_ListPayment.SetActive(false);
        Alerte_WaitingForValidation.SetActive(false);
        Alerte_PaymentSuccess.SetActive(false);
        Alerte_PaymentFail.SetActive(false);
        Alerte_NoPaymentMethodAvailable.SetActive(false);
        LoadingEffect.SetActive(false);
    }



   
}
