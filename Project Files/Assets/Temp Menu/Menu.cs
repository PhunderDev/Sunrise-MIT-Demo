using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private int CurrentLayer = 0;
    private int CurrentOptionsTab = 0;
    [SerializeField] private GameObject[] MenuGameObjects;
    [SerializeField] private GameObject TabsObject;
    [SerializeField] private GameObject ContentObject;
    [SerializeField] private EventSystem EventSys;
    private List<GameObject> Tabs = new List<GameObject>();
    private List<GameObject> OptionsItems = new List<GameObject>();
    private InputSystemUIInputModule InputSysUI;
    private InputActionAsset InputActionAsset;
    private InputAction InitiateMenu;
    private InputAction Submit;
    void Start()
    {
        InputSysUI = EventSys.GetComponent<InputSystemUIInputModule>();
        InputActionAsset = InputSysUI.actionsAsset;
        InitiateMenu = InputActionAsset.FindActionMap("PreStart").FindAction("InitiateMenu");
        Submit = InputActionAsset.FindActionMap("UI").FindAction("Submit");
        for (int i = 0; i < MenuGameObjects.Length; i++)
        {
            Tabs.Add(TabsObject.transform.GetChild(i).gameObject);
            OptionsItems.Add(ContentObject.transform.GetChild(i).gameObject);
        }
        ChangeSubmitAction(InitiateMenu);
        ChangeMenuLayer(0);
    }

    public void ChangeMenuLayer(int layer)
    {
        CurrentLayer = layer;
        if (CurrentLayer == 0) ChangeSubmitAction(InitiateMenu); else ChangeSubmitAction(Submit);
        if (CurrentLayer == 2) ChangeOptionsTab(0);
        for (int i = 0; i < MenuGameObjects.Length; i++)
        {
            MenuGameObjects[i].SetActive(i == CurrentLayer);
        }
        GameObject ParentObject = MenuGameObjects[layer];
        EventSys.SetSelectedGameObject(ParentObject.transform.GetChild(0).gameObject);
    }
    public void ChangeOptionsTab(int tab)
    {
        CurrentOptionsTab = tab;
        for (int i = 0; i < Tabs.Count; i++)
        {
            Button TabButton = Tabs[i].GetComponent<Button>();
            ColorBlock color = TabButton.colors;
            OptionsItems[i].SetActive(i == CurrentOptionsTab);
            if (i == CurrentOptionsTab) 
            {
                
                color.normalColor = Color.red;
                color.highlightedColor = Color.red;
                color.selectedColor = Color.red;
                TabButton.colors = color;
            }
            else
            {
                color.normalColor = Color.white;
                color.highlightedColor = Color.white;
                color.selectedColor = Color.white;
                TabButton.colors = color;

            }
                
        }
        Debug.Log(tab);
    }
    private void ChangeSubmitAction(InputAction action)
    {
        InputSysUI.submit = InputActionReference.Create(action);
        Debug.Log(InputSysUI.submit);
    }
    public void OnToggleTabs(InputValue input)
    {
        if (CurrentLayer != 2) return;
        int value = input.Get().ConvertTo<int>();
        int OptionsTabToChangeTo = (CurrentOptionsTab + value) % Tabs.Count;
        if (OptionsTabToChangeTo < 0) OptionsTabToChangeTo = Tabs.Count - 1;
        ChangeOptionsTab(OptionsTabToChangeTo);
    }
    public void OnCancel()
    {
        if (CurrentLayer > 0)
        {
            ChangeMenuLayer(CurrentLayer - 1);
        }
    }
    public void LoadGame()
    {
        SceneManager.LoadScene("Test Env 1");
    }
}