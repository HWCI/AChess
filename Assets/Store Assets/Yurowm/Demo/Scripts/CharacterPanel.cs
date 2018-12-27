using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    private Actions actions;
    public Transform actionsPanel;
    public Button buttonPrefab;
    private Camera[] cameras;
    public Transform camerasPanel;

    public GameObject character;
    private PlayerController controller;
    public Slider motionSpeed;
    public Transform weaponsPanel;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        actions = character.GetComponent<Actions>();
        controller = character.GetComponent<PlayerController>();

        foreach (var a in controller.arsenal)
            CreateWeaponButton(a.name);

        CreateActionButton("Stay");
        CreateActionButton("Walk");
        CreateActionButton("Run");
        CreateActionButton("Sitting");
        CreateActionButton("Jump");
        CreateActionButton("Aiming");
        CreateActionButton("Attack");
        CreateActionButton("Damage");
        CreateActionButton("Death Reset", "Death");

        cameras = FindObjectsOfType<Camera>();
        var sort = from s in cameras orderby s.name select s;

        foreach (var c in sort)
            CreateCameraButton(c);

        camerasPanel.GetChild(0).GetComponent<Button>().onClick.Invoke();
    }

    private void CreateWeaponButton(string name)
    {
        var button = CreateButton(name, weaponsPanel);
        button.onClick.AddListener(() => controller.SetArsenal(name));
    }

    private void CreateActionButton(string name)
    {
        CreateActionButton(name, name);
    }

    private void CreateActionButton(string name, string message)
    {
        var button = CreateButton(name, actionsPanel);
        button.onClick.AddListener(() => actions.SendMessage(message, SendMessageOptions.DontRequireReceiver));
    }

    private void CreateCameraButton(Camera c)
    {
        var button = CreateButton(c.name, camerasPanel);
        button.onClick.AddListener(() => { ShowCamera(c); });
    }

    private Button CreateButton(string name, Transform group)
    {
        var obj = Instantiate(buttonPrefab.gameObject);
        obj.name = name;
        obj.transform.SetParent(group);
        obj.transform.localScale = Vector3.one;
        var text = obj.transform.GetChild(0).GetComponent<Text>();
        text.text = name;
        return obj.GetComponent<Button>();
    }

    private void ShowCamera(Camera cam)
    {
        foreach (var c in cameras)
            c.gameObject.SetActive(c == cam);
    }

    private void Update()
    {
        Time.timeScale = motionSpeed.value;
    }

    public void OpenPublisherPage()
    {
        Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/publisher/11008");
    }
}