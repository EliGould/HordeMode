using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#region Base Types
public class Optional<T>
{
	[SerializeField]
	bool use = false;

	[SerializeField]
	T val = default(T);

	public bool hasValue
	{
		get { return use; }
	}

	public T value
	{
		get { return val; }
	}

	public T GetOrDefault(T _default)
	{
		return hasValue ? val : _default;
	}

	public Optional(T val)
	{
		this.val = val;
		this.use = true;
	}

	public Optional(T val, bool use)
	{
		this.val = val;
		this.use = use;
	}

	public Optional()
	{
		this.val = default(T);
		this.use = false;
	}
}

public static class OptionalExtensions
{
	public static T? AsNullable<T>(this Optional<T> optional) where T : struct
	{
		return optional.hasValue ? optional.value : (T?)null;
	}

	public static T AsRef<T>(this Optional<T> optional) where T : class
	{
		return optional.hasValue ? optional.value : null;
	}
}
#endregion // Base Types

#region Default Implementations
[Serializable]
public class OptionalBool : Optional<bool>
{
	public OptionalBool(bool v) : base(v) {}
	public OptionalBool() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalBool))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalFloat : Optional<float> {
	public OptionalFloat(float v) : base(v) {}
	public OptionalFloat(float v, bool use) : base(v, use) { }
	public OptionalFloat() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalFloat))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalFloatArray : Optional<float[]>
{
	public OptionalFloatArray(float[] v) : base(v) { }
	public OptionalFloatArray() : base() { }
}

[Serializable]
public class OptionalInt : Optional<int> {
	public OptionalInt(int v) : base(v) {}
	public OptionalInt() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalInt))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalString : Optional<string> {
	public OptionalString(string v) : base(v) {}
	public OptionalString() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalString))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalVector2 : Optional<Vector2> {
	public OptionalVector2(Vector2 v) : base(v) {}
	public OptionalVector2() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalVector2))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalVector3 : Optional<Vector3> {
	public OptionalVector3(Vector3 v) : base(v) {}
	public OptionalVector3() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalVector3))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalGameObject : Optional<GameObject> {
	public OptionalGameObject(GameObject v) : base(v) {}
	public OptionalGameObject() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalGameObject))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalTransform : Optional<Transform> {
	public OptionalTransform(Transform v) : base(v) {}
	public OptionalTransform() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalTransform))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalColor : Optional<Color> {
	public OptionalColor(Color v) : base(v) {}
	public OptionalColor(Color v, bool use) : base(v, use) { }
	public OptionalColor() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalColor))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalGradient : Optional<Gradient>
{
	public OptionalGradient(Gradient v) : base(v) { }
	public OptionalGradient() : base() { }
}

[Serializable]
public class OptionalBounds : Optional<Bounds> {
	public OptionalBounds(Bounds b) : base(b) {}
	public OptionalBounds() : base() {}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalBounds))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalCollider : Optional<Collider>
{
	public OptionalCollider(Collider c) : base(c) { }
	public OptionalCollider() : base() { }

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OptionalCollider))]
	public class Drawer : OptionalSingleLineDrawer { }
#endif // UNITY_EDITOR
}

[Serializable]
public class OptionalConfigJointMotionArray : Optional<ConfigurableJointMotion[]>
{
	public OptionalConfigJointMotionArray(ConfigurableJointMotion[] v) : base(v) { }
	public OptionalConfigJointMotionArray() : base() { }
}

[Serializable]
public class OptionalTexture : Optional<Texture>
{
	public OptionalTexture(Texture v) : base(v) { }
	public OptionalTexture() : base() { }
}

[Serializable]
public class OptionalMaterial : Optional<Material>
{
	public OptionalMaterial(Material v) : base(v) { }
	public OptionalMaterial() : base() { }
}

[Serializable]
public class OptionalSprite : Optional<Sprite>
{
	public OptionalSprite(Sprite v) : base(v) { }
	public OptionalSprite() : base() { }
}

#region UI
[Serializable]
public class OptionalFont : Optional<Font>
{
	public OptionalFont(Font v) : base(v) { }
	public OptionalFont() : base() { }
}

[Serializable]
public class OptionalFontStyle : Optional<FontStyle>
{
	public OptionalFontStyle(FontStyle v) : base(v) { }
	public OptionalFontStyle() : base() { }
}

[Serializable]
public class OptionalRectOffset : Optional<RectOffset>
{
	public OptionalRectOffset(RectOffset v) : base(v) { }
	public OptionalRectOffset() : base() { }
}

[Serializable]
public class OptionalTextAnchor : Optional<TextAnchor>
{
	public OptionalTextAnchor(TextAnchor v) : base(v) { }
	public OptionalTextAnchor() : base() { }
}
#endregion // UI

#region Editor
#if UNITY_EDITOR
[Serializable]
public class OptionalModelImporterMaterialSearch : Optional<ModelImporterMaterialSearch>
{
	public OptionalModelImporterMaterialSearch(ModelImporterMaterialSearch v) : base(v) { }
	public OptionalModelImporterMaterialSearch() : base() { }
}
#endif // UNITY_EDITOR
#endregion // Editor
#endregion

#region Drawers
#if UNITY_EDITOR
public class OptionalSingleLineDrawer : PropertyDrawer
{
	const float fontSize = 18f;
	const float identSize = 15f;

	const string useVarName = "use";
	const string valueVarName = "val";

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var useProperty = property.FindPropertyRelative(useVarName);
		return EditorGUI.GetPropertyHeight(useProperty);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		label.tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, defaultValue: null);

		EditorGUI.BeginProperty(position, label, property);

		var useProperty = property.FindPropertyRelative(useVarName);
		var valueProperty = property.FindPropertyRelative(valueVarName);

		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		const float useWidth = 10f;
		const float valueOffset = useWidth + 5f;

		var use = useProperty.boolValue;

		var usePosition = new Rect(position.x, position.y, useWidth, position.height);
		var valuePosition = new Rect(position.x + valueOffset, position.y, position.width - valueOffset, position.height);

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		EditorGUI.PropertyField(usePosition, useProperty, GUIContent.none);
		if(!use)
		{
			GUI.enabled = false;
		}

		EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none);

		if(!use)
		{
			GUI.enabled = true;
		}

		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}

	void DrawValue(Rect position, SerializedProperty property)
	{
		if(property == null)
		{
			position.yMax = position.y + fontSize;
			GUI.Label(position, "Missing");
			return;
		}

		EditorGUIHelper.DrawPropertyRecursivly(position, property);
	}
}
#endif // UNITY_EDITOR
#endregion // Drawers