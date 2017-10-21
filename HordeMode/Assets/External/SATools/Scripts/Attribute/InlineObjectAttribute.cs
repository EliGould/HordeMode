using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class InlineObjectAttribute : PropertyAttribute
{
	// Will use the object's editor when inlining
	// Note that this requires to root serialized
	// object to also have an Editor since they
	// use GUILayout
	public readonly bool useEditor;

	public InlineObjectAttribute(bool useEditor = false)
	{
		this.useEditor = useEditor;
	}

	#region Drawer
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(InlineObjectAttribute))]
	class Drawer : PropertyDrawer
	{
		new InlineObjectAttribute attribute
		{
			get { return (InlineObjectAttribute)base.attribute; }
		}

		bool CanInlineProperty(SerializedProperty property)
		{
			if(property.serializedObject.isEditingMultipleObjects)
			{
				return false;
			}

			if(property.propertyType == SerializedPropertyType.ObjectReference)
			{
				return true;
			}

			if(!property.isArray) { return false; }

			if(property.arraySize == 0) { return false; }

			return property.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if(!CanInlineProperty(property))
			{
				return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
			}

			if(attribute.useEditor)
			{
				return 0.0f;
			}

			float height = EditorGUI.GetPropertyHeight(property, label, includeChildren: false);

			if(!property.isExpanded) { return height; }

			if(property.isArray)
			{
				// +1 line for Size
				height += EditorGUIUtility.singleLineHeight;
			}

			Iterate(property,
				foreachArrayElem: (SerializedProperty eleProp) =>
				{
					height += EditorGUI.GetPropertyHeight(eleProp);
				},
				foreachChild: (SerializedProperty prop) =>
				{
					height += EditorGUI.GetPropertyHeight(prop);
				}
			);

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, defaultValue: label.tooltip);

			if(!CanInlineProperty(property))
			{
				EditorGUI.PropertyField(position, property, label, includeChildren: true);
				return;
			}

			if(!attribute.useEditor)
			{
				position.height = EditorGUI.GetPropertyHeight(property, label, includeChildren: false);

				EditorGUI.PropertyField(position, property, label, includeChildren: false);
			}
			else
			{
				EditorGUILayout.PropertyField(property, label, includeChildren: false);
			}

			if(!attribute.useEditor)
			{
				DrawWithoutEditor(position, property);
			}
			else
			{
				DrawWithEditor(property);
			}
		}

		void DrawWithoutEditor(Rect position, SerializedProperty property)
		{
			Rect foldoutRect = position;
			foldoutRect.width = 20.0f;
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "");

			if(!property.isExpanded) { return; }

			position.y += position.height;

			if(property.isArray)
			{
				position.height = EditorGUIUtility.singleLineHeight;
				Rect sizeRect = position;
				EditorGUIHelper.Indent(ref sizeRect);

				property.arraySize = EditorGUI.IntField(sizeRect, "Size", property.arraySize);

				position.y += position.height;
			}

			EditorGUIHelper.Indent(ref position);

			Iterate(property,
				foreachArrayElem: (SerializedProperty eleProp) =>
				{
					position.height = EditorGUI.GetPropertyHeight(eleProp);

					EditorGUI.PropertyField(position, eleProp, includeChildren: false);

					if(eleProp.objectReferenceValue != null)
					{
						foldoutRect = position;
						foldoutRect.width = 20.0f;
						eleProp.isExpanded = EditorGUI.Foldout(foldoutRect, eleProp.isExpanded, "");
					}

					position.y += position.height;
				},
				foreachChild: (SerializedProperty prop) =>
				{
					position.height = EditorGUI.GetPropertyHeight(prop);
					Rect indented = position;
					EditorGUIHelper.Indent(ref indented);
					EditorGUI.PropertyField(indented, prop, includeChildren: true);
					position.y += position.height;
				}
			);
		}

		void DrawWithEditor(SerializedProperty property)
		{
			Editor editor = Editor.CreateEditor(property.objectReferenceValue);

			if(editor == null)
			{
				return;
			}

			Rect foldoutRect = GUILayoutUtility.GetLastRect();
			foldoutRect.width = 20.0f;
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "");

			if(property.isExpanded)
			{
				if(property.isArray)
				{
					property.arraySize = EditorGUILayout.IntField("Size", property.arraySize);
				}

				try
				{
					EditorGUILayoutHelper.Indent(() =>
					{
						editor.OnInspectorGUI();
					});
				}
				catch(Exception exc)
				{
					Dbg.LogExc(exc);
					Dbg.LogErrorRelease("Exception thrown with InlineObjectAttriute.useEditor = true. Does the serialized object type have an Editor?");
					throw exc;
				}
			}
		}

		void Iterate(SerializedProperty property, Action<SerializedProperty> foreachArrayElem, Action<SerializedProperty> foreachChild)
		{
			if(property.isArray)
			{
				for(int i = 0; i < property.arraySize; ++i)
				{
					var eleProp = property.GetArrayElementAtIndex(i);
					foreachArrayElem(eleProp);
					Iterate(eleProp, foreachArrayElem, foreachChild);
				}

				return;
			}

			if(property.propertyType != SerializedPropertyType.ObjectReference)
			{
				return;
			}

			if(!property.isExpanded) { return; }

			var obj = (UE.Object)property.objectReferenceValue;

			if(obj == null) { return; }

			var serObj = new SerializedObject(obj);
			serObj.Update();
			SerializedProperty iter = serObj.GetIterator();
			// Needed to initialize iter
			iter.Next(enterChildren: true);
			// To skip the first "script" field
			iter.NextVisible(enterChildren: false);
			while(iter.NextVisible(enterChildren: false))
			{
				foreachChild(iter);
			}
			serObj.ApplyModifiedProperties();
		}
	}
#endif // UNITY_EDITOR
	#endregion // Drawer
}
