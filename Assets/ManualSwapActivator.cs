using System.Collections;
using UnityEngine;

public class ManualSwapActivator : MonoBehaviour
{
    [SerializeField] private GameObject _manualSwapScene;
    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f); //Wait for 2 seconds to start the scene because that interferes with camera startup apparently
        //TODO only show after camera is set up. right now the camera must be plugged in before starting the app
        _manualSwapScene.gameObject.SetActive(true);
    }
}
