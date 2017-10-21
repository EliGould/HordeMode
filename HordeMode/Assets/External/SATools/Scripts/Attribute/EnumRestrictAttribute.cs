using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

// Hides a property if an enum has a
// certain value
// Usage:
// enum Kind
// {
//	 A,
//	 B,
// }

// [SerializeField]
// Kind kind;

// [SerializeField, EnumRestrict("kind", Kind.A)]
// Transform test; // Will only draw if kind == Kind.A
public class EnumRestrictAttribute : PropertyAttribute
{
	#region Fields
	// The relative path in the parent object to the property
	public readonly string propertyPath;
	// The enum type
	public readonly Type enumType;
	// What enum values is required for the property to draw
	public readonly Enum[] acceptableValues;
	#endregion // Fields

	#region Methods
	/// NOTE: params Enum[] does not work, compiler exception
	public EnumRestrictAttribute(string propertyPath, params object[] acceptableValues)
	{
		this.propertyPath = propertyPath;

		this.enumType = acceptableValues.Length == 0 ?
			null :
			acceptableValues[0].GetType();

		this.acceptableValues = new Enum[acceptableValues.Length];
		Array.Copy(
			acceptableValues,
			this.acceptableValues,
			acceptableValues.Length
		);
	}
	#endregion // Methods

	#region Drawer
	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(EnumRestrictAttribute))]
	class Drawer : PropertyDrawer
	{
		new EnumRestrictAttribute attribute
		{
			get { return (EnumRestrictAttribute)base.attribute; }
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if(!CheckKind(property))
			{
				return 0.0f;
			}

			return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if(position.height <= 0.0f) { return; }

			label.tooltip = EditorHelper.GetTooltipFromAttribute(
				fieldInfo,
				defaultValue: label.tooltip
			);

			EditorGUI.PropertyField(position, property, label, includeChildren: true);
		}

		bool CheckKind(SerializedProperty property)
		{
			// Property path something like 
			// "bla.bli.blo"
			// Find 'bli' as that would our parent
			string path = property.propertyPath;

			int index = path.LastIndexOf('.');

			if(index == -1)
			{
				// We're a root property (i.e. 'bla')
				path = attribute.propertyPath;
			}
			else
			{
				// 'bla.bli'
				path = path.Substring(0, index);
				path += "." + attribute.propertyPath;
			}

			SerializedProperty kindProp = property.serializedObject.FindProperty(path);

			// Just always draw on error
			if(kindProp == null) { return true; }

			if(kindProp.propertyType != SerializedPropertyType.Enum) { return true; }

			if(attribute.enumType == null) { return false; }

			// There are no acceptable values
			Enum[] acceptableValues = attribute.acceptableValues;
			if(acceptableValues.Length == 0) { return false; }

			int enumIndex = kindProp.enumValueIndex;

			Array enumVals = Enum.GetValues(attribute.enumType);
			if(enumIndex >= enumVals.Length) { return true; }

			var enumVal = (Enum)enumVals.GetValue(enumIndex);

			return -1 != Array.IndexOf(acceptableValues, enumVal);
		}
	}
	#endif // UNITY_EDITOR
	#endregion // Drawer
}
