using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ProductItem
{
    [Header("PUBLIC CONFIG")]
    public string Currency; //The currency,  You can leave the currency blank
    public string itemPublicName; // Item name visible to user
    public Sprite itemSprite; // Item sprite ,logo or illustration
    public int itemPrice;   // Item price, visible to user


    [Header("PRIVATE CONFIG")]
    //Not visible to the user
    public string PlayerprefKey; // the playerpref key, which will be upgraded after purchase
    public int itemCount;   // The quantity that will be added to the playerpref value
}

public class ProductItemGeneration : MonoBehaviour
{
    PaymentUiElement paymentUiElement;

    [Header("Item configuration")]
    public List<ProductItem> items = new List<ProductItem>();

    [Header("Prefab for display (UI)")]
    public GameObject itemPrefab; // A prefab containing an image and text

    [Header("Variables")]
    public RectTransform parentPanel; // UI panel where items will be instantiated
    public Button CHOICE_btn; // UI panel where items will be instantiated
    public Sprite onSelected; 
    public Sprite onDeselected;
    private GameObject lastSelectedItem = null; // Reference to last selected item

    private void Awake()
    {
        CHOICE_btn.interactable = false;
    }
    void Start()
    {
        CHOICE_btn.onClick.AddListener(ConfirmChoice);
        paymentUiElement = FindObjectOfType<PaymentUiElement>();
        GenerateItems();
    }

    void GenerateItems()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("No prefab assigned for items!");
            return;
        }

        if (parentPanel == null)
        {
            Debug.LogError("No panels assigned for items!");
            return;
        }

        foreach (var item in items)
        {
            // Set up the item as a panel child
            GameObject newItem = Instantiate(itemPrefab, parentPanel);

            // Configure item data
            UpdateItemUI(newItem, item);

            // Add an Event Listener to display info
            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                // Capture local information to avoid reference problems
                ProductItem capturedItem = item;

                button.onClick.AddListener(() => PrintItemInfo(newItem, capturedItem));
            }

            // Set the initial state to "deselected"
            UpdateItemSprite(newItem, onDeselected);
        }
    }


    void ConfirmChoice()
    {
      paymentUiElement.Panel_ProductItem.SetActive(false);
    }









    void UpdateItemUI(GameObject newItem, ProductItem item)
    {
        // Update name
        Text nameText = newItem.transform.GetChild(0).Find("itemPublicName")?.GetComponent<Text>();
        if (nameText != null) nameText.text = item.itemPublicName;

        // Update quantity
        Text countText = newItem.transform.GetChild(0).Find("CountText")?.GetComponent<Text>();
        if (countText != null) countText.text = $"x{item.itemCount}" ;

        // Update price
        Text priceText = newItem.transform.GetChild(0).Find("PriceText")?.GetComponent<Text>() ;
        if (priceText != null) priceText.text = $"{item.itemPrice} " + item.Currency;

        // Update sprite
        Image itemImage = newItem.transform.GetChild(0).Find("ItemImage")?.GetComponent<Image>();
        if (itemImage != null) itemImage.sprite = item.itemSprite;
    }

    public ProductItem actualItemSelected;
    void PrintItemInfo(GameObject selectedItem, ProductItem item)
    {
        Debug.Log($"Name: {item.itemPublicName}, Quantity: {item.itemCount}, Price: {item.itemPrice}");
        actualItemSelected = item;

        // Update sprites
        UpdateSelectedItem(selectedItem);


        CHOICE_btn.interactable = true;
    }



    void UpdateSelectedItem(GameObject selectedItem)
    {
        // Set sprite for the newly selected item
        UpdateItemSprite(selectedItem, onSelected);

        // Reset sprite for the last selected item (if any)
        if (lastSelectedItem != null && lastSelectedItem != selectedItem)
        {
            UpdateItemSprite(lastSelectedItem, onDeselected);
        }

        // Update the last selected item reference
        lastSelectedItem = selectedItem;
    }

    void UpdateItemSprite(GameObject item, Sprite sprite)
    {
        Image backgroundImage = item.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }
}
