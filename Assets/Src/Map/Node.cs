using Assets.Src.Util;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
	public int Index { get; set; }

	public bool IsEndpoint = false;

	void Start() {
		this.transform.localScale = new Vector3(Constants.NODE_RADIUS * 2, 0.001f, Constants.NODE_RADIUS * 2);
	}

    public Node SetLabel(string label)
    {
        this.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = label;
        return this;
    }

    public string GetLabel()
    {
        return this.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text;
    }

	public Node SetColor(Color color) {
		this.GetComponent<Renderer>().material.color = color;
		return this;
	}
	
	public Color GetColor() {
		return this.GetComponent<Renderer>().material.color;
	}
}
