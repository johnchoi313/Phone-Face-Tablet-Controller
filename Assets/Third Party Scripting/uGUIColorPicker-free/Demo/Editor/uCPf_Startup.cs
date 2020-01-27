using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class uCPf_Startup
{
	public static readonly string[] path = new string[]{
		"Assets/uGUIColorPicker-free/Demo/Demo-Index.unity",
		"Assets/uGUIColorPicker-free/Demo/Demo-HowTo.unity",
		"Assets/uGUIColorPicker-free/Demo/Demo-PresetSample.unity",
		"Assets/uGUIColorPicker-free/Demo/Demo-Animation.unity",
		"Assets/uGUIColorPicker-free/Demo/Demo-WorldUI.unity"
	};

	static readonly string tempFilePath = "Assets/uGUIColorPicker-free/Demo/Resources/setupfile";

	static uCPf_Startup()
	{
		if (System.IO.File.Exists (tempFilePath))
			return;
		System.IO.File.Create(tempFilePath);

		if (EditorBuildSettings.scenes.Any (x => x.path == path[0]))
			return;

		EditorApplication.delayCall += delay;
	}

	static void delay()
	{
		confirmWindow.Open ();
		EditorApplication.delayCall -= delay;
	}
}

public class confirmWindow :EditorWindow
{
	[MenuItem("Window/uCPf Demo Setup")]
	public static void Open()
	{
		var w = FindObjectOfType<confirmWindow> ();
		if (w != null)
			EditorWindow.FocusWindowIfItsOpen<confirmWindow> ();
		else
		{
			var window = EditorWindow.GetWindowWithRect<confirmWindow> (
				new Rect (Screen.width - 160, Screen.height - 80, 320, 160),
				true,
				"Setup comfirmation",
				true
			);

			window.wantsMouseMove = false;
			window.Show ();
		}
	}
	
	void OnGUI ()
	{
		EditorGUILayout.LabelField ("Would you want to setup uGUI Color Picker demo?");
		EditorGUILayout.LabelField ("This process will edit the build settings and");
		EditorGUILayout.LabelField ("load a Demo-Index scene.");
		EditorGUILayout.LabelField ("*** The current scene won't be saved ***");
		EditorGUILayout.Space ();

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Yes"))
		{
			var scenes = EditorBuildSettings.scenes.ToList();
			foreach (var p in uCPf_Startup.path)
			{
				if (!EditorBuildSettings.scenes.Any (x => x.path == p))
					scenes.Add (new EditorBuildSettingsScene(p,true));
			}
			EditorBuildSettings.scenes = scenes.ToArray();
			EditorApplication.OpenScene (uCPf_Startup.path[0]);
			Close();
		}
		if (GUILayout.Button ("No"))
			Close();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("You can setup anytime.");
		EditorGUILayout.LabelField ("[Window]->[uCP Demo Setup]");
	}
}
