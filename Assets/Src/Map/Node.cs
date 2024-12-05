using Assets.Src.Util;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
	public int Index { get; set; }
	private string name;


	void Start() {
		this.transform.localScale = new Vector3(Constants.NODE_RADIUS * 2, 0.001f, Constants.NODE_RADIUS * 2);
	}

	public Node SetName(string name)
	{
		this.name = name;
		return this;
	}

	public string GetName()
	{
		return this.name;
	}

	public Node SetMaterial(Material newMaterial)
	{
		Renderer node = this.GetComponent<Renderer>();
		if (node != null)
		{
			GetComponent<Renderer>().material = newMaterial;
		}

		return this;
	}

	public Material GetMaterial()
	{
		return this.GetComponent<Renderer>().material;
	}

	public Node SetColor(Color color)
	{
		this.GetComponent<Renderer>().material.color = color;
		return this;
	}

	public Color GetColor()
	{
		return this.GetComponent<Renderer>().material.color;
	}
}
