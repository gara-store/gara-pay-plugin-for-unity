using EasyUI.Toast;
using System.Collections;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PaymentData
{
    public int id = 1311;
    public string provider = "INTOUCH";
    public string platform = "MINI_CUP";
    public string returnUrl = "";
    public string action = "IN_APP_PURCHASE";
    public bool isCashIn = false;
    public string paymentMethodType = "MOBILE_MONEY";
    public int amount = 0;
    public string currencySymbol = "FCFA";
    public string phoneNumber = "";
    public string serviceCode = "";
    public string countryCode = "";
    public string otp = "";
    public string appUserId = "";
}

[System.Serializable]
public class TransactionEvents
{
    public string transactionId = "";
    public string paymentUrl = "";
    public string redirectUrl = "";
    public string status = "";
}
public class NewPaymentSystem : MonoBehaviour
{
    PaymentUiElement paymentUiElement;
    ProductItemGeneration PIG;
    public PaymentData payData;
    public TransactionEvents transactionEvent;
    private string token = ""; 
    public int attempts;


    public delegate void PaymentCallback();

    void Awake()
    {
      

        paymentUiElement = FindObjectOfType<PaymentUiElement>();
        PIG = FindObjectOfType<ProductItemGeneration>();
        Application.runInBackground = true;
        SetupUserInfo();
    }
    private void Start()
    {
        paymentUiElement.ConfirmNumber.onClick.AddListener(setupUserPhoneNumer);
        paymentUiElement.ConfirmRESUME.onClick.AddListener(ConfirmPaymentAfterResume);
        token = paymentUiElement.TOKEN;
    }

    void SetupUserInfo()
    {
        payData.countryCode = PlayerPrefs.GetString("countryCode");
        payData.appUserId = PlayerPrefs.GetString("id", "123");
    }


    public void PurchaseItem()
    {
       
        NewPaymentSystem paymentSystem = FindObjectOfType<NewPaymentSystem>();

        string itemName = PIG.actualItemSelected.itemPublicName;
        int itemPrice = PIG.actualItemSelected.itemPrice;
      //  print(PIG.actualItemSelected.itemName);
      //  print(PIG.actualItemSelected.itemPrice);

        paymentSystem.Activate(itemName, itemPrice, OnPotionPurchaseSuccess, OnPotionPurchaseFailure);
    }

    void OnPotionPurchaseSuccess()
    {
        Debug.Log("Item purchased successfully!");
        Debug.Log("Add quantity to playerpref (item name)");
        PlayerPrefs.SetInt(PIG.actualItemSelected.PlayerprefKey, PlayerPrefs.GetInt(PIG.actualItemSelected.PlayerprefKey) + PIG.actualItemSelected.itemCount);

        paymentUiElement.Payment_Success();
    }

    void OnPotionPurchaseFailure()
    {
        Debug.Log("Potion purchase failed!");
        CancelAll();
    }



    /// <summary>
    /// Launches a payment process for a specific item.
    /// </summary>
    /// <param name="itemName">Name of the item being purchased.</param>
    /// <param name="amount">The cost of the item in FCFA.</param>
    /// <param name="successCallback">Callback executed on successful payment.</param>
    /// <param name="failureCallback">Callback executed on payment failure.</param>
    public void Activate(string itemName, int amount, PaymentCallback successCallback, PaymentCallback failureCallback)
    {
        payData.amount = amount;

        if (payData.amount < 100)
        {
            if (PlayerPrefs.GetInt("lang") == 0)
            {
                Toast.Show("Amount must be at least 200 FCFA.", 3, ToastColor.Red, ToastPosition.MiddleCenter);
            }
            else
            {
                Toast.Show("Le montant doit être d'au moins 200 FCFA.", 3, ToastColor.Red, ToastPosition.MiddleCenter);
            }
            return;
        }

        StartCoroutine(Launcher(itemName, successCallback, failureCallback));
    }

