using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{

	private GameObject Cone;
	private Map map;
	public int Index { get; set; }

	public Node SetMap(Map map) {
		this.map = map;
		return this;
	}

    public Node SetLabel(string label)
    {
        this.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = label;
        return this;
    }

	public Node SetCone(GameObject cone) {
		this.Cone = cone;
		this.map.TriggerMapChange();
		return this;
	}

	public bool HasCone() {
		return this.Cone != null;
	}

	public Node SetColor(Color color) {
		this.GetComponent<Renderer>().material.color = color;
		return this;
	}
	
	public Color GetColor() {
		return this.GetComponent<Renderer>().material.color;
	}
}
