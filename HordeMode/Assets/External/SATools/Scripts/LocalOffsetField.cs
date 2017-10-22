using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

// To visualize these create a custom editor
// and call Draw from OnSceneGUI +
// StopEditing() from OnDisable
// F.ex.:
// [CustomEditor(typeof(GrabQuirk))]
// class Editor : UnityEditor.Editor
// {
//     protected void OnSceneGUI()
//     {
//         LocalOffsetField.Draw(serializedObject, "jointData.bone");
//     }
//
//     protected void OnDisable()
//     {
//         LocalOffsetField.StopEditing();
//     }
// }
[Serializable]
public class LocalOffsetField : FindTransField
{
	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	[Tooltip("Local rotation from root bone.")]
	Vector3 _localOffset;
	[SerializeField]
	[Tooltip("Local rotation from root bone in degrees.")]
	Vector3 _localRotation;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	public Vector3 localOffset
	{
		get { return _localOffset; }
		set { _localOffset = value; }
	}

	public Quaternion localRotation
	{
		get { return Quaternion.Euler(_localRotation); }
		set { _localRotation = value.eulerAngles; }
	}

	public Vector3 worldPosition
	{
		get { return component.TransformPoint(localOffset); }
	}

	public Quaternion worldRotation
	{
		// TODO: This does not take scale into account
		get { return component.rotation * localRotation; }
	}
	#endregion // Properties

	#region Methods
	public Vector3 TransformLocalPosition(Vector3 localOffset)
	{
		return component.TransformPoint(localOffset);
	}

	public void GetLocal(out Vector3 localPos, out Quaternion localRot)
	{
		localPos = localOffset;
		localRot = localRotation;
	}

	public void GetWorld(out Vector3 worldPos, out Quaternion worldRot)
	{
		worldPos = worldPosition;
		worldRot = worldRotation;
	}

	public void GetWorldWithBone(out Vector3 worldPos, out Quaternion worldRot, Transform bone)
	{
		worldPos = bone.TransformPoint(localOffset);
		worldRot = bone.rotation * localRotation;
	}
	#endregion // Methods

	#region Editor
#if UNITY_EDITOR
	// Should be called from OnSceneGUI
	public static void Draw(SerializedObject obj, string fieldPath, Transform self = null)
	{
		if(obj.isEditingMultipleObjects) { return; }

		var fieldProp = obj.FindProperty(fieldPath);
		if(fieldProp == null) { return; }

		Draw(
			fieldProp,
			GetDefaultLabel(fieldProp),
			self
		);
	}

	public static void Draw(SerializedProperty fieldProperty, string relativePath = null, Transform self = null)
	{
		fieldProperty = GetWithRelativePath(fieldProperty, relativePath);

		Draw(
			fieldProperty,
			GetDefaultLabel(fieldProperty),
			self
		);
	}

	public static void Draw(SerializedProperty fieldProperty, string relativePath, string label, Transform self = null)
	{
		fieldProperty = GetWithRelativePath(fieldProperty, relativePath);

		Draw(
			fieldProperty,
			new GUIContent(label),
			self
		);
	}

	public static void Draw(SerializedProperty fieldProperty, GUIContent label, Transform self = null)
	{
		if(self == null)
		{
			self = GetSelfTrans(fieldProperty.serializedObject);
		}
		
		if(self == null) { return; }

		InternalEditor.Draw(fieldProperty, label, self);
	}

	// Should be called from OnDisable
	public static void StopEditing()
	{
		InternalEditor.StopEditing();
	}

	static GUIContent GetDefaultLabel(SerializedProperty fieldProperty)
	{
		if(fieldProperty.serializedObject.isEditingMultipleObjects)
		{
			return null;
		}

		string label = fieldProperty.serializedObject.targetObject.ToString();
		label = string.Format("{0}.{1}", label, fieldProperty.propertyPath);
		label = label.Replace(".Array.data[", "[");
		return new GUIContent(label);
	}

	static SerializedProperty GetWithRelativePath(SerializedProperty prop, string relativePath)
	{
		if(!string.IsNullOrEmpty(relativePath))
		{
			return prop.FindPropertyRelative(relativePath);
		}

		return prop;
	}

	static Transform GetSelfTrans(SerializedObject self)
	{
		Transform selfTrans = null;

		if(self.targetObject is Component)
		{
			selfTrans = ((Component)self.targetObject).gameObject.transform;
		}

		if(self.targetObject is GameObject)
		{
			selfTrans = ((GameObject)self.targetObject).transform;
		}

		return selfTrans;
	}

