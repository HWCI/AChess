using UnityEngine.UI;

namespace UnityEngine.XR.iOS
{
    [RequireComponent(typeof(Text))]
    public class PrintBounds : MonoBehaviour
    {
        private Text m_BoundsText;

        [SerializeField] private PickBoundingBox m_Picker;

        private void Start()
        {
            m_BoundsText = GetComponent<Text>();
        }

        // Update is called once per frame
        private void Update()
        {
            var bounds = m_Picker.bounds;
            m_BoundsText.text = string.Format("Bounds:{0}", bounds.ToString("F2")) +
                                string.Format(",size={0} ", bounds.size.ToString("F2"));
            m_BoundsText.text += string.Format("Transform.pos:{0} rot:{1}", m_Picker.transform.position.ToString("F2"),
                m_Picker.transform.rotation.ToString("F2"));
        }
    }
}