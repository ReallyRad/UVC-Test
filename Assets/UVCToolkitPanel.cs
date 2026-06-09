using System;
using System.Collections.Generic;
using LunarConsolePlugin;
using UnityEngine;
using UnityEngine.UIElements;

namespace Serenegiant.UVC
{
    public class UVCToolkitPanel : MonoBehaviour
    {
        [SerializeField]
        private UVCManager manager;

        private UIDocument document;
        
        private Button showConsoleButton;
        private DropdownField cameraDropdown;
        private DropdownField resolutionDropdown;
        private ScrollView controlsContainer;

        private List<UVCManager.CameraInfo> cameras =
            new();

        private UVCManager.CameraInfo currentCamera;
        
        private List<UVCManager.CameraInfo> lastCameras = new();
        
        private const string PREF_WIDTH = "uvc_width";
        private const string PREF_HEIGHT = "uvc_height";
        
        void Start()
        {
            document =
                GetComponent<UIDocument>();

            var root =
                document.rootVisualElement;

            showConsoleButton = root.Q<Button>("show-console-button");
            
            showConsoleButton.clicked += () =>
            {
                LunarConsole.Show();
            };
            
            cameraDropdown =
                root.Q<DropdownField>(
                    "cameraDropdown");

            resolutionDropdown =
                root.Q<DropdownField>(
                    "resolutionDropdown");

            controlsContainer =
                root.Q<ScrollView>(
                    "controlsContainer");

            cameraDropdown.RegisterValueChangedCallback(
                OnCameraChanged);

            resolutionDropdown.RegisterValueChangedCallback(
                OnResolutionChanged);

            Refresh();
        }

        private void Update()
        {
            var current =
                manager.GetAttachedDevices();

            if (current.Count != lastCameras.Count)
            {
                Refresh();
                lastCameras = current;
            }
        }
        
        public void Refresh()
        {
            cameras =
                manager.GetAttachedDevices();
            
            Debug.Log($"Refresh: found {cameras.Count} cameras");

            cameraDropdown.choices.Clear();

            foreach (var camera in cameras)
            {
                cameraDropdown.choices.Add(
                    camera.DeviceName);
            }

            if (cameras.Count > 0)
            {
                cameraDropdown.index = 0;
                SelectCamera(0);
            }
        }

        void OnCameraChanged(
            ChangeEvent<string> evt)
        {
            SelectCamera(
                cameraDropdown.index);
        }

        void SelectCamera(int index)
        {
            if (index < 0 ||
                index >= cameras.Count)
                return;

            currentCamera =
                cameras[index];

            BuildResolutionList();

            BuildControls();
        }

        void BuildResolutionList()
        {
            resolutionDropdown.choices.Clear();

            int savedWidth =
                PlayerPrefs.GetInt(PREF_WIDTH, -1);

            int savedHeight =
                PlayerPrefs.GetInt(PREF_HEIGHT, -1);

            int selectedIndex = 0;

            for (int i = 0;
                 i < currentCamera.SupportedSizes.Length;
                 i++)
            {
                var size =
                    currentCamera.SupportedSizes[i];

                resolutionDropdown.choices.Add(
                    $"{size.Width}x{size.Height}");

                if (size.Width == savedWidth &&
                    size.Height == savedHeight)
                {
                    selectedIndex = i;
                }
            }

            resolutionDropdown.index =
                selectedIndex;
        }
        void OnResolutionChanged(ChangeEvent<string> evt)
        {
            if (currentCamera == null) return;
            int index = resolutionDropdown.index;
            if (index < 0) return;
            var size = currentCamera.SupportedSizes[index];
            PlayerPrefs.SetInt(PREF_WIDTH, (int)size.Width);
            PlayerPrefs.SetInt(PREF_HEIGHT, (int)size.Height);
            PlayerPrefs.Save();
            Debug.Log($"Saved default resolution: {size.Width}x{size.Height}");
        }

        void BuildControls()
        {
            controlsContainer.Clear();

            foreach (ulong ctrl in currentCamera.GetCtrls())
            {
                UVCCtrlInfo info;

                try { info = currentCamera.GetInfo(ctrl); }
                catch { continue; }

                bool isToggle = info.min == 0 && info.max == 1;

                if (isToggle) CreateToggle(ctrl);
                else CreateSlider(ctrl, info); 
            }
        }

        void CreateSlider(
            ulong ctrl,
            UVCCtrlInfo info)
        {
            var label =
                new Label(
                    GetControlName(ctrl));

            controlsContainer.Add(label);

            var slider =
                new SliderInt(
                    info.min,
                    info.max);

            slider.value =
                currentCamera.GetValue(ctrl);

            slider.RegisterValueChangedCallback(
                evt =>
                {
                    currentCamera.SetValue(
                        ctrl,
                        evt.newValue);
                });

            controlsContainer.Add(slider);
        }

        void CreateToggle(
            ulong ctrl)
        {
            var toggle =
                new Toggle(
                    GetControlName(ctrl));

            toggle.value =
                currentCamera.GetValue(ctrl) > 0;

            toggle.RegisterValueChangedCallback(
                evt =>
                {
                    currentCamera.SetValue(
                        ctrl,
                        evt.newValue ? 1 : 0);
                });

            controlsContainer.Add(toggle);
        }

        string GetControlName(
            ulong ctrl)
        {
            return ctrl switch
            {
                0x00000008 => "Exposure",
                0x00000020 => "Focus",
                0x00020000 => "Auto Focus",
                0x00000200 => "Zoom",

                0x80000001 => "Brightness",
                0x80000002 => "Contrast",
                0x80000004 => "Hue",
                0x80000008 => "Saturation",
                0x80000010 => "Sharpness",
                0x80000020 => "Gamma",
                0x80000040 => "White Balance",
                0x80000200 => "Gain",

                _ => $"0x{ctrl:X}"
            };
        }
    }
}