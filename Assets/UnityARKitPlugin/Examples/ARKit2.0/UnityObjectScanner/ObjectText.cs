using UnityEngine;

public class ObjectText : MonoBehaviour
{
    public TextMesh textMesh;

    // Use this for initialization
    private void Start()
    {
    }

    public void UpdateTextMesh(string nameOfReferenceObject)
    {
        textMesh.text = nameOfReferenceObject;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}