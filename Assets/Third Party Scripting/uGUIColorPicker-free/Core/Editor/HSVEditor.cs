using UnityEngine;
using UnityEditor;

namespace uCPf
{
	[CustomPropertyDrawer(typeof(HSV))]
	public sealed class HSVEditor : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty (position, label, property);

			float h = property.FindPropertyRelative("h").floatValue;
			float s = property.FindPropertyRelative("s").floatValue;
			float v = property.FindPropertyRelative("v").floatValue;
			float a = property.FindPropertyRelative("a").floatValue;
			var hsv = new HSV (h, s, v, a);

			position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
			var color = EditorGUI.ColorField (position,(Color)hsv);

			hsv = color;
			if (hsv.v == 0)
			{
				hsv.s = s;
				hsv.h = h;
			}
			if (hsv.s == 0)
				hsv.h = h;

			property.FindPropertyRelative("h").floatValue = hsv.h;
			property.FindPropertyRelative("s").floatValue = hsv.s;
			property.FindPropertyRelative("v").floatValue = hsv.v;
			property.FindPropertyRelative("a").floatValue = hsv.a;

			EditorGUI.EndProperty ();
		}
	}
}
