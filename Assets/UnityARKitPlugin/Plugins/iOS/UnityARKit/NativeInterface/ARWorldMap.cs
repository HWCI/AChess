using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.iOS
{
    public enum ARWorldMappingStatus
    {
        /** World mapping is not available. */
        ARWorldMappingStatusNotAvailable,

        /** World mapping is available but has limited features.
     For the device's current position, the session’s world map is not recommended for relocalization. */
        ARWorldMappingStatusLimited,

        /** World mapping is actively extending the map with the user's motion.
     The world map will be relocalizable for previously visited areas but is still being updated for the current space. */
        ARWorldMappingStatusExtending,

        /** World mapping has adequately mapped the visible area.
     The map can be used to relocalize for the device's current position. */
        ARWorldMappingStatusMapped
    }

    public class ARWorldMap
    {
        public static bool supported
        {
            get { return worldMap_GetSupported(); }
        }

        public bool Save(string path)
        {
            return worldMap_Save(nativePtr, path);
        }

        public static ARWorldMap Load(string path)
        {
            var ptr = worldMap_Load(path);
            if (ptr == IntPtr.Zero)
                return null;

            return new ARWorldMap(ptr);
        }

        public static ARWorldMap SerializeFromByteArray(byte[] mapByteArray)
        {
            var lengthBytes = mapByteArray.LongLength;
            var handle = GCHandle.Alloc(mapByteArray, GCHandleType.Pinned);
            var newMapPtr = worldMap_SerializeFromByteArray(handle.AddrOfPinnedObject(), lengthBytes);
            handle.Free();
            return new ARWorldMap(newMapPtr);
        }

        public byte[] SerializeToByteArray()
        {
            var worldMapByteArray = new byte[worldMap_SerializedLength(nativePtr)];
            var handle = GCHandle.Alloc(worldMapByteArray, GCHandleType.Pinned);
            worldMap_SerializeToByteArray(nativePtr, handle.AddrOfPinnedObject());
            handle.Free();
            return worldMapByteArray;
        }

        public Vector3 center
        {
            get { return UnityARMatrixOps.GetPosition(worldMap_GetCenter(nativePtr)); }
        }

        public Vector3 extent
        {
            get { return worldMap_GetExtent(nativePtr); }
        }

        public ARPointCloud pointCloud
        {
            get { return ARPointCloud.FromPtr(worldMap_GetPointCloud(nativePtr)); }
        }

        internal IntPtr nativePtr { get; private set; }

        internal static ARWorldMap FromPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            return new ARWorldMap(ptr);
        }

        internal ARWorldMap(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("ptr may not be IntPtr.Zero");

            nativePtr = ptr;
        }

#if !UNITY_EDITOR && UNITY_IOS
        [DllImport("__Internal")]
        static extern bool worldMap_Save(IntPtr worldMapPtr, string path);

        [DllImport("__Internal")]
        static extern IntPtr worldMap_Load(string path);

        [DllImport("__Internal")]
        static extern Vector3 worldMap_GetCenter(IntPtr worldMapPtr);

        [DllImport("__Internal")]
        static extern Vector3 worldMap_GetExtent(IntPtr worldMapPtr);

		[DllImport("__Internal")]
		static extern IntPtr worldMap_GetPointCloud(IntPtr worldMapPtr);

        [DllImport("__Internal")]
        static extern bool worldMap_GetSupported();

		[DllImport("__Internal")]
		static extern long worldMap_SerializedLength(IntPtr worldMapPtr);

		[DllImport("__Internal")]
		static extern void worldMap_SerializeToByteArray(IntPtr worldMapPtr, IntPtr serByteArray);

		[DllImport("__Internal")]
		static extern IntPtr worldMap_SerializeFromByteArray(IntPtr serByteArray, long lengthBytes);
#else
        private static bool worldMap_Save(IntPtr worldMapPtr, string path)
        {
            return false;
        }

        private static IntPtr worldMap_Load(string path)
        {
            return IntPtr.Zero;
        }

        private static Vector3 worldMap_GetCenter(IntPtr worldMapPtr)
        {
            return Vector3.zero;
        }

        private static Vector3 worldMap_GetExtent(IntPtr worldMapPtr)
        {
            return Vector3.zero;
        }

        private static IntPtr worldMap_GetPointCloud(IntPtr worldMapPtr)
        {
            return IntPtr.Zero;
        }

        private static bool worldMap_GetSupported()
        {
            return false;
        }

        private static long worldMap_SerializedLength(IntPtr worldMapPtr)
        {
            return 0;
        }

        private static void worldMap_SerializeToByteArray(IntPtr worldMapPtr, IntPtr serByteArray)
        {
        }

        private static IntPtr worldMap_SerializeFromByteArray(IntPtr serByteArray, long lengthBytes)
        {
            return IntPtr.Zero;
        }
#endif
    }


    [Serializable]
    public class serializableARWorldMap
    {
        private readonly byte[] arWorldMapData;

        public serializableARWorldMap(byte[] inputMapData)
        {
            arWorldMapData = inputMapData;
        }

        public static implicit operator serializableARWorldMap(ARWorldMap arWorldMap)
        {
            if (arWorldMap != null)
                return new serializableARWorldMap(arWorldMap.SerializeToByteArray());
            return new serializableARWorldMap(null);
        }

        public static implicit operator ARWorldMap(serializableARWorldMap serWorldMap)
        {
            if (serWorldMap != null)
                return ARWorldMap.SerializeFromByteArray(serWorldMap.arWorldMapData);
            return null;
        }
    }
}