	[InitializeOnLoad]
	static class InternalEditor
	{
		static LocalOffsetFieldUndoHelper _undoHelper;
		static SerializedProperty editingProperty;
		static string editingLabel;

		static LocalOffsetFieldUndoHelper undoHelper
		{
			get
			{
				if(_undoHelper == null)
				{
					_undoHelper = ScriptableObject.CreateInstance<LocalOffsetFieldUndoHelper>();
				}

				return _undoHelper;
			}
		}

		static InternalEditor()
		{
			EditorApplication.update += Update;
		}

		static bool IsEditingProperty(SerializedProperty fieldProperty)
		{
			if(editingProperty == null) { return false; }

			if(editingProperty == fieldProperty) { return true; }

			return SerializedProperty.EqualContents(editingProperty, fieldProperty);
		}

		public static void StopEditing()
		{
			editingProperty = null;
		}

		public static void Draw(SerializedProperty fieldProperty, GUIContent label, Transform self)
		{
			if(fieldProperty.hasMultipleDifferentValues)
			{
				return;
			}

			if(editingProperty != null && !IsEditingProperty(fieldProperty))
			{
				return;
			}

			var kindProp = fieldProperty.FindPropertyRelative("kind");
			var parentProp = fieldProperty.FindPropertyRelative("parent");
			var childNameProp = fieldProperty.FindPropertyRelative("childName");
			var findInactiveProp = fieldProperty.FindPropertyRelative("findInactive");
			var findInChildrenProp = fieldProperty.FindPropertyRelative("findInChildren");
			var directCompProp = fieldProperty.FindPropertyRelative("directComponent");
			var localOffsetProp = fieldProperty.FindPropertyRelative("_localOffset");
			var localRotProp = fieldProperty.FindPropertyRelative("_localRotation");

			bool anyNull =
				kindProp == null ||
				parentProp == null ||
				childNameProp == null ||
				findInactiveProp == null ||
				findInChildrenProp == null ||
                directCompProp == null ||
				localOffsetProp == null ||
				localRotProp == null;

			if(anyNull)
			{
				Dbg.LogWarnOnceRelease(
					Dbg.Context(fieldProperty.propertyPath),
					"Property with path {0} lacked a field. Are you sure it's a LocalOffsetField?",
					fieldProperty.propertyPath
				);
				return;
			}

			Transform bone = FindCompField<Transform>.Get(
				kindProp.EnumValue<FindCompKind>(),
				(Transform)parentProp.objectReferenceValue,
				self,
				childNameProp.stringValue,
				findInChildrenProp.boolValue,
                findInactiveProp.boolValue,
				(Transform)directCompProp.objectReferenceValue
			);

			if(bone == null) { return; }

			var localOffset = localOffsetProp.vector3Value;
			var localRotation = Quaternion.Euler(localRotProp.vector3Value);

			EditorHelper.RegisterUndo("Transformed Effect",
				fieldProperty.serializedObject.targetObject,
				undoHelper
			);

			Vector3 worldPos = bone.TransformPoint(localOffset);

			bool clicked = false;
			HandlesHelper.Scope(() =>
			{
				if(IsEditingProperty(fieldProperty)) { Handles.color = Color.green; }

				if(editingProperty == null)
				{
					clicked = Handles.Button(
						worldPos,
						Quaternion.identity,
						size: 0.05f,
						pickSize: 0.05f,
						capFunction: Handles.SphereHandleCap
					);
				}
				else
				{
					Handles.SphereHandleCap(
						controlID: 0,
						position: worldPos,
						rotation: Quaternion.identity,
						size: 0.05f,
						eventType: EventType.Repaint
					);
				}
			});

			if(clicked)
			{
				undoHelper.worldSpaceRot = Quaternion.identity;
				editingProperty = fieldProperty;
				if(Tools.current != Tool.Move && Tools.current != Tool.Rotate)
				{
					Tools.current = Tool.Move;
				}
			}

			GUIStyle style = EditorStyles.toolbarTextField;
			style.CalcSize(label);
			Handles.Label(worldPos, label, style);

			if(!IsEditingProperty(fieldProperty)) { return; }

			fieldProperty.serializedObject.Update();

			HandlesHelper.Scope(() =>
			{
				if(Tools.current == Tool.Rotate)
				{
					if(Tools.pivotRotation == PivotRotation.Local)
					{
						Handles.matrix = bone.localToWorldMatrix;
					}
					else
					{
						Handles.matrix = Matrix4x4.TRS(
							bone.TransformPoint(localOffset),
							Quaternion.identity,
							bone.localScale
						);
					}

					DrawArrowCap(localOffset, localRotation, bone);

					Quaternion newLocalRot = localRotation;

					if(Tools.pivotRotation == PivotRotation.Global)
					{
						var newWorldSpaceRot = Handles.RotationHandle(undoHelper.worldSpaceRot, Vector3.zero);

						// Difference between the the new and the old world space rotation
						var diff = Quaternion.Inverse(undoHelper.worldSpaceRot) * newWorldSpaceRot;

						if(diff != Quaternion.identity)
						{
							// Convert local rot to world
							Quaternion rotInWorld = bone.rotation * localRotation;
							// Apply world difference
							rotInWorld = diff * rotInWorld;
							// Convert back to local space
							newLocalRot = Quaternion.Inverse(bone.rotation) * rotInWorld;

							undoHelper.worldSpaceRot = newWorldSpaceRot;
						}
					}
					else
					{
						newLocalRot = Handles.RotationHandle(localRotation, localOffset);
					}

					if(localRotation != newLocalRot)
					{
						localRotProp.vector3Value = newLocalRot.eulerAngles;
					}
				}
				else if(Tools.current == Tool.Move)
				{
					Handles.matrix = bone.localToWorldMatrix;

					var rot = localRotation;
					if(Tools.pivotRotation == PivotRotation.Global)
					{
						rot = Quaternion.LookRotation(
							bone.InverseTransformDirection(Vector3.forward),
							bone.InverseTransformDirection(Vector3.up)
						);
					}
					var pos = Handles.PositionHandle(localOffset, rot);
					if(localOffset != pos)
					{
						localOffsetProp.vector3Value = pos;
					}
				}
			});

			fieldProperty.serializedObject.ApplyModifiedProperties();

			DrawEditingWarning();

			Event e = Event.current;

			if(e.type == EventType.keyUp)
			{
				if(e.keyCode == KeyCode.Escape)
				{
					StopEditing();
				}
			}
		}

