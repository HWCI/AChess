using UnityEngine;

public class ModeSwitcher : MonoBehaviour
{
    private int appMode;

    public GameObject ballMake;

    public GameObject ballMove;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void EnableBallCreation(bool enable)
    {
        ballMake.SetActive(enable);
        ballMove.SetActive(!enable);
    }

    private void OnGUI()
    {
        var modeString = appMode == 0 ? "MAKE" : "BREAK";
        if (GUI.Button(new Rect(Screen.width - 150.0f, 0.0f, 150.0f, 100.0f), modeString))
        {
            appMode = (appMode + 1) % 2;
            EnableBallCreation(appMode == 0);
        }
    }
}