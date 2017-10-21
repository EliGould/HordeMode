using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#region Supporting Types
[Flags]
public enum PropFlags
{
	NoNull = 1 << 0,
}

public enum PropReadRestrict
{
	None,
	NoEditRuntime,
	ReadOnly,
}

public enum PropObjRefRestrict
{
	None,
	OnlyAsset,
	OnlySceneObject,
	OnlySelf,
	OnlySelfAndChildren,
	OnlyWithinObject,
}
#endregion // Supporting Types

public class PropAttribute : PropertyAttribute
{
	#region Types
	#endregion // Types

	#region Fields
	readonly PropFlags flags;
	readonly PropReadRestrict readRestriction;
	readonly PropObjRefRestrict objRefRestriction;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public PropAttribute(
		PropFlags flags = (PropFlags)0,
		PropReadRestrict readRestrict = PropReadRestrict.None,
		PropObjRefRestrict objRefRestrict  = PropObjRefRestrict.None
	)
	{
		this.flags = flags;
		this.readRestriction = readRestrict;
		this.objRefRestriction = objRefRestrict;
	}
	#endregion // Methods

	#region Drawer
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(PropAttribute))]
	class Drawer : UnityEditor.PropertyDrawer
	{
		#region Constants
		static readonly Color errorColor = new Color(0.9f, 0.125f, 0.125f, 1.0f);

		static readonly GUIContent content = new GUIContent("");
		#endregion // Constants

		#region Properties
		new PropAttribute attribute
		{
			get { return (PropAttribute)base.attribute; }
		}
		#endregion // Properties

		#region Methods
		bool IsSingleObjRef(SerializedProperty property)
		{
			return property.propertyType == SerializedPropertyType.ObjectReference &&
				!property.hasMultipleDifferentValues;
		}

		bool UseObjField(SerializedProperty property)
		{
			return IsSingleObjRef(property) && attribute.objRefRestriction == PropObjRefRestrict.OnlyAsset;
		}

		protected virtual bool IsReadOnly(SerializedProperty property)
		{
			switch(attribute.readRestriction)
			{
			case PropReadRestrict.ReadOnly:
				return true;
			case PropReadRestrict.NoEditRuntime:
				return EditorApplication.isPlayingOrWillChangePlaymode;
            }

			return false;
		}

		protected virtual bool Validate(SerializedProperty property)
		{
			if(IsSingleObjRef(property))
			{
				UE.Object objRef = property.objectReferenceValue;

				if(0 != (attribute.flags & PropFlags.NoNull))
				{
					if(objRef == null)
					{
						return false;
					}
				}

				if(objRef != null)
				{
					bool isSceneObject = !EditorUtility.IsPersistent(objRef);

					switch(attribute.objRefRestriction)
					{
					case PropObjRefRestrict.OnlyAsset:
						if(isSceneObject) { return false; }
						break;
					case PropObjRefRestrict.OnlySceneObject:
						if(!isSceneObject) { return false; }
						break;
					}

					if(!ValidateHierarchy(property, objRef))
					{
						return false;
					}
				}
			}

			return true;
		}

		bool ValidateHierarchy(SerializedProperty property, UE.Object objRef)
		{
			if(EditorUtility.IsPersistent(objRef)) { return true; }

			bool needCheck =
				attribute.objRefRestriction == PropObjRefRestrict.OnlySelf ||
				attribute.objRefRestriction == PropObjRefRestrict.OnlySelfAndChildren ||
				attribute.objRefRestriction == PropObjRefRestrict.OnlyWithinObject
			;

			if(!needCheck) { return true; }

			SerializedObject owner = property.serializedObject;
			if(owner.isEditingMultipleObjects)
			{
				return true;
			}

			var target = owner.targetObject as Component;
			if(target == null)
			{
				return true;
			}

			Transform targetTrans = target.transform;
			Transform objTrans = null;
			if(objRef is GameObject) { objTrans = ((GameObject)objRef).transform; }
			else if(objRef is Component) { objTrans = ((Component)objRef).transform; }

			if(objTrans == null)
			{
				return true;
			}

			switch(attribute.objRefRestriction)
			{
			case PropObjRefRestrict.OnlySelf:
				return targetTrans == objTrans;
            case PropObjRefRestrict.OnlySelfAndChildren:
				return FindDescending(toFind: objTrans, tree: targetTrans);
            case PropObjRefRestrict.OnlyWithinObject:
				Transform root = targetTrans;
				while(root.parent != null)
				{
					root = root.parent;
				}
				return FindDescending(objTrans, root);
			}

			return true;
        }

		protected virtual void GetTitles(SerializedProperty property, List<string> results)
		{
			if(IsSingleObjRef(property))
			{
				if(0 != (attribute.flags & PropFlags.NoNull))
				{
					results.Add("Can't Be None");
				}

				switch(attribute.objRefRestriction)
				{
				case PropObjRefRestrict.OnlyAsset:
					results.Add("Only Asset");
					break;
				case PropObjRefRestrict.OnlySceneObject:
					results.Add("Only Scene Objects");
					break;
				case PropObjRefRestrict.OnlySelf:
					results.Add("Only Self");
					break;
				case PropObjRefRestrict.OnlySelfAndChildren:
					results.Add("Only Self And Children");
					break;
				case PropObjRefRestrict.OnlyWithinObject:
					results.Add("Only Within Object");
					break;
				}
			}
        }

		static bool FindDescending(Transform toFind, Transform tree)
		{
			if(tree == toFind)
			{
				return true;
			}

			for(int i = 0; i < tree.childCount; ++i)
			{
				Transform child = tree.GetChild(i);
				if(child == toFind)
				{
					return true;
				}
				else if(FindDescending(toFind, child))
				{
					return true;
				}
			}

			return false;
		}

		#region Drawing
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUI.GetPropertyHeight(property);

			using(var titles = TempList<string>.Get())
			{
				GetTitles(property, titles.buffer);

				string title = JoinTitle(titles.buffer);
				height += GetTitleHeight(title);
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, label.tooltip);

			bool enabled = GUI.enabled;
			Color color = GUI.color;

			if(IsReadOnly(property))
			{
				GUI.enabled = false;
			}

			if(!Validate(property))
			{
				GUI.color = errorColor;
			}

			using(var titles = TempList<string>.Get())
			{
				GetTitles(property, titles.buffer);

				string title = JoinTitle(titles.buffer);

				if(!string.IsNullOrEmpty(title))
				{
					content.text = title;
					float titleHeight = GetTitleHeight(title);

					Rect titleRect = position;
					titleRect.height = titleHeight;
					EditorGUIHelper.Indent(ref titleRect, steps: EditorGUI.indentLevel);
					GUI.Label(titleRect, content);
					position.y += titleHeight;
					position.height -= titleHeight;
                }

				if(!UseObjField(property))
				{
					EditorGUI.PropertyField(position, property, label, includeChildren: true);
				}
				else
				{
					property.objectReferenceValue = EditorGUI.ObjectField(
						position,
						label,
						property.objectReferenceValue,
						GetObjRefType(),
						allowSceneObjects: attribute.objRefRestriction != PropObjRefRestrict.OnlyAsset
					);
				}
			}

			GUI.color = color;
			GUI.enabled = enabled;
		}

		string JoinTitle(List<string> titles)
		{
			if(titles.Count == 0) { return null; }

			if(titles.Count == 1) { return string.Format("[{0}]", titles[0]); }

			using(var sb = TempStringBuilder.Get())
			{
				sb.buffer.Append("[");
				for(int i = 0; i < titles.Count; ++i)
				{
					string title = titles[i];
					sb.buffer.AppendFormat("{0}{1}", title, i == titles.Count - 1 ? "" : ", ");
				}
				sb.buffer.Append("]");

				return sb.buffer.ToString();
			}
		}

		float GetTitleHeight(string title)
		{
			if(string.IsNullOrEmpty(title)) { return 0.0f; }

			return GUI.skin.label.CalcHeight(content, EditorGUIUtility.currentViewWidth);
		}

		Type GetObjRefType()
		{
			Type type = fieldInfo.FieldType;

			if(type.BaseType == typeof(Array))
			{
				return type.GetElementType();
			}

			if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				return type.GetGenericArguments()[0];
			}

			return type;
		}
		#endregion // Drawing

		#region Object References
		#endregion // Object References
		#endregion // Methods
	}
#endif // UNITY_EDITOR
	#endregion // Drawing
}
