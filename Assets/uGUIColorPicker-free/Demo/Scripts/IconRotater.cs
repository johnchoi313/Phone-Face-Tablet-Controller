using UnityEngine;
using System.Collections;

public class IconRotater : MonoBehaviour
{
	void Update()
	{
		transform.Rotate (0f, 0f, 0.2f, Space.World);
	}
}
