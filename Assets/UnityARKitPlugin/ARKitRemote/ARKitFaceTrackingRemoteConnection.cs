using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using System.Text;
using UnityEngine.XR.iOS.Utils;
#if UNITY_EDITOR
using UnityEditor.Networking.PlayerConnection;

namespace UnityEngine.XR.iOS
{
    public class ARKitFaceTrackingRemoteConnection : MonoBehaviour
    {
        private bool bTexturesInitialized;

        private int currentPlayerID = -1;

        private EditorConnection editorConnection;

        [Header("AR FaceTracking Config Options")]
        public bool enableLightEstimation = true;

        private string guimessage = "none";
        private Texture2D remoteScreenUVTex;

        private Texture2D remoteScreenYTex;
        public bool removeExistingAnchors = true;

        [Header("Run Options")] public bool resetTracking = true;

        // Use this for initialization
        private void Start()
        {
            bTexturesInitialized = false;


            editorConnection = EditorConnection.instance;
            editorConnection.Initialize();
            editorConnection.RegisterConnection(PlayerConnected);
            editorConnection.RegisterDisconnection(PlayerDisconnected);
            editorConnection.Register(ConnectionMessageIds.updateCameraFrameMsgId, UpdateCameraFrame);
            editorConnection.Register(ConnectionMessageIds.addFaceAnchorMsgeId, AddFaceAnchor);
            editorConnection.Register(ConnectionMessageIds.updateFaceAnchorMsgeId, UpdateFaceAnchor);
            editorConnection.Register(ConnectionMessageIds.removePlaneAnchorMsgeId, RemoveFaceAnchor);
            editorConnection.Register(ConnectionMessageIds.screenCaptureYMsgId, ReceiveRemoteScreenYTex);
            editorConnection.Register(ConnectionMessageIds.screenCaptureUVMsgId, ReceiveRemoteScreenUVTex);
        }

        private void PlayerConnected(int playerID)
        {
            currentPlayerID = playerID;
        }

        private void OnGUI()
        {
            if (!bTexturesInitialized)
            {
                if (currentPlayerID != -1)
                {
                    guimessage = "Connected to ARKit Remote device : " + currentPlayerID;

                    if (GUI.Button(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 100),
                        "Start Remote ARKit FaceTracking Session")) SendInitToPlayer();
                }
                else
                {
                    guimessage = "Please connect to player in the console menu";
                }

                GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 100, 400, 50), guimessage);
            }
        }

        private void PlayerDisconnected(int playerID)
        {
            if (currentPlayerID == playerID) currentPlayerID = -1;
        }

        private void OnDestroy()
        {
#if UNITY_2017_1_OR_NEWER
            if (editorConnection != null) editorConnection.DisconnectAll();
#endif
        }


        private void InitializeTextures(UnityARCamera camera)
        {
            var yWidth = camera.videoParams.yWidth;
            var yHeight = camera.videoParams.yHeight;
            var uvWidth = yWidth / 2;
            var uvHeight = yHeight / 2;
            if (remoteScreenYTex == null || remoteScreenYTex.width != yWidth || remoteScreenYTex.height != yHeight)
            {
                if (remoteScreenYTex) Destroy(remoteScreenYTex);
                remoteScreenYTex = new Texture2D(yWidth, yHeight, TextureFormat.R8, false, true);
            }

            if (remoteScreenUVTex == null || remoteScreenUVTex.width != uvWidth || remoteScreenUVTex.height != uvHeight)
            {
                if (remoteScreenUVTex) Destroy(remoteScreenUVTex);
                remoteScreenUVTex = new Texture2D(uvWidth, uvHeight, TextureFormat.RG16, false, true);
            }

            bTexturesInitialized = true;
        }

        private void UpdateCameraFrame(MessageEventArgs mea)
        {
            var serCamera = mea.data.Deserialize<serializableUnityARCamera>();

            var scamera = new UnityARCamera();
            scamera = serCamera;

            InitializeTextures(scamera);

            UnityARSessionNativeInterface.SetStaticCamera(scamera);
            UnityARSessionNativeInterface.RunFrameUpdateCallbacks();
        }

        private void AddFaceAnchor(MessageEventArgs mea)
        {
            var serFaceAnchor = mea.data.Deserialize<serializableUnityARFaceAnchor>();

            ARFaceAnchor arFaceAnchor = serFaceAnchor;
            UnityARSessionNativeInterface.RunAddAnchorCallbacks(arFaceAnchor);
        }

        private void UpdateFaceAnchor(MessageEventArgs mea)
        {
            var serFaceAnchor = mea.data.Deserialize<serializableUnityARFaceAnchor>();

            ARFaceAnchor arFaceAnchor = serFaceAnchor;
            UnityARSessionNativeInterface.RunUpdateAnchorCallbacks(arFaceAnchor);
        }

        private void RemoveFaceAnchor(MessageEventArgs mea)
        {
            var serFaceAnchor = mea.data.Deserialize<serializableUnityARFaceAnchor>();

            ARFaceAnchor arFaceAnchor = serFaceAnchor;
            UnityARSessionNativeInterface.RunRemoveAnchorCallbacks(arFaceAnchor);
        }

        private void ReceiveRemoteScreenYTex(MessageEventArgs mea)
        {
            if (!bTexturesInitialized)
                return;
            remoteScreenYTex.LoadRawTextureData(mea.data);
            remoteScreenYTex.Apply();
            var arVideo = Camera.main.GetComponent<UnityARVideo>();
            if (arVideo) arVideo.SetYTexure(remoteScreenYTex);
        }

        private void ReceiveRemoteScreenUVTex(MessageEventArgs mea)
        {
            if (!bTexturesInitialized)
                return;
            remoteScreenUVTex.LoadRawTextureData(mea.data);
            remoteScreenUVTex.Apply();
            var arVideo = Camera.main.GetComponent<UnityARVideo>();
            if (arVideo) arVideo.SetUVTexure(remoteScreenUVTex);
        }


        private void SendInitToPlayer()
        {
            //we're going to reuse ARSessionConfiguration and only use its lightestimation field.

            var sfem = new serializableFromEditorMessage();
            sfem.subMessageId = SubMessageIds.editorInitARKitFaceTracking;
            var ssc = new serializableARSessionConfiguration(UnityARAlignment.UnityARAlignmentCamera,
                UnityARPlaneDetection.None, false, enableLightEstimation, true);
            var roTracking = resetTracking ? UnityARSessionRunOption.ARSessionRunOptionResetTracking : 0;
            var roAnchors = removeExistingAnchors ? UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors : 0;
            sfem.arkitConfigMsg = new serializableARKitInit(ssc, roTracking | roAnchors);
            SendToPlayer(ConnectionMessageIds.fromEditorARKitSessionMsgId, sfem);
        }

        private void SendToPlayer(System.Guid msgId, byte[] data)
        {
            editorConnection.Send(msgId, data);
        }

        public void SendToPlayer(System.Guid msgId, object serializableObject)
        {
            var arrayToSend = serializableObject.SerializeToByteArray();
            SendToPlayer(msgId, arrayToSend);
        }


        // Update is called once per frame
        private void Update()
        {
        }
    }
}
#endif