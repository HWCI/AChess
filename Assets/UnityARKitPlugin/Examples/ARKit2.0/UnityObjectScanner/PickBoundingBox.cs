﻿namespace UnityEngine.XR.iOS
{
    public class PickBoundingBox : MonoBehaviour
    {
        private const float k_PinchTurnRatio = Mathf.PI / 2;

        private const float k_MinTurnAngle = 0f;

        private const float k_PinchRatio = 1f;

        private const float k_MinPinchDistance = 0f;

        private ARHitTestResult m_ARHit;

        [SerializeField] private Transform m_Back;

        private Vector3 m_BeganFacePosition;

        [SerializeField] private Transform m_Bottom;

        [SerializeField] private Transform m_Front;

        private Bounds m_InitialPinchBounds;

        private float m_InitialPinchDistance;

        private HitType m_LastHitType = HitType.None;

#if UNITY_EDITOR
        private Vector3 m_LastTouchPosition;
#endif

        [SerializeField] private Transform m_Left;

        private RaycastHit m_PhysicsHit;

        private float m_PinchDistance;

        private float m_PinchDistanceDelta;

        [SerializeField] private Transform m_Right;

        [SerializeField] private Material m_SelectedMaterial;

        [SerializeField] private Transform m_Top;

        private float m_TurnAngleDelta;

        [SerializeField] private Material m_UnselectedMaterial;

        public Bounds bounds
        {
            get
            {
                var center = (m_Bottom.position + m_Top.position) / 2f;
                var size = new Vector3(
                    m_Right.localPosition.x - m_Left.localPosition.x,
                    m_Top.localPosition.y - m_Bottom.localPosition.y,
                    m_Front.localPosition.z - m_Back.localPosition.z);

                return new Bounds(center, size);
            }

            set
            {
                var center = value.center;
                var extents = value.extents;
                var size = value.size;

                transform.position = center + Vector3.down * extents.y;

                m_Bottom.transform.localPosition = Vector3.zero;
                m_Top.transform.localPosition = new Vector3(0f, size.y, 0f);
                m_Left.transform.localPosition = new Vector3(-extents.x, extents.y, 0f);
                m_Right.transform.localPosition = new Vector3(extents.x, extents.y, 0f);
                m_Back.transform.localPosition = new Vector3(0f, extents.y, -extents.z);
                m_Front.transform.localPosition = new Vector3(0f, extents.y, extents.z);

                m_Top.transform.localScale = new Vector3(size.x, 1f, size.z);
                m_Bottom.transform.localScale = new Vector3(size.x, 1f, size.z);
                m_Back.transform.localScale = new Vector3(size.x, size.y, 1f);
                m_Front.transform.localScale = new Vector3(size.x, size.y, 1f);
                m_Left.transform.localScale = new Vector3(1f, size.y, size.z);
                m_Right.transform.localScale = new Vector3(1f, size.y, size.z);
            }
        }

        private float rotationOnYAxis
        {
            get { return transform.rotation.eulerAngles.y; }

            set
            {
                var euler = transform.eulerAngles;
                euler.y = value;
                transform.eulerAngles = euler;
            }
        }

        private bool touched
        {
            get { return Input.GetMouseButton(0) || Input.touchCount > 0; }
        }

        private bool DoPhysicsRaycast(Touch touch, ref RaycastHit hitOut)
        {
            var ray = Camera.main.ScreenPointToRay(touch.position);
            var layerMask = 1 << gameObject.layer;
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                return false;

            hitOut = hit;
            return true;
        }

        private bool DoARRaycast(Touch touch, ref ARHitTestResult hitOut)
        {
            var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
            var point = new ARPoint
            {
                x = screenPosition.x,
                y = screenPosition.y
            };

            var hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface()
                .HitTest(point, ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
            if (hitResults.Count < 1)
                return false;

            hitOut = hitResults[0];
            return true;
        }

        private void SetFaceMaterial(Collider collider, Material material)
        {
            var meshRenderer = collider.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = material;
        }

        private void DoBegan(Touch touch)
        {
            if (Input.touchCount > 1)
                return;

            m_LastHitType = HitType.None;

            // First, see if we hit any face
            if (DoPhysicsRaycast(touch, ref m_PhysicsHit))
            {
                m_LastHitType = HitType.BoxFace;
                m_BeganFacePosition = GetFaceFromCollider(m_PhysicsHit.collider).position;
                SetFaceMaterial(m_PhysicsHit.collider, m_SelectedMaterial);
                return;
            }

            // Check AR plane
            var arHit = new ARHitTestResult();
            if (DoARRaycast(touch, ref arHit))
            {
                if (m_ARHit.anchorIdentifier != arHit.anchorIdentifier)
                    transform.position = UnityARMatrixOps.GetPosition(arHit.worldTransform);

                m_LastHitType = HitType.ARPlane;
                m_ARHit = arHit;
            }
        }

        private void DoRotateScale()
        {
            Calculate();

            if (m_LastHitType != HitType.Rotate)
            {
                m_InitialPinchDistance = m_PinchDistance;
                m_InitialPinchBounds = bounds;
            }

            if (Mathf.Abs(m_TurnAngleDelta) > 0f)
                rotationOnYAxis -= m_TurnAngleDelta;

            var scale = m_PinchDistance / m_InitialPinchDistance;
            var size = m_InitialPinchBounds.size * scale;
            if (size.x < .01f || size.y < 0.01f || size.z < 0.01f)
                return;

            var center = bounds.center;
            center = m_Bottom.position + Vector3.up * (size.y / 2f);
            bounds = new Bounds(center, size);
        }

        private void DoMoved(Touch touch)
        {
            if (Input.touchCount == 2)
            {
                ResetMaterial();
                DoRotateScale();

                // This prevents any other move logic from running
                // until the number of touches drops back to zero.
                m_LastHitType = HitType.Rotate;
            }
            else
            {
                switch (m_LastHitType)
                {
                    case HitType.BoxFace:
                        MoveFace(touch);
                        break;
                    case HitType.ARPlane:
                        MovePlane(touch);
                        break;
                }
            }
        }

        private Transform GetFaceFromCollider(Collider collider)
        {
            return collider.gameObject.transform.parent.parent;
        }

        private void MoveFace(Touch touch)
        {
            // http://morroworks.com/Content/Docs/Rays%20closest%20point.pdf

            var faceTransform = GetFaceFromCollider(m_PhysicsHit.collider);
            var a = m_PhysicsHit.normal;
            var A = m_PhysicsHit.point;

            var ray = Camera.main.ScreenPointToRay(touch.position);
            var b = ray.direction;
            var B = ray.origin;

            // Compute 'delta', an offset on the ray defined by the hit's point and normal
            // which is closest to the screen ray. A + delta would be the point on the face ray
            // which is closest to the screen ray
            var c = A - B;
            var ab = Vector3.Dot(a, b);
            var bc = Vector3.Dot(b, c);
            var ac = Vector3.Dot(a, c);
            var denom = 1f - ab * ab;
            if (Mathf.Abs(denom) < 0.001f)
                return;

            var delta = a * ((ab * bc - ac) / denom);

            faceTransform.position = m_BeganFacePosition + delta;

            if (faceTransform == m_Top || faceTransform == m_Bottom)
            {
                var length = m_Top.localPosition.y - m_Bottom.localPosition.y;

                AdjustY(m_Left, length);
                AdjustY(m_Right, length);
                AdjustY(m_Back, length);
                AdjustY(m_Front, length);
            }

            if (faceTransform == m_Left || faceTransform == m_Right)
            {
                var length = m_Right.localPosition.x - m_Left.localPosition.x;

                AdjustX(m_Bottom, length);
                AdjustX(m_Top, length);
                AdjustX(m_Back, length);
                AdjustX(m_Front, length);
            }

            if (faceTransform == m_Back || faceTransform == m_Front)
            {
                var length = m_Front.localPosition.z - m_Back.localPosition.z;

                AdjustZ(m_Top, length);
                AdjustZ(m_Bottom, length);
                AdjustZ(m_Left, length);
                AdjustZ(m_Right, length);
            }

            Recenter();
        }

        private void Recenter()
        {
            // Recenter the box
            var center = bounds.center;
            var delta = center - transform.position;
            delta.y = 0f;
            transform.position += delta;

            m_Top.transform.position -= delta;
            m_Bottom.transform.position -= delta;
            m_Front.transform.position -= delta;
            m_Back.transform.position -= delta;
            m_Left.transform.position -= delta;
            m_Right.transform.position -= delta;
        }

        private void AdjustX(Transform face, float length)
        {
            var localScale = face.localScale;
            localScale.x = length;
            face.localScale = localScale;

            var localPosition = face.localPosition;
            localPosition.x = m_Left.localPosition.x + length / 2;
            face.localPosition = localPosition;
        }

        private void AdjustY(Transform face, float length)
        {
            var localScale = face.localScale;
            localScale.y = length;
            face.localScale = localScale;

            var localPosition = face.localPosition;
            localPosition.y = m_Bottom.localPosition.y + length / 2;
            face.localPosition = localPosition;
        }

        private void AdjustZ(Transform face, float length)
        {
            var localScale = face.localScale;
            localScale.z = length;
            face.localScale = localScale;

            var localPosition = face.localPosition;
            localPosition.z = m_Back.localPosition.z + length / 2;
            face.localPosition = localPosition;
        }

        private void MovePlane(Touch touch)
        {
            var arHit = new ARHitTestResult();
            if (DoARRaycast(touch, ref arHit))
            {
                var hitPosition = UnityARMatrixOps.GetPosition(arHit.worldTransform);

                if (m_ARHit.anchorIdentifier != arHit.anchorIdentifier)
                {
                    // This means we've hit a different plane, so move to it immediately
                    transform.position = hitPosition;
                }
                else
                {
                    // Calculate the difference
                    var lastPosition = UnityARMatrixOps.GetPosition(m_ARHit.worldTransform);
                    var delta = hitPosition - lastPosition;
                    transform.position += delta;
                }

                m_ARHit = arHit;
            }
        }

        private Touch GetTouch()
        {
#if UNITY_EDITOR
            var touch = new Touch();
            if (Input.GetMouseButtonDown(0))
                touch.phase = TouchPhase.Began;
            else if (Input.GetMouseButton(0) && m_LastTouchPosition != Input.mousePosition)
                touch.phase = TouchPhase.Moved;
            else
                touch.phase = TouchPhase.Stationary;

            touch.position = Input.mousePosition;
            m_LastTouchPosition = touch.position;
            return touch;
#else
            return Input.GetTouch(0);
#endif
        }

        private void Update()
        {
            if (touched)
            {
                var touch = GetTouch();
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        DoBegan(touch);
                        break;
                    case TouchPhase.Moved:
                        DoMoved(touch);
                        break;
                }
            }
            else
            {
                ResetMaterial();
                m_LastHitType = HitType.None;
            }
        }

        private void ResetMaterial()
        {
            if (m_LastHitType == HitType.BoxFace && m_PhysicsHit.collider != null)
                SetFaceMaterial(m_PhysicsHit.collider, m_UnselectedMaterial);
        }

        private void Calculate()
        {
            m_PinchDistance = m_PinchDistanceDelta = 0f;
            m_TurnAngleDelta = 0f;

            // if two fingers are touching the screen at the same time ...
            if (Input.touchCount != 2)
                return;

            var touch1 = Input.touches[0];
            var touch2 = Input.touches[1];

            // ... if at least one of them moved ...
            // if (touch1.phase != TouchPhase.Moved && touch2.phase != TouchPhase.Moved)
            //     return;

            // ... check the delta distance between them ...
            m_PinchDistance = Vector2.Distance(touch1.position, touch2.position);
            var prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition,
                touch2.position - touch2.deltaPosition);
            m_PinchDistanceDelta = m_PinchDistance - prevDistance;

            // ... if it's greater than a minimum threshold, it's a pinch!
            if (Mathf.Abs(m_PinchDistanceDelta) > k_MinPinchDistance)
                m_PinchDistanceDelta *= k_PinchRatio;
            else
                m_PinchDistance = m_PinchDistanceDelta = 0;

            // ... or check the delta angle between them ...
            var turnAngle = Angle(touch1.position, touch2.position);
            var prevTurn = Angle(touch1.position - touch1.deltaPosition,
                touch2.position - touch2.deltaPosition);
            m_TurnAngleDelta = Mathf.DeltaAngle(prevTurn, turnAngle);

            // ... if it's greater than a minimum threshold, it's a turn!
            if (Mathf.Abs(m_TurnAngleDelta) > k_MinTurnAngle)
                m_TurnAngleDelta *= k_PinchTurnRatio;
        }

        private static float Angle(Vector2 pos1, Vector2 pos2)
        {
            var from = pos2 - pos1;
            var to = new Vector2(1, 0);

            var result = Vector2.Angle(from, to);
            var cross = Vector3.Cross(from, to);

            if (cross.z > 0f)
                result = 360f - result;

            return result;
        }

        private enum HitType
        {
            None,
            BoxFace,
            ARPlane,
            Rotate
        }
    }
}