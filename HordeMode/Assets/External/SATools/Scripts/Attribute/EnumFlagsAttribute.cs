using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class EnumFlagsAttribute : PropertyAttribute
{
	#region Drawer
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	class Drawer : UnityEditor.PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = EditorHelper.GetTooltipFromAttribute(
				fieldInfo,
				defaultValue: label.tooltip
			);

			if(property.hasMultipleDifferentValues)
			{
				position = EditorGUIHelper.StandardLabel(position, label);
				position = EditorGUIHelper.CompensateIndentLevel(position);
				EditorGUIHelper.SinglePopup(position, "Multi-Object Editing Not Supported");
				return;
			}

			if(!fieldInfo.FieldType.IsEnum)
			{
				EditorGUI.PropertyField(
					position,
					property,
					label,
					includeChildren: true
				);
				return;
			}

			// If a single value, property uses EnumValueIndex
			int enumIndex = property.enumValueIndex;
			Enum enumVal;
			if(enumIndex != -1)
			{
				Array values = Enum.GetValues(fieldInfo.FieldType);
				enumVal = (Enum)values.GetValue(enumIndex);
			}
			else
			{
				int intVal = property.intValue;
				enumVal = (Enum)Enum.ToObject(fieldInfo.FieldType, intVal);
			}

			enumVal = EditorGUI.EnumMaskField(position, label, enumVal);

			int newIntVal = Convert.ToInt32(enumVal);
			// Check if a PoT (which would mean a single value)
			// https://stackoverflow.com/questions/1662144/testing-a-flags-enum-value-for-a-single-value

			bool isSingleValue = newIntVal != 0 && (newIntVal & (newIntVal - 1)) == 0;
			if(isSingleValue)
			{
				Array values = Enum.GetValues(fieldInfo.FieldType);
				property.enumValueIndex = Array.IndexOf(values, enumVal);
			}
			else
			{
				property.intValue = newIntVal;
            }
        }
	}
#endif // UNITY_EDITOR
	#endregion // Drawer
}
