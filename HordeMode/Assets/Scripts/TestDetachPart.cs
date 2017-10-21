using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class TestDetachPart : SafeBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	float force = 100.0f;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	protected void Update()
	{
		if(!App.isSetup) { return; }

		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo))
			{
				var bodyPart = hitInfo.collider.GetComponent<BodyPart>();
				if(bodyPart != null)
				{
					bodyPart.Detach();
					bodyPart.rigid.AddExplosionForce(force, hitInfo.collider.transform.position, 1.0f);
				}
			}
		}
	}
	#endregion // Methods
}
