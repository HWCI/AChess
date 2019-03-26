﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityPointCloudExample : MonoBehaviour
{
    private Vector3[] m_PointCloudData;
    public uint numPointsToShow = 100;
    private List<GameObject> pointCloudObjects;
    public GameObject PointCloudPrefab;

    public void Start()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
        if (PointCloudPrefab != null)
        {
            pointCloudObjects = new List<GameObject>();
            for (var i = 0; i < numPointsToShow; i++) pointCloudObjects.Add(Instantiate(PointCloudPrefab));
        }
    }

    public void ARFrameUpdated(UnityARCamera camera)
    {
        m_PointCloudData = camera.pointCloudData;
    }

    public void Update()
    {
        if (PointCloudPrefab != null && m_PointCloudData != null)
            for (var count = 0; count < Math.Min(m_PointCloudData.Length, numPointsToShow); count++)
            {
                Vector4 vert = m_PointCloudData[count];
                var point = pointCloudObjects[count];
                point.transform.position = new Vector3(vert.x, vert.y, vert.z);
            }
    }
}