using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.iOS;

[CreateAssetMenu(fileName = "ARReferenceObjectsSetAsset", menuName = "UnityARKitPlugin/ARReferenceObjectsSetAsset",
    order = 4)]
public class ARReferenceObjectsSetAsset : ScriptableObject
{
    public ARReferenceObjectAsset[] referenceObjectAssets;

    public string resourceGroupName;

    public List<ARReferenceObject> LoadReferenceObjectsInSet()
    {
        var listRefObjects = new List<ARReferenceObject>();

        if (UnityARSessionNativeInterface.IsARKit_2_0_Supported() == false) return listRefObjects;

        var folderPath = Application.streamingAssetsPath + "/ARReferenceObjects/" + resourceGroupName +
                         ".arresourcegroup";
        var contentsJsonPath = Path.Combine(folderPath, "Contents.json");

        var resGroupContents = JsonUtility.FromJson<ARResourceGroupContents>(File.ReadAllText(contentsJsonPath));

        foreach (var arrgr in resGroupContents.resources)
        {
            var objectFolderPath = Path.Combine(folderPath, arrgr.filename);
            var objJsonPath = Path.Combine(objectFolderPath, "Contents.json");
            var resourceContents =
                JsonUtility.FromJson<ARReferenceObjectResourceContents>(File.ReadAllText(objJsonPath));
            var fileToLoad = Path.Combine(objectFolderPath, resourceContents.objects[0].filename);
            var arro = ARReferenceObject.Load(fileToLoad);
            arro.name = resourceContents.referenceObjectName;
            listRefObjects.Add(arro);
        }

        return listRefObjects;
    }
}