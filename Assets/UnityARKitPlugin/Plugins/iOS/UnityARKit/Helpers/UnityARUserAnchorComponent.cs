using System;

namespace UnityEngine.XR.iOS
{
    public class UnityARUserAnchorComponent : MonoBehaviour
    {
        public string AnchorId { get; private set; }

        private void Awake()
        {
            UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += GameObjectAnchorUpdated;
            UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += AnchorRemoved;
            AnchorId = UnityARSessionNativeInterface.GetARSessionNativeInterface()
                .AddUserAnchorFromGameObject(gameObject).identifierStr;
        }

        private void Start()
        {
        }

        public void AnchorRemoved(ARUserAnchor anchor)
        {
            if (anchor.identifier.Equals(AnchorId)) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= GameObjectAnchorUpdated;
            UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= AnchorRemoved;
            UnityARSessionNativeInterface.GetARSessionNativeInterface().RemoveUserAnchor(AnchorId);
        }

        private void GameObjectAnchorUpdated(ARUserAnchor anchor)
        {
            if (anchor.identifier.Equals(AnchorId))
            {
                transform.position = UnityARMatrixOps.GetPosition(anchor.transform);
                transform.rotation = UnityARMatrixOps.GetRotation(anchor.transform);

                Console.WriteLine("Updated: pos = " + transform.position + AnchorId);
            }
        }
    }
}