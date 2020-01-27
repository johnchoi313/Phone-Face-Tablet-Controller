using UnityEngine;
using System.Collections;

public class RenderColor : MonoBehaviour
{
	public bool useSharedMaterial = true;

	Renderer _renderer;
	
	public Color color
	{
		get
		{
			if(useSharedMaterial)
				return _renderer.sharedMaterial.color;
			else
				return _renderer.material.color;
		}
		set
		{
			if(_renderer == null)
				return;

			if(useSharedMaterial && _renderer.sharedMaterial != null)
				_renderer.sharedMaterial.color = value;
			else
				_renderer.material.color = value;
		}
	}

	void Awake()
	{
		_renderer = GetComponent<Renderer> ();
	}
}
