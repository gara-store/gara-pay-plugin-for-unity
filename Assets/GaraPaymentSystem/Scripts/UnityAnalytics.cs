using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using Unity.Services.Core.Environments;
using System;


public class UnityAnalytics : MonoBehaviour
{
    public string environment = "production";

    async void Start()
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName(environment);

        try
        {
            await UnityServices.InitializeAsync(options);
            Debug.Log($"Initialized UGS Analytics with user ID: {AnalyticsService.Instance.GetAnalyticsUserID()}");
            AnalyticsService.Instance.StartDataCollection();
            // Verification of required consents
            //   await AnalyticsService.Instance.CheckForRequiredConsents();

            InitAnalytics();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    public class PlatformEvent : Unity.Services.Analytics.Event
    {
        public PlatformEvent(string plateform) : base("plateform")
        {
            SetParameter("plateform", plateform);
        }
    }


    void InitAnalytics()
    {
        // Event creation and registration
        var PlatformEvent = new PlatformEvent(FindObjectOfType<NewPaymentSystem>().payData.platform);
        AnalyticsService.Instance.RecordEvent(PlatformEvent);

        AnalyticsService.Instance.StartDataCollection();
    }

   
}
