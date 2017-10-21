using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

// Hides a property if a field has
// a certain value
// Usage:

// [SerializeField]
// int value;

// [SerializeField, ValueRestrict("value", 0, 1, 2)]
// Transform test; // Will only draw if value is 0, 1 or 2

// TODO: Less-/More-than Comparisons
#if UNITY_EDITOR
[InitializeOnLoad]
#endif // UNITY_EDITOR
public class ValueRestrictAttribute : PropertyAttribute
{
	#region Constructors
	public ValueRestrictAttribute(string propertyPath, bool value)
	{
#if UNITY_EDITOR
		this.propertyPath = propertyPath;
		this.validator = CreateValidator(new bool[] { value });
#endif // UNITY_EDITOR
	}

	public ValueRestrictAttribute(string propertyPath, params int[] args)
	{
#if UNITY_EDITOR
		this.propertyPath = propertyPath;
		this.validator = CreateValidator(args);
#endif // UNITY_EDITOR
	}

	public ValueRestrictAttribute(string propertyPath, params string[] args)
	{
#if UNITY_EDITOR
		this.propertyPath = propertyPath;
		this.validator = CreateValidator(args);
#endif // UNITY_EDITOR
	}
	#endregion // Constructors

#if UNITY_EDITOR
	#region Fields
	// The relative path in the parent object to the property
	readonly string propertyPath;
	// The validator
	readonly Func<SerializedProperty, bool> validator;
	#endregion // Fields

	#region Static
	static Dictionary<Type, SerializedPropertyType> lookup;

	static ValueRestrictAttribute()
	{
		lookup = new Dictionary<Type, SerializedPropertyType>();
		lookup[typeof(bool)] = SerializedPropertyType.Boolean;
		lookup[typeof(int)] = SerializedPropertyType.Integer;
		lookup[typeof(string)] = SerializedPropertyType.String;
	}
	#endregion // Static

	#region Methods
	Func<SerializedProperty, bool> CreateValidator<T>(T[] values)
	{
		SerializedPropertyType? pType = lookup.FindOrNullable(typeof(T));
		if(pType == null) { return null; }

		switch(pType.Value)
		{
			case SerializedPropertyType.Boolean: { return (prop) => -1 != Array.IndexOf(values, prop.boolValue); }
			case SerializedPropertyType.Integer: { return (prop) => -1 != Array.IndexOf(values, prop.intValue); }
			case SerializedPropertyType.String: { return (prop) => -1 != Array.IndexOf(values, prop.stringValue); }
			default: { return null; }
		}
	}
	#endregion // Methods

	#region Drawer
	[CustomPropertyDrawer(typeof(ValueRestrictAttribute))]
	class Drawer : PropertyDrawer
	{
		new ValueRestrictAttribute attribute
		{
			get { return (ValueRestrictAttribute)base.attribute; }
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if(!Validate(property))
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

		bool Validate(SerializedProperty property)
		{
			if(attribute.validator == null) { return true; }

			SerializedProperty prop = property.serializedObject.FindProperty(attribute.propertyPath);
			if(prop == null) { return true; }

			bool valid = true;
			try
			{
				valid = attribute.validator(prop);
			}
			catch(Exception)
			{
				Dbg.LogErrorOnce(
					Dbg.Context(attribute),
					"ValueRestrictAttribute validator for {0}.{1} threw. This message will only log once",
					property.serializedObject, property.propertyPath
				);
			}

			return valid;
		}
	}
	#endregion // Drawer
#endif // UNITY_EDITOR
}