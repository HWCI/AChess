using System.Collections.Generic;
using UnityEngine;

public class GUIControllerFlying : MonoBehaviour
{
    [SerializeField] private GameObject CameraObj;

    private bool cameraUp;

    private Vector3 PosDefault;

    [SerializeField] private GameObject queryChan;

    private int querySoundNumber;
    private int targetNum;
    private readonly List<string> targetSounds = new List<string>();


    private void Start()
    {
        PosDefault = CameraObj.transform.localPosition;
        cameraUp = false;
        querySoundNumber = 0;

        foreach (var targetAudio in queryChan.GetComponent<QuerySoundController>().soundData)
            targetSounds.Add(targetAudio.name);
        targetNum = targetSounds.Count - 1;

        ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_IDLE);
    }

    private void OnGUI()
    {
        //AnimationChange ------------------------------------------------

        if (GUI.Button(new Rect(0, 0, 100, 80), "Idle"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_IDLE);
        if (GUI.Button(new Rect(0, 80, 100, 80), "Straight"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_STRAIGHT);
        if (GUI.Button(new Rect(0, 160, 100, 80), "toRight"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_TORIGHT);
        if (GUI.Button(new Rect(0, 240, 100, 80), "toLeft"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_TOLEFT);
        if (GUI.Button(new Rect(0, 320, 100, 80), "Up"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_UP);
        if (GUI.Button(new Rect(0, 400, 100, 80), "Down"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_DOWN);
        if (GUI.Button(new Rect(0, 480, 100, 80), "ItemGet"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_ITEMGET);
        if (GUI.Button(new Rect(0, 560, 100, 80), "ItemGetLoop"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_ITEMGET_LOOP);
        if (GUI.Button(new Rect(0, 640, 100, 80), "Damage"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_DAMAGE);
        if (GUI.Button(new Rect(0, 720, 100, 80), "Disappo"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_DISAPPOINTMENT);
        if (GUI.Button(new Rect(0, 800, 100, 80), "DisappoLoop"))
            ChangeAnimation(QueryAnimationController.QueryChanAnimationType.FLY_DISAPPOINTMENT_LOOP);


        //FaceChange ------------------------------------------------

        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Normal"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.NORMAL_EYEOPEN_MOUTHCLOSE);
        if (GUI.Button(new Rect(Screen.width - 100, 100, 100, 100), "Mabataki"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.NORMAL_EYECLOSE_MOUTHCLOSE);
        if (GUI.Button(new Rect(Screen.width - 100, 200, 100, 100), "Anger"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.ANGER_EYEOPEN_MOUTHCLOSE);
        if (GUI.Button(new Rect(Screen.width - 100, 300, 100, 100), "Sad"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.SAD_EYEOPEN_MOUTHCLOSE);
        if (GUI.Button(new Rect(Screen.width - 100, 400, 100, 100), "Fun"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.FUN_EYEOPEN_MOUTHCLOSE);
        if (GUI.Button(new Rect(Screen.width - 100, 500, 100, 100), "Surprised"))
            ChnageFace(QueryEmotionalController.QueryChanEmotionalType.SURPRISED_EYEOPEN_MOUTHOPEN);


        //CameraChange --------------------------------------------

        if (GUI.Button(new Rect(Screen.width / 2 - 75, 0, 150, 80), "Camera"))
        {
            if (cameraUp)
            {
                CameraObj.GetComponent<Camera>().fieldOfView = 60;
                CameraObj.transform.localPosition = new Vector3(PosDefault.x, PosDefault.y, PosDefault.z);
                cameraUp = false;
            }
            else
            {
                CameraObj.GetComponent<Camera>().fieldOfView = 25;
                CameraObj.transform.localPosition = new Vector3(PosDefault.x, PosDefault.y + 0.5f, PosDefault.z);
                cameraUp = true;
            }
        }


        //Sound ---------------------------------------------------------

        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 100, 50, 100), "<---"))
        {
            querySoundNumber--;
            if (querySoundNumber < 0) querySoundNumber = targetNum;
        }

        if (GUI.Button(new Rect(Screen.width / 2 + 100, Screen.height - 100, 50, 100), "--->"))
        {
            querySoundNumber++;
            if (querySoundNumber > targetNum) querySoundNumber = 0;
        }

        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 70, 200, 70), "Play"))
            queryChan.GetComponent<QuerySoundController>().PlaySoundByNumber(querySoundNumber);

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 30),
            querySoundNumber + 1 + " / " + (targetNum + 1) + "  :  " + targetSounds[querySoundNumber]);


        //SceneChange --------------------------------------------

        if (GUI.Button(new Rect(Screen.width - 100, 700, 100, 100), "to Attack Mode"))
            Application.LoadLevel("03_OperateQuery_Attack");
    }


    private void ChnageFace(QueryEmotionalController.QueryChanEmotionalType faceNumber)
    {
        queryChan.GetComponent<QueryEmotionalController>().ChangeEmotion(faceNumber);
    }


    private void ChangeAnimation(QueryAnimationController.QueryChanAnimationType animNumber)
    {
        queryChan.GetComponent<QueryAnimationController>().ChangeAnimation(animNumber);
    }
}