using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{

    public Node SetLabel(string label)
    {
        this.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = label;
        return this;
    }

}
