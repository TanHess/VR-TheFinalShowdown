using UnityEngine;
using System.Collections;
[RequireComponent(typeof(MeshRenderer))]
public class UVScroll : MonoBehaviour 
{
	[SerializeField] float scrollSpeed = 0.5F;
	private Material FinalMat;
	[SerializeField] Vector2 ScrollVector;
	[SerializeField] string FieldName = "_MainTex";
	void OnEnable()
	{
		FinalMat = GetComponent<MeshRenderer>().material;
		if (FinalMat)
		{
			FinalMat.SetTextureOffset(FieldName, new Vector2(0, 0));
		}
		
	}

	void Update() {
		float offset = Mathf.Repeat(Time.time * scrollSpeed,100);
		if (FinalMat)
		{
			FinalMat.SetTextureOffset(FieldName, new Vector2(ScrollVector.x * offset, ScrollVector.y * offset));
		}
		

	}
	void OnDisable()
	{
		FinalMat.SetTextureOffset(FieldName, new Vector2(0,0));
	}
}