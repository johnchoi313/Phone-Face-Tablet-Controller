using UnityEngine;
using System.Collections;

public class OpenBrowser : MonoBehaviour
{
	public void OpenHTML(string url)
	{
		Application.OpenURL(url);
	}
}
