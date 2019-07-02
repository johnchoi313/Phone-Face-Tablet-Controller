using UnityEngine;
using System.Collections;
using uCPf;


[RequireComponent(typeof(GradationUI))]
public class BackGroundEffect : MonoBehaviour
{
	GradationUI gradationUI;

	void Start()
	{
		gradationUI = GetComponent<GradationUI>();
		StartCoroutine ( ColorChanger() );
	}

	IEnumerator ColorChanger()
	{
		int vertex;
		Color fromColor;
		Color toColor;
		float lerpTime;

		while (true) 
		{
			vertex = Random.Range(0,3);
			fromColor = gradationUI.colors[vertex];
			toColor = new Color(Random.value,Random.value,Random.value);
			lerpTime=0;

			while (gradationUI.colors[vertex] != toColor)
			{
				gradationUI.colors[vertex] = Color.Lerp(fromColor,toColor,lerpTime/5);
				lerpTime += Time.deltaTime;
				gradationUI.UpdateColors();
				yield return null;
			}

			yield return null;
		}
	}
}
