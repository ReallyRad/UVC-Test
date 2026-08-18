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
	[SerializeField] private Texture _texture;
	
	private const string TAG = "UVCDrawer#";

	private void OnEnable()
	{
		CustomPlayer.SignalingSelf += ReceivedRenderTarget;
	}

	private void OnDisable()
	{
		CustomPlayer.SignalingSelf -= ReceivedRenderTarget;
	}

	public bool OnUVCAttachEvent(UVCManager manager, UVCDevice device)
	{
		Debug.Log($"{TAG}OnUVCAttachEvent:{device}");
		// XXX The current implementation basically accepts all UVC devices.
		// However, THETA S, THETA V, and THETA Z1 are omitted because they have interfaces that cannot acquire video.
		// Similar to IsUVCEnabled, the UVC device filter should be configurable in the inspector.
		
		var result = !device.IsRicoh || device.IsTHETA;
		result &= UVCFilter.Match(device, UVCFilters);
		return result;
	} 

	public void OnUVCDetachEvent(UVCManager manager, UVCDevice device) //UVC device has been removed
	{
		Debug.Log($"{TAG}OnUVCDetachEvent:{device}");
	}

	public bool IsUVCEnabled(UVCManager manager, UVCDevice device)
	{
		return UVCFilter.Match(device, UVCFilters);
	}

	public void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex) //Video acquisition has begun
	{
		Debug.Log($"{TAG}HandleOnStartPreview:({tex})");
		_texture = tex;
		Debug.Log("assigned incoming texture to uvc drawer _texture");
		if (_renderTarget == null) Debug.Log("render target is null");
		if (_renderTarget.material == null) Debug.Log("render target material is null");
		if (_renderTarget.material.mainTexture == null) Debug.Log("render target material mainTexture is null");
		_renderTarget.material.mainTexture = tex;
		Debug.Log("assigned incoming texture to render target main texture");
	}

	public void OnUVCStopEvent(UVCManager manager, UVCDevice device) //Video acquisition has finished.
	{
		Debug.Log($"{TAG}OnUVCStopEvent:{device}");
	}

	private void ReceivedRenderTarget(Transform player)
	{
		Debug.Log("ReceivedRenderTarget " + player.gameObject.name);
		//_renderTarget.gameObject.SetActive(false);
		_renderTarget = player.gameObject.GetComponent<CustomPlayer>().pano.GetComponent<MeshRenderer>();
		_renderTarget.material.mainTexture = _texture;
	}
	
	public bool IsUACEnabled(UVCManager manager, UVCDevice device) //TODO kept to satisfy IUVC requirements
	{
		return false;
	}

	public void OnUACStartEvent(UVCManager manager, UVCDevice device, AudioClip audioClip) //TODO kept to satisfy IUVC requirements
	{
	}

	public void OnUACStopEvent(UVCManager manager, UVCDevice device) //TODO kept to satisfy IUVC requirements
	{
	}

} 