    private IEnumerator Launcher(string itemName, PaymentCallback successCallback, PaymentCallback failureCallback)
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Launcher");
        if (!string.IsNullOrEmpty(payData.appUserId))
        {
            var req = new UnityWebRequest("https://75272b11ec.execute-api.eu-west-3.amazonaws.com/api/mpayment/appuser/game/cashout", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(payData));
            req.uploadHandler = new UploadHandlerRaw(jsonToSend);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", token);
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();


            print(req.downloadHandler.text);
            if (req.responseCode == 400 || req.responseCode == 500 || req.isNetworkError || req.isHttpError)
            {
                Debug.Log("Error While Sending: " + req.error);
                Toast.Show(req.error, 3, ToastColor.Red, ToastPosition.MiddleCenter);
                failureCallback?.Invoke();
            }
            else
            {
                transactionEvent = JsonUtility.FromJson<TransactionEvents>(req.downloadHandler.text);

                // Open payment URL automatically
                if (!string.IsNullOrEmpty(transactionEvent.paymentUrl) || !string.IsNullOrEmpty(transactionEvent.redirectUrl))
                {
                    if (!string.IsNullOrEmpty(transactionEvent.paymentUrl))
                    {
                        Application.OpenURL(transactionEvent.paymentUrl);
                    }
                    else
                    {
                        Application.OpenURL(transactionEvent.redirectUrl);
                    }
                }
                else
                {
                    Debug.Log("Payment URL is empty.");
                    if (PlayerPrefs.GetInt("lang") == 0)
                    {
                        Toast.Show("Payment URL is missing.", 3, ToastColor.Red, ToastPosition.MiddleCenter);
                    }
                    else
                    {
                        Toast.Show("L'URL de paiement est manquante.", 3, ToastColor.Red, ToastPosition.MiddleCenter);
                    }
                }

                // Start payment status check
                CheckPaymentState(itemName, successCallback, failureCallback);
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("lang") == 0)
            {
                Toast.Show("User ID is missing.", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
            else
            {
                Toast.Show("L'identifiant de l'utilisateur est manquant.", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
        }
    }


    private void CheckPaymentState(string itemName, PaymentCallback successCallback, PaymentCallback failureCallback)
    {
        attempts = 0;
        StartCoroutine(CheckPayment(itemName, successCallback, failureCallback));
    }

    private IEnumerator CheckPayment(string itemName, PaymentCallback successCallback, PaymentCallback failureCallback)
    {
        while (attempts < 30) //20 max attempts
        {
            attempts++;

            string url = $"https://75272b11ec.execute-api.eu-west-3.amazonaws.com/api/mpayment/appuser/game/{transactionEvent.transactionId}";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Authorization", token);
                yield return webRequest.SendWebRequest();

                print(webRequest.downloadHandler.text);

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    transactionEvent = JsonUtility.FromJson<TransactionEvents>(webRequest.downloadHandler.text);

                    switch (transactionEvent.status)
                    {
                        case "SUCCESSFUL":
                            Debug.Log($"Payment successful for {itemName}!");
                            successCallback?.Invoke();
                            yield break; // Stop coroutine if payment is successful
                        case "PAYMENT_FAILED":
                        case "FAILED":
                            Debug.Log($"Payment failed for {itemName}.");
                            if (PlayerPrefs.GetInt("lang") == 0)
                            {
                                Toast.Show("Payment failed !", 3, ToastColor.Red, ToastPosition.TopCenter);
                            }
                            else
                            {
                                Toast.Show("Le paiement a échoué !", 3, ToastColor.Red, ToastPosition.TopCenter);
                            }
                            failureCallback?.Invoke();
                            yield break; // Stop coroutine on failure
                        default:
                            paymentUiElement.Panel_RESUME.SetActive(false);
                            paymentUiElement.WaitingForPaymentValidation();
                            Debug.Log("Awaiting payment confirmation...");
                            break;
                    }
                }
                else
                {
                    Toast.Show(webRequest.error, 3, ToastColor.Red, ToastPosition.TopCenter);
                    Debug.Log($"Error checking payment: {webRequest.error}");
                }
            }
          
            yield return new WaitForSeconds(5f); // Wait 5 seconds before trying again
        }

        if (PlayerPrefs.GetInt("lang") == 0)
        {
            Toast.Show("Payment confirmation time reached !", 3, ToastColor.Red, ToastPosition.TopCenter);
        }
        else
        {
            Toast.Show("Délai de confirmation du paiement atteint !", 3, ToastColor.Red, ToastPosition.TopCenter);
        }
        Debug.Log("Payment confirmation timeout.");
        failureCallback?.Invoke();
    }




    string stock_customer_number;
    public void setupUserPhoneNumer()
    {
        stock_customer_number = "";

        Debug.Log("setupUserPhoneNumer");

        if (string.IsNullOrEmpty(paymentUiElement.Number_InputField.text))
        {

            if (PlayerPrefs.GetInt("lang") == 0)
            {
                Toast.Show("Please insert your number !", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
            else
            {
                Toast.Show("Veuillez insérer votre numéro !", 3, ToastColor.Red, ToastPosition.TopCenter);
            }
            Debug.Log("Please insert your number !");
        }
        else
        {
            PhoneNumberValidator validator = new PhoneNumberValidator();

            var correctUserPhone = "+" + paymentUiElement.PhoneCode + paymentUiElement.Number_InputField.text; //Example : +225 0709664300 


            bool isInvalid = validator.IsValidPhoneNumber(correctUserPhone);

            if (isInvalid)
            {
                //  Panel_Invalide_Numer_Regex.SetActive(true);
                Debug.Log("Invalide Phone Number !");

                if (PlayerPrefs.GetInt("lang") == 0)
                {
                    Toast.Show("Invalide Phone Number !", 3, ToastColor.Red, ToastPosition.TopCenter);
                }
                else
                {
                    Toast.Show("Numéro de téléphone Invalide !", 3, ToastColor.Red, ToastPosition.TopCenter);
                }
            }
            else
            {
                stock_customer_number = correctUserPhone;
                ///Debug.LogError("Valide Phone Number !");
                //SHOW RESUME

                paymentUiElement.resume_PhoneNumber.text = stock_customer_number;
               paymentUiElement.ShowResume();
            }
        }
    }



    public void ConfirmPaymentAfterResume()
    {
        paymentUiElement.ShowConfirmRESUME_Loader();
        // paymentUiElement.ConfirmRESUME.gameObject.SetActive(false);
        // paymentUiElement.ConfirmRESUME_Loader.SetActive(true);

        //  paymentUiElement.Panel_RESUME.SetActive(false);

        payData.phoneNumber = stock_customer_number;
        paymentUiElement.NumberIsOk();
        //Continue the process 
        paymentUiElement.ActualPaymentObjectSelected.GetComponent<paymentObjPrefab>().Onclick_ConfirmAchatProcess();
    }




    class PhoneNumberValidator
    {
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^\d{7,}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }



    public void CancelAll()
    {
        paymentUiElement.Payment_Failed();

        // Stop payment status checks
        CancelInvoke(nameof(CheckPayment));

        // Reset attempts
        attempts = 0;

        // Reset transaction data
        transactionEvent = new TransactionEvents();

        // Réinitialiser les données de paiement
        //payData = new PaymentData
        //{
        //    id = 1311,
        //    provider = "INTOUCH",
        //    platform = "MINI_CUP",
        //    returnUrl = "",
        //    action = "IN_APP_PURCHASE",
        //    isCashIn = false,
        //    paymentMethodType = "MOBILE_MONEY",
        //    amount = 0,
        //    currencySymbol = "FCFA",
        //    phoneNumber = "",
        //    serviceCode = "",
        //    countryCode = PlayerPrefs.GetString("countryCode"),
        //    appUserId = PlayerPrefs.GetString("id", "123")
        //};


        if (PlayerPrefs.GetInt("lang") == 0)
        {
            Toast.Show("Payment process canceled.", 3, ToastColor.Yellow, ToastPosition.TopCenter);
        }
        else
        {
            Toast.Show("Le processus de paiement a été annulé.", 3, ToastColor.Yellow, ToastPosition.TopCenter);
        }

        Debug.Log("Payment process has been canceled and all states reset.");
    }

}
