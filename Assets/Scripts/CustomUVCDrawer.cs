#define ENABLE_LOG
using System;
using System.Collections.Generic;
using Mirror.Examples.Pong;
using UnityEngine;
using UnityEngine.UI;
using Serenegiant.UVC;
using Object = UnityEngine.Object;

public class CustomUVCDrawer : MonoBehaviour, IUVCDrawer
{
	public UVCFilter[] UVCFilters; //For filters during connection and drawing.

	// A GameObject that holds the Material to which the image from the UVC device will be rendered.
    // If not set, the same GameObject to which this script is assigned will be used.
	[SerializeField] private MeshRenderer _renderTarget;

	private const string TAG = "UVCDrawer#";

	private void OnEnable()
	{
		CustomNetworkManager.ConnectionEstablished += ReceivedRenderTarget;
	}

	private void OnDisable()
	{
		CustomNetworkManager.ConnectionEstablished -= ReceivedRenderTarget;
	}

	public bool OnUVCAttachEvent(UVCManager manager, UVCDevice device)
	{
		Console.WriteLine($"{TAG}OnUVCAttachEvent:{device}");
		// XXX The current implementation basically accepts all UVC devices.
		// However, THETA S, THETA V, and THETA Z1 are omitted because they have interfaces that cannot acquire video.
		// Similar to IsUVCEnabled, the UVC device filter should be configurable in the inspector.
		
		var result = !device.IsRicoh || device.IsTHETA;
		result &= UVCFilter.Match(device, UVCFilters);
		return result;
	} 

	public void OnUVCDetachEvent(UVCManager manager, UVCDevice device) //UVC device has been removed
	{
		Console.WriteLine($"{TAG}OnUVCDetachEvent:{device}");
	}

	public bool IsUVCEnabled(UVCManager manager, UVCDevice device)
	{
		return UVCFilter.Match(device, UVCFilters);
	}

	public void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex) //Video acquisition has begun
	{
		Console.WriteLine($"{TAG}HandleOnStartPreview:({tex})");
		_renderTarget.material.mainTexture = tex;
	}

	public void OnUVCStopEvent(UVCManager manager, UVCDevice device) //Video acquisition has finished.
	{
		Console.WriteLine($"{TAG}OnUVCStopEvent:{device}");
	}

	private void ReceivedRenderTarget(GameObject player)
	{
		

	}
	
	public bool IsUACEnabled(UVCManager manager, UVCDevice device) //TODO kept to satisfy IUVC requirements
	{
		return false;
	}

	public void OnUACStartEvent(UVCManager manager, UVCDevice device, AudioClip audioClip)
	{
	}

	public void OnUACStopEvent(UVCManager manager, UVCDevice device)
	{
	}

} 

