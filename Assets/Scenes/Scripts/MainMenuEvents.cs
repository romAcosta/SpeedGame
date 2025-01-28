using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private Button StartGameBtn, SettingsBtn,lobbyCodeBtn, settingsCloseBtn, saveChangesBtn;
    private TextElement lobbyCode;    
    private TextField[] keysLeft = new TextField[5], keysRight = new TextField[5];
    [SerializeField]public InputData keybindData;
    private string lobbyCodetxt;
    public UIDocument uiDocument1; 
    public UIDocument uiDocument2; 
    private VisualElement root1; 
    private VisualElement root2;
    [SerializeField] private GameObject targetObject;
    private void Awake()
    {
        root1 = uiDocument1.rootVisualElement;
        root2 = uiDocument2.rootVisualElement;
        ToggleUISettings();
        #region Settings UI variables
        keysLeft[0] = root2.Q<TextField>("KeyLeft1");
        keysLeft[1] = root2.Q<TextField>("KeyLeft2");
        keysLeft[2] = root2.Q<TextField>("KeyLeft3");
        keysLeft[3] = root2.Q<TextField>("KeyLeft4");
        keysLeft[4] = root2.Q<TextField>("KeyLeft5");
        keysRight[0] = root2.Q<TextField>("KeyRight1");
        keysRight[1] = root2.Q<TextField>("KeyRight2");
        keysRight[2] = root2.Q<TextField>("KeyRight3");
        keysRight[3] = root2.Q<TextField>("KeyRight4");
        keysRight[4] = root2.Q<TextField>("KeyRight5");
        ApplyLengthRestriction(keysLeft);
        ApplyLengthRestriction(keysRight);
        Debug.Log("Before Button");
        saveChangesBtn = root2.Q<Button>("saveChangesButton");
        saveChangesBtn.clickable.clicked += OnSaveChangesButtonClicked;
        Debug.Log("After Button");
        settingsCloseBtn = root2.Q<Button>("closeSettingsBtn");
        settingsCloseBtn.clickable.clicked += OnSettingsCloseButtonClicked;
        #endregion
        #region Main UI variables
        lobbyCode = root1.Q<TextElement>("LobbyCode");
        StartGameBtn = root1.Q<Button>("StartGameBtn");
        StartGameBtn.clickable.clicked += OnPlayButtonClicked;
        SettingsBtn = root1.Q<Button>("SettingsBtn");
        SettingsBtn.clickable.clicked += OnSettingsButtonClicked;
        lobbyCodeBtn = root1.Q<Button>("LobbyCodeBtn");
        lobbyCodeBtn.clickable.clicked += OnLobbyButtonClicked;
        #endregion
    }

    private void OnSaveChangesButtonClicked()
    {
        if (keysLeft != null || keysRight != null){
        KeyCode[] keyCodesLeft = new KeyCode[keysLeft.Length];
        KeyCode[] keyCodesRight = new KeyCode[keysRight.Length];
        for (int i = 0; i < keysLeft.Length; i++)
        {
            if (keysLeft[i].value != null || !keyCodesLeft.Contains((KeyCode)Enum.Parse(typeof(KeyCode), keysLeft[i].value, true)))
            {
                System.Enum.TryParse(keysLeft[i].value, true, out KeyCode key);keyCodesLeft[i] = key;
            }
            if (keysRight[i].value != null || !keyCodesRight.Contains((KeyCode)Enum.Parse(typeof(KeyCode), keysRight[i].value, true))){
                System.Enum.TryParse(keysRight[i].value, true, out KeyCode key);
                keyCodesRight[i] = key;
            }
        }
        keyChange(keyCodesLeft, keyCodesRight);
        }else{
            Debug.Log("Null");
        }
    }

    private void keyChange(KeyCode[] keyCodesLeft, KeyCode[] keyCodesRight)
    {
        string checker = "";
        if (keyCodesLeft != null && keyCodesRight != null){
            for (int i = 0; i < keyCodesLeft.Length; i++)
            {
                if(keybindData.leftKeys[i] != keyCodesLeft[i] & !checker.Contains(keyCodesLeft[i].ToString()))
                {
                    checker += keyCodesLeft[i].ToString();
                    keybindData.leftKeys[i] = keyCodesLeft[i];
                }
                if(keybindData.rightKeys[i] != keyCodesRight[i] & !checker.Contains(keyCodesRight[i].ToString()))
                {
                    checker += keyCodesRight[i].ToString();
                    keybindData.rightKeys[i] = keyCodesRight[i];
                }
            }
        }
    }

    private void OnSettingsCloseButtonClicked()
    {
        ToggleUIMain();
        ToggleUISettings();
    }

    private void OnSettingsButtonClicked()
    {
        ToggleUISettings();
        ToggleUIMain();
    }

    private void OnLobbyButtonClicked()
    {
        if (targetObject != null)
        {
            // Get the TargetObjectScript component and call the method
            var targetScript = targetObject.GetComponent<ClientServerScript>();
            if (targetScript != null)
            {
                lobbyCode.text = targetScript.LobbyCodeRequest();
                Debug.Log(lobbyCodetxt);
            }
            else
            {
                Debug.LogError("TargetObjectScript not found on the target object.");
            }
        }
        else
        {
            Debug.LogError("Target object is not assigned.");
        }
    }

    private void OnPlayButtonClicked()
    {
        if (targetObject != null)
        {
            // Get the TargetObjectScript component and call the method
            var targetScript = targetObject.GetComponent<ClientServerScript>();
            if (targetScript != null)
            {
                ToggleUIMain();
                targetScript.LobbyJoinRequest();
            }
            else
            {
                Debug.LogError("TargetObjectScript not found on the target object.");
            }
        }
        else
        {
            Debug.LogError("Target object is not assigned.");
        }
    }
    void ToggleUIMain(){
        if (root1.style.display == DisplayStyle.None)
        {
        root1.style.display = DisplayStyle.Flex;
        }else{
        root1.style.display = DisplayStyle.None;
        }
    }
    void ToggleUISettings(){
        if (root2.style.display == DisplayStyle.None)
        {
        root2.style.display = DisplayStyle.Flex;
        }else{
        root2.style.display = DisplayStyle.None;
        }
    }
    private void ApplyLengthRestriction(TextField[] textFields)
    {
        foreach (var textField in textFields)
        {
            if (textField != null)
            {
                textField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue.Length > 1)
                    {
                        textField.value = evt.newValue.Substring(0, 1);
                    }
                });
            }
        }
    }
}
