using UnityEngine;

public class ColorPickerTester : MonoBehaviour
{
    public ColorPicker picker;

    public new Renderer renderer;

    // Use this for initialization
    private void Start()
    {
        picker.onValueChanged.AddListener(color => { renderer.material.color = color; });
        renderer.material.color = picker.CurrentColor;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}