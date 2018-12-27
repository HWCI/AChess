using System;

namespace UnityEngine.XR.iOS
{
    [Serializable]
    public class ARResourceGroupInfo
    {
        public string author;
        public int version;
    }

    [Serializable]
    public class ARResourceGroupResource
    {
        public string filename;
    }

    [Serializable]
    public class ARResourceGroupContents
    {
        public ARResourceGroupInfo info;
        public ARResourceGroupResource[] resources;
    }

    [Serializable]
    public class ARResourceInfo
    {
        public string author;
        public int version;
    }

    [Serializable]
    public class ARResourceProperties
    {
        public float width;
    }

    [Serializable]
    public class ARResourceFilename
    {
        public string filename;
        public string idiom;
    }

    [Serializable]
    public class ARResourceContents
    {
        public ARResourceFilename[] images;
        public ARResourceInfo info;
        public ARResourceProperties properties;
    }

    [Serializable]
    public class ARReferenceObjectResourceContents
    {
        public ARResourceInfo info;
        public ARResourceFilename[] objects;
        public string referenceObjectName;
    }
}