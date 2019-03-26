using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.iOS
{
    public class ARPointCloud
    {
        internal IntPtr nativePtr { get; private set; }

        public int Count
        {
            get { return pointCloud_GetCount(nativePtr); }
        }

        public Vector3[] Points
        {
            get { return GetPoints(); }
        }

        internal static ARPointCloud FromPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            return new ARPointCloud(ptr);
        }

        internal ARPointCloud(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("ptr may not be IntPtr.Zero");

            nativePtr = ptr;
        }
#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		static extern int pointCloud_GetCount(IntPtr ptr);

		[DllImport("__Internal")]
		static extern IntPtr pointCloud_GetPointsPtr(IntPtr ptr);

#else
        private static int pointCloud_GetCount(IntPtr ptr)
        {
            return 0;
        }

        private static IntPtr pointCloud_GetPointsPtr(IntPtr ptr)
        {
            return IntPtr.Zero;
        }
#endif

        private Vector3[] GetPoints()
        {
            var pointsPtr = pointCloud_GetPointsPtr(nativePtr);
            var pointCount = Count;
            if (pointCount <= 0 || pointsPtr == IntPtr.Zero) return null;

            // Load the results into a managed array.
            var floatCount = pointCount * 4;
            var resultVertices = new float[floatCount];
            Marshal.Copy(pointsPtr, resultVertices, 0, floatCount);

            var verts = new Vector3[pointCount];

            for (var count = 0; count < pointCount; count++)
            {
                //convert to Unity coords system
                verts[count].x = resultVertices[count * 4];
                verts[count].y = resultVertices[count * 4 + 1];
                verts[count].z = -resultVertices[count * 4 + 2];
            }

            return verts;
        }
    }
}