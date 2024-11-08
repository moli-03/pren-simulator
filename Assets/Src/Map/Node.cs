using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
	public int Index { get; set; }

    public Node SetLabel(string label)
    {
        this.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = label;
        return this;
    }

	public Node SetColor(Color color) {
		this.GetComponent<Renderer>().material.color = color;
		return this;
	}
	
	public Color GetColor() {
		return this.GetComponent<Renderer>().material.color;
	}
}
