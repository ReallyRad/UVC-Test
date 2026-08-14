#define ENABLE_LOG
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Serenegiant.UVC;
public class CustomUVCDrawer : MonoBehaviour, IUVCDrawer
{
	public UVCFilter[] UVCFilters; //For filters during connection and drawing.

	// A GameObject that holds the Material to which the image from the UVC device will be rendered.
    // If not set, the same GameObject to which this script is assigned will be used.
	public List<GameObject> RenderTargets;

	private const string TAG = "UVCDrawer#";

	private Object[] TargetMaterials; //the material to render the image, obtained through either object's skybox, renderer, rawImage or object material, in that order
	private Texture[] SavedTextures; //the original texture
	private Quaternion[] quaternions;

	void Start()
	{
		UpdateRenderTarget();
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
		Console.WriteLine($"{TAG}OnUVCStartEvent:{device}");
		HandleOnStartPreview(tex);
	}

	public void OnUVCStopEvent(UVCManager manager, UVCDevice device) //Video acquisition has finished.
	{
		Console.WriteLine($"{TAG}OnUVCStopEvent:{device}");
		RestoreTexture(); //Restore the texture to which the drawing is directed.
	}


	private void UpdateRenderTarget()
	{
		bool found = false;
		
		if ((RenderTargets != null) && (RenderTargets.Count > 0))
		{
			TargetMaterials = new Object[RenderTargets.Count];
			SavedTextures = new Texture[RenderTargets.Count];
			quaternions = new Quaternion[RenderTargets.Count];
			
			int i = 0;
			foreach (var target in RenderTargets)
			{
				if (target != null)
				{
					var material = TargetMaterials[i] = GetTargetMaterial(target);
					if (material != null) found = true;
					Console.WriteLine($"{TAG}UpdateRenderTarget:material={material}");
				}
				i++;
			}
		}
		if (!found)
		{  
			// This script runs when no rendering targets are found.
			// Attempts to obtain a target from a GameObject that has been added as a component.
			// Set the gameObject to XXX RenderTargets?
			TargetMaterials = new Object[1];
			SavedTextures = new Texture[1];
			quaternions = new Quaternion[1];
			TargetMaterials[0] = GetTargetMaterial(gameObject);
			found = TargetMaterials[0] != null;
		}

		if (!found) throw new UnityException("no target material found.");
		
	} //Update the drawing destination.

	
	private Object GetTargetMaterial(GameObject target) //Gets the Material that renders the image as a texture.Priority: Skybox > Renderer > RawImage > Material
	{
		// Attempting to obtain Skybox
		var skyboxs = target.GetComponents<Skybox>();
		
		if (skyboxs != null)
		{
			foreach (var skybox in skyboxs)
			{
				if (skybox.isActiveAndEnabled && (skybox.material != null))
				{
					RenderSettings.skybox = skybox.material;
					return skybox.material;
				}
			}
		}
		
		// If the Skybox cannot be obtained, try to obtain the Renderer.
		var renderers = target.GetComponents<Renderer>();
		if (renderers != null)
		{
			foreach (var renderer in renderers) if (renderer.enabled && (renderer.material != null)) return renderer.material;
		}
		
		var rawImages = target.GetComponents<RawImage>();
		if (rawImages != null)
		{
			foreach (var rawImage in rawImages) if (rawImage.enabled && (rawImage.material != null)) return rawImage;
		}
		
		// If neither the Skybox nor the Renderer can be obtained, try to obtain the RawImage.
		var material = target.GetComponent<Material>();
		if (material != null) return material;
		
		return null;
	}

	private void RestoreTexture()
	{
		for (int i = 0; i < TargetMaterials.Length; i++)
		{
			var target = TargetMaterials[i];
			try
			{
				if (target is Material) (target as Material).mainTexture = SavedTextures[i];
				else if (target is RawImage) (target as RawImage).texture = SavedTextures[i];
			}
			catch
			{
				Console.WriteLine($"{TAG}RestoreTexture:Exception cought");
			}
			SavedTextures[i] = null;
			quaternions[i] = Quaternion.identity;
		}
	}

	private void HandleOnStartPreview(Texture tex) // Processing at the start of video acquisition
	{
		Console.WriteLine($"{TAG}HandleOnStartPreview:({tex})");
		int i = 0;
		foreach (var target in TargetMaterials)
		{
			if (target is Material)
			{
				Console.WriteLine($"{TAG}HandleOnStartPreview:assign Texture to Material({target})");
				SavedTextures[i++] = (target as Material).mainTexture;
				(target as Material).mainTexture = tex;
			}
			else if (target is RawImage)
			{
				Console.WriteLine($"{TAG}HandleOnStartPreview:assign Texture to RawImage({target})");
				SavedTextures[i++] = (target as RawImage).texture;
				(target as RawImage).texture = tex;
			}
		}
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

