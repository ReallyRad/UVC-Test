using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPInputField : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField _IPInputField;

    private void Awake()
    {
        _IPInputField.onEndEdit.AddListener(delegate
        {
            PlayerPrefs.SetString("othersIP", _IPInputField.text); //TODO make it dynamic!
        });
    }

    private void Start()
    {
        SetIpInputField();
    }

    private void SetIpInputField()
    {
        if (_IPInputField.text != null) _IPInputField.text = PlayerPrefs.GetString("othersIP");
    }    

}
