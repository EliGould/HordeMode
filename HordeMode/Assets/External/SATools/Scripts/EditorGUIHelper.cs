#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public static class EditorGUIHelper
{
	public delegate void DrawDelegate();
	public delegate void ChangeDelegate();

	public const float INDENT_WIDTH = 15.0f;
	public const float propertyFontSize = 18f;
	public const float propertyIndentSize = 15f;
	public static readonly Color errorColor = new Color(0.9f, 0.125f, 0.125f, 1.0f);

	// Consider moving since this is not necessarily EditorGUI
	public static void SetColor(Color color, DrawDelegate draw)
	{
		var c = GUI.color;
		GUI.color = color;
		if(draw != null)
		{
			draw();
		}
		GUI.color = c;
	}

	public static bool ChangeCheck(DrawDelegate draw)
	{
		EditorGUI.BeginChangeCheck();

		if(draw != null)
		{
			draw();
		}

		return EditorGUI.EndChangeCheck();
	}

	public static void IfChangedThen(DrawDelegate ifChanged, ChangeDelegate then)
	{
		if(ChangeCheck(ifChanged) && then != null)
		{
			then();
		}
	}

	public static void Enabled(bool enabled, DrawDelegate draw)
	{
		var old = GUI.enabled;
		// Make sure that we can't have enable true in case of already being not enabled
		GUI.enabled = enabled && old;

		if(draw != null)
		{
			draw();
		}

		GUI.enabled = old;
	}

	public static void MixedValue(bool mixedValue, DrawDelegate draw)
	{
		var old = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = mixedValue;

		if(draw != null)
		{
			draw();
		}

		EditorGUI.showMixedValue = old;
	}

	public static void Depth(int depth, DrawDelegate draw)
	{
		int old = GUI.depth;
		GUI.depth = depth;

		if(draw != null)
		{
			draw();
		}

		GUI.depth = old;
	}

	// TODO: Might want to use PrefixLabel instead
	// A rect that should look like a standard Unity EditorGUI label
	// (i.e. the name before the value)
	// Returns position with the label's width deducted
	public static Rect StandardLabel(Rect position, GUIContent label)
	{
		EditorGUIHelper.Indent(ref position, EditorGUI.indentLevel);

		float labelWidth = EditorGUIUtility.labelWidth - EditorGUI.indentLevel * INDENT_WIDTH;
		Rect labelRect = position;

		labelRect.width = labelWidth;
		GUI.Label(labelRect, label);

		position.x += labelWidth;
		position.width -= labelWidth;

		return position;
	}

	public static void Indent(ref Rect rect, int steps = 1)
	{
		for(int i = 0; i < steps; ++i)
		{
			rect.x += INDENT_WIDTH;
			rect.width -= INDENT_WIDTH;
		}
	}

	public static void Unindent(ref Rect rect, int steps = 1)
	{
		for(int i = 0; i < steps; ++i)
		{
			rect.x -= INDENT_WIDTH;
			rect.width += INDENT_WIDTH;
		}
	}

	// Weirdly enough, indent level does not work properly when you want it to
	// but sometimes popups it gets applied to a lot of elements that you don't
	// want it to.
	public static Rect CompensateIndentLevel(Rect rect)
	{
		Unindent(ref rect, EditorGUI.indentLevel);
		return rect;
	}

	public static void SinglePopup(Rect position, string option, GUIStyle style = null)
	{
		SinglePopup(position, new GUIContent(option));
	}

	public static void SinglePopup(Rect position, GUIContent option, GUIStyle style = null)
	{
		using(var opts = SmallTempArray<GUIContent>.Get())
		{
			opts[0] = option;
			if(style == null)
			{
				EditorGUI.Popup(position, selectedIndex: 0, displayedOptions: opts.buffer);
			}
			else
			{
				// Named params just broke here for whatever reason
				EditorGUI.Popup(position, 0, opts.buffer, style);
			}
		}
	}

	#region Splitter
	// Courtesy of http://answers.unity3d.com/questions/216584/horizontal-line.html
	// Consider moving since this is not necessarily EditorGUI

	public static readonly GUIStyle splitter;
	private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);

	static EditorGUIHelper()
	{
		splitter = new GUIStyle();
		splitter.normal.background = EditorGUIUtility.whiteTexture;
		splitter.stretchWidth = true;
		splitter.margin = new RectOffset(0, 0, 7, 7);
	}

	public static void Splitter(Rect position)
	{
		if(Event.current.type != EventType.Repaint)
		{
			return;
		}

		EditorGUIHelper.SetColor(splitterColor, () =>
		{
			bool isHover = false, isActive = false, on = false, hasKeyboardFocus = false;
			splitter.Draw(position, isHover, isActive, on, hasKeyboardFocus);
		});
	}

	#endregion // Splitter

	#region Property Drawing
	public static void DrawPropertyRecursivly(Rect position, SerializedProperty property)
	{
		// Make sure it doesn't edit the earlier ones
		property = property.Copy();
		bool isExpanded = property.isExpanded;
		if(isExpanded)
		{
			var pos = position;
			pos.yMax = pos.y + propertyFontSize;
			EditorGUI.PropertyField(pos, property);
			position.y += propertyFontSize;
		}
		else
		{
			EditorGUI.PropertyField(position, property);
			return;
		}
		// Ident children
		position.x += propertyIndentSize;
		position.xMax -= propertyIndentSize;
		int depth = property.depth;
		while(isExpanded)
		{
			if(!property.NextVisible(true))
			{
				// Somehow this seems to be needed
				break;
			}
			int currentDepth = property.depth;
			if(currentDepth < depth + 1)
			{
				break;
			}
			else if(currentDepth > depth + 1)
			{
				continue;
			}

			float h = EditorGUI.GetPropertyHeight(property);
			position.yMax = position.y + h;
			DrawPropertyRecursivly(position, property);
			position.y += h;
		}
	}

	public static bool TrySetProperty(SerializedProperty property, object value)
	{
		switch(property.propertyType)
		{
		case SerializedPropertyType.Float:
			if(value is float)
			{
				property.floatValue = (float)value;
				return true;
			}
			else if(value is int)
			{
				property.floatValue = (float)((int)value);
				return true;
			}
			else if(value is double)
			{
				property.floatValue = (float)((double)value);
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.Integer:
		case SerializedPropertyType.ArraySize:
			if(value is int)
			{
				property.intValue = (int)value;
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.Boolean:
			if(value is bool)
			{
				property.boolValue = (bool)value;
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.String:
			if(value is string)
			{
				property.stringValue = (string)value;
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.Vector2:
			if(value is Vector2)
			{
				property.vector2Value = (Vector2)value;
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.Vector3:
			if(value is Vector3)
			{
				property.vector3Value = (Vector3)value;
				return true;
			}
			else
			{
				return false;
			}
		case SerializedPropertyType.Enum:
			{
				var type = value.GetType();
				if(type.IsEnum)
				{
					// Will this work?
					property.enumValueIndex = (int)value;
					return true;
				}
				else
				{
					return false;
				}
			}
		case SerializedPropertyType.Color:
			if(value is Color)
			{
				property.colorValue = (Color)value;
				return true;
			}
			else
			{
				return false;
			}
		default:
			return false;
		}
	}
	#endregion // Property Drawing
}

public class EditorGUILayoutHelper
{
	public delegate void DrawDelegate();

	public static T ObjectField<T>(string label, T obj, bool allowSceneObjects) where T : UnityEngine.Object
	{
		return EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects) as T;
	}

	public static T ObjectField<T>(T obj, bool allowSceneObjects) where T : UnityEngine.Object
	{
		return EditorGUILayout.ObjectField(obj, typeof(T), allowSceneObjects) as T;
	}

	public static void Vertical(DrawDelegate draw, params GUILayoutOption[] opts)
	{
		Vertical(draw, null, opts);
	}

	public static void Vertical(DrawDelegate draw, GUIStyle style, params GUILayoutOption[] opts)
	{
		if(style == null)
		{
			EditorGUILayout.BeginVertical(opts);
		}
		else
		{
			EditorGUILayout.BeginVertical(style, opts);
		}
		if(draw != null)
		{
			draw();
		}
		EditorGUILayout.EndVertical();
	}

	public static void Horizontal(DrawDelegate draw, params GUILayoutOption[] opts)
	{
		EditorGUILayout.BeginHorizontal(opts);
		if(draw != null)
		{
			draw();
		}
		EditorGUILayout.EndHorizontal();
	}

	public static bool Foldout(bool foldout, string content, DrawDelegate draw)
	{
		foldout = EditorGUILayout.Foldout(foldout, content);
		if(foldout)
		{
			EditorGUILayoutHelper.Indent(draw);
		}
		return foldout;
	}

	public static bool ToggleGroup(string label, bool toggle, DrawDelegate draw)
	{
		toggle = EditorGUILayout.BeginToggleGroup(label, toggle);
		EditorGUILayoutHelper.Indent(draw);
		EditorGUILayout.EndToggleGroup();
		return toggle;
	}

	public static Vector2 ScrollView(Vector2 scrollPosition, DrawDelegate draw, params GUILayoutOption[] options)
	{
		bool alwaysShowHorizontal = false, alwaysShowVertical = false;
		return ScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, draw);
	}

	public static Vector2 ScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, DrawDelegate draw, params GUILayoutOption[] options)
	{
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, options);
		if(draw != null)
		{
			draw();
		}
		EditorGUILayout.EndScrollView();
		return scrollPosition;
	}

	public static void FlexibleSpace(DrawDelegate draw)
	{
		GUILayout.FlexibleSpace();
		if(draw != null)
		{
			draw();
		}
		GUILayout.FlexibleSpace();
	}

	public static void HorizontalFlexible(DrawDelegate draw)
	{
		Horizontal(() =>
		{
			FlexibleSpace(() =>
			{
				if(draw != null)
				{
					draw();
				}
			});
		});
	}

	public static void HorizontalFlexibleLeft(DrawDelegate draw)
	{
		Horizontal(() =>
		{
			GUILayout.FlexibleSpace();
			if(draw != null)
			{
				draw();
			}
		});
	}

	public static void HorizontalFlexibleRight(DrawDelegate draw)
	{
		Horizontal(() =>
		{
			if(draw != null)
			{
				draw();
			}
			GUILayout.FlexibleSpace();
		});
	}

	public static void Indent(DrawDelegate draw)
	{
		if(draw != null)
		{
			//			++EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUIHelper.INDENT_WIDTH);
			EditorGUILayout.BeginVertical();
			draw();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//			--EditorGUI.indentLevel;
		}
	}

	#region Splitter
	// Courtesy of http://answers.unity3d.com/questions/216584/horizontal-line.html
	// Consider moving since this is not necessarily EditorGUI

	public static void Splitter(float thickness = 1)
	{
		Splitter(thickness, EditorGUIHelper.splitter);
	}

	public static void Splitter(Color rgb, float thickness = 1)
	{
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, EditorGUIHelper.splitter, GUILayout.Height(thickness));
		EditorGUIHelper.Splitter(position);
	}

	public static void Splitter(float thickness, GUIStyle splitterStyle)
	{
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));
		EditorGUIHelper.Splitter(position);
	}
	#endregion // Splitter
}
#endif