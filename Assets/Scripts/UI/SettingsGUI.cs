using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using extOSC;
using ScriptableObjectArchitecture;
using UnityEngine.Serialization;

public class SettingsGUI : MonoBehaviour
{
    public delegate void OnExposureValueChanged(int value);
    public static OnExposureValueChanged ExposureValueChanged;

    public delegate void OnToggleDim();
    public static OnToggleDim ToggleDim;

    public delegate void OnRotateCamera();
    public static OnRotateCamera RotateCamera;
    
    public delegate void OnSetHost(bool on);
    public static OnSetHost SetHost;
    
    public delegate void OnDebugMenuPressed();
    public static OnDebugMenuPressed DebugMenuPressed;
    
    public delegate void OnRecenterPose();
    public static OnRecenterPose RecenterPose;

    [SerializeField] private GameObject _panel;
    [SerializeField] private IPInputField _ipInputField;
    
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Button _resetYawButton;
    [SerializeField] private Button _debugUIButton;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Text _exposureText;
    [SerializeField] private Toggle _hostToggle;

    private void Awake()
    {
        _dimButton.onClick.AddListener(delegate { ToggleDim(); });
        //_cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        _hostToggle.onValueChanged.AddListener(delegate { SetHost(_hostToggle.isOn); });
        _resetYawButton.onClick.AddListener(delegate { RecenterPose(); });

        _exposureSlider.onValueChanged.AddListener(delegate(float value)
        {
            ExposureValueChanged((int) value);
            PlayerPrefs.SetInt("exposure", (int) value);
            _exposureText.text = "Exposure : " + value;
        });
        
        _rotateCameraButton.onClick.AddListener(delegate { RotateCamera(); });
        _debugUIButton.onClick.AddListener(delegate { DebugMenuPressed(); });
    }

    private void Start()
    {        
        _hostToggle.isOn = PlayerPrefs.GetInt("host") == 1;
        SetHost(_hostToggle.isOn);
        
        if (PlayerPrefs.GetInt("exposure", 1) != 1)
        {
            _exposureSlider.value = PlayerPrefs.GetInt("exposure");
            _exposureText.text = "Exposure : " + _exposureSlider.value;
        }
        
        OSCUtilities.GetLocalHost(); //TODO remove?
    }

    private void Update()
    {     
        //TODO move out of settings GUI
        if (Input.GetKeyDown("m")) ToggleDisplay();
    }

    private void ToggleDisplay()
    {
        if (_panel.GetComponent<CanvasGroup>().alpha == 0f) _panel.GetComponent<CanvasGroup>().alpha = 1f;
        else _panel.GetComponent<CanvasGroup>().alpha = 0f;
    }

}
