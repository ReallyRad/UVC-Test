using UnityEngine;
using UnityEngine.XR;

// change namespace to your own namespace
[DefaultExecutionOrder(-1000)]
public class DisableMeshOcclusion : MonoBehaviour {
    
    // Leave this true to disable the occlusion mesh, only set it false if you want to re‑enable it later at runtime
    public bool disableOcclusionMesh = true;

    private void Awake() 
    {
        ApplySetting();
    }
   
    private void ApplySetting() {
        if (XRSettings.enabled && disableOcclusionMesh) {
            XRSettings.useOcclusionMesh = false;
        }
    }
}