		static void DrawArrowCap(Vector3 localOffset, Quaternion localRotation, Transform bone)
		{
			HandlesHelper.Scope(() =>
			{
				Vector3 worldPos = bone.TransformPoint(localOffset);
				Handles.matrix = Matrix4x4.TRS(
					worldPos,
					bone.rotation * localRotation,
					bone.localScale
				);

				float size = 0.1f;//HandleUtility.GetHandleSize(worldPos);

				Handles.color = Color.blue.WithAlpha(0.5f);
				Handles.ArrowHandleCap(
					controlID: 0,
					position: Vector3.zero,
					rotation: Quaternion.LookRotation(Vector3.forward),
					size: size,
					eventType: EventType.Repaint
				);

				Handles.color = Color.red.WithAlpha(0.5f);
				Handles.ArrowHandleCap(
					controlID: 0,
					position: Vector3.zero,
					rotation: Quaternion.LookRotation(Vector3.right),
					size: size,
					eventType: EventType.Repaint
				);

				Handles.color = Color.green.WithAlpha(0.5f);
				Handles.ArrowHandleCap(
					controlID: 0,
					position: Vector3.zero,
					rotation: Quaternion.LookRotation(Vector3.up),
					size: size,
					eventType: EventType.Repaint
				);
			});
		}

		static void DrawEditingWarning()
		{
			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(10.0f, Screen.height - 80, 200, 50));

			EditorGUILayout.HelpBox("Editing local offset. Esc to exit.", MessageType.Warning);

			GUILayout.EndArea();
			Handles.EndGUI();
		}

		static void Update()
		{
			var undoHelp = undoHelper;
			if(undoHelp == null)
			{
				return;
			}
			if(Tools.pivotRotation != PivotRotation.Global)
			{
				undoHelper.worldSpaceRot = Quaternion.identity;
			}

			if(editingProperty == null)
			{
				return;
			}

			try
			{
				UE.Object targetObj = editingProperty.serializedObject.targetObject;

				if(targetObj is Component)
				{
					Selection.activeGameObject = ((Component)targetObj).gameObject;
				}
			}
			catch(Exception exc)
			{
				exc = new Exception("Exception was thrown. Did you forget to call LocalOffsetField.StopEditing in OnDisable?", exc);
				StopEditing();
				throw exc;
			}
		}
	}
#endif // UNITY_EDITOR
	#endregion // Editor
}
