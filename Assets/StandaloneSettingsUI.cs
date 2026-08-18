using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Serenegiant.UVC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StandaloneSettingsUI : MonoBehaviour
{
    public delegate void OnSetRepeater(bool on);
    public static OnSetRepeater SetRepeater = delegate {};


    [SerializeField] private UVCManager _manager;
    
    [SerializeField] private Button _showConsoleButton;
    [SerializeField] private TMP_Dropdown _cameraDropdown;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Text _exposureText;
    [SerializeField] private Toggle _hostToggle;
    [SerializeField] private TMP_Text _localIPAddressText;

    private List<UVCManager.CameraInfo> _cameras = new();
    private UVCManager.CameraInfo _currentCamera;
    private List<UVCManager.CameraInfo> _lastCameras = new();
    
    private const string PREF_WIDTH = "uvc_width";
    private const string PREF_HEIGHT = "uvc_height";
    
    private const ulong AUTO_EXPOSURE = 0x2;
    private const ulong AUTO_EXPOSURE_PRIORITY = 0x4;
    private const ulong EXPOSURE = 0x8;
    
    private void Awake()
    {
        _hostToggle.onValueChanged.AddListener(delegate { SetRepeater(_hostToggle.isOn); });
    }
    
    private void Start()
    {
        //_showConsoleButton.clicked += () => LunarConsole.Show();
        _hostToggle.isOn = PlayerPrefs.GetInt("repeater") == 1;
        SetRepeater(_hostToggle.isOn);

        _cameraDropdown.onValueChanged.AddListener(OnCameraChanged);
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        _hostToggle.onValueChanged.AddListener(delegate
        {
            SetRepeater(_hostToggle.isOn);
            SetRepeaterPlayerPrefs(_hostToggle.isOn);
        });

        _localIPAddressText.text = "Local IP Address : " + GetLocalIPAddress();
        
        Refresh();
    }

    private void Update()
    {
        var current = _manager.GetAttachedDevices();

        if (current.Count != _lastCameras.Count)
        {
            Refresh();
            _lastCameras = current;
        }
    }

    public void SetExposure(float exposure)
    {
        _currentCamera.SetValue(EXPOSURE, (int) exposure);
        Debug.Log("SetExposure to " + exposure);
    }
    
    private void Refresh()
    {
        _cameras = _manager.GetAttachedDevices();
        Debug.Log($"Refresh: found {_cameras.Count} cameras");
        
        _cameraDropdown.ClearOptions();
        
        foreach (var camera in _cameras) 
            _cameraDropdown.options.Add(new TMP_Dropdown.OptionData(camera.DeviceName));
        
        if (_cameras.Count > 0)
        {
            _cameraDropdown.SetValueWithoutNotify(0);
            SelectCamera(0);
        }
    }

    private void OnCameraChanged(int newValue)
    {
        SelectCamera(newValue);
    }

    private void SelectCamera(int index)
    {
        if (index < 0 || index >= _cameras.Count) return;
        _currentCamera = _cameras[index];
        _resolutionDropdown.ClearOptions();

        int selectedIndex = 0;
        for (int i = 0; i < _currentCamera.SupportedSize.Length; i++)
        {
            var size = _currentCamera.SupportedSize[i];
            _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{size.Width}x{size.Height}"));
            if (size.Width == PlayerPrefs.GetInt(PREF_WIDTH, -1) && size.Height == PlayerPrefs.GetInt(PREF_HEIGHT, -1)) 
                selectedIndex = i;
        }

        _resolutionDropdown.value = selectedIndex;        
        _currentCamera.UpdateCtrls();
       
        if (_currentCamera != null)
        {
            _currentCamera.SetValue(AUTO_EXPOSURE_PRIORITY, 0); //set auto exposure priority before setting value. not sure if this is the right value
            _currentCamera.SetValue(AUTO_EXPOSURE, 1); //this value allows us to set the exposure manually on the fhd01m
        }
    }

    private void OnResolutionChanged(int newValue)
    {
        var size = _currentCamera.SupportedSize[newValue];

        PlayerPrefs.SetInt(PREF_WIDTH, (int)size.Width);
        PlayerPrefs.SetInt(PREF_HEIGHT, (int)size.Height);
        PlayerPrefs.Save();

        Debug.Log($"Changing to {size.Width}x{size.Height}");

        bool changed = _manager.SetVideoSize(_currentCamera.device, size);

        Debug.Log($"SetVideoSize returned {changed}");
    }

    private void SetRepeaterPlayerPrefs(bool r) //TODO this was in OSCManager. make variable? 
    {
        if (r) PlayerPrefs.SetInt("repeater", 1);
        else PlayerPrefs.SetInt("repeater", 0);
    }

    private string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList) if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return ip.ToString();
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

}
