using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ColorLabel : MonoBehaviour
{
    private Text label;
    public float maxValue = 255;
    public float minValue;
    public ColorPicker picker;

    public int precision;

    public string prefix = "R: ";

    public ColorValues type;

    private void Awake()
    {
        label = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (Application.isPlaying && picker != null)
        {
            picker.onValueChanged.AddListener(ColorChanged);
            picker.onHSVChanged.AddListener(HSVChanged);
        }
    }

    private void OnDestroy()
    {
        if (picker != null)
        {
            picker.onValueChanged.RemoveListener(ColorChanged);
            picker.onHSVChanged.RemoveListener(HSVChanged);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        label = GetComponent<Text>();
        UpdateValue();
    }
#endif

    private void ColorChanged(Color color)
    {
        UpdateValue();
    }

    private void HSVChanged(float hue, float sateration, float value)
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        if (picker == null)
        {
            label.text = prefix + "-";
        }
        else
        {
            var value = minValue + picker.GetValue(type) * (maxValue - minValue);

            label.text = prefix + ConvertToDisplayString(value);
        }
    }

    private string ConvertToDisplayString(float value)
    {
        if (precision > 0)
            return value.ToString("f " + precision);
        return Mathf.FloorToInt(value).ToString();
    }
}