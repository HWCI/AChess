﻿using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.iOS;

public class GameManager : MonoBehaviour
{
    public enum ARState
    {
        ScanPlane,
        PlacedScene
    }

    public enum GameState
    {
        Readying,
        PlayerTurn,
        EnemyTurn,
        Resolution
    }

    public float createHeight;
    public float maxRayDistance = 30.0f;
    public ARState state;
    public ARState _state = ARState.ScanPlane;
    public GameState gameStage;
    private GameState _gameStage = GameState.Readying;
    public bool anchored;
    public static GameManager instance;
    public GameObject planePrefab;
    private UnityARAnchorManager unityARAnchorManager;
    public GameObject _currentScene;
    public NavMeshData meshdata;
    
    public delegate void GameStateChange();
    public event GameStateChange OnGameStateChange;
    public delegate void ARStateChange();
    public event ARStateChange OnARStateChange;

    // Use this for initialization
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            state = ARState.ScanPlane;
            gameStage = GameState.Readying;
        }
        if (instance != this) DestroyImmediate(gameObject);
    }

    void Update()
    {
        if (_gameStage != gameStage)
        {
            _gameStage = gameStage;
            OnGameStateChange();
        }if (_state != state)
        {
            _state = state;
            OnARStateChange();
        }
        if (state == ARState.ScanPlane)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
                //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
                if (Physics.Raycast(ray, out hit, maxRayDistance))
                {
                    SetScene(hit.point, hit.transform.parent);

                    //we're going to get the position from the contact point
                    Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", hit.point.x, hit.point.y,
                        hit.point.z));
                }
            }
        }
    }

    void SetScene(Vector3 atPosition, Transform parent)
    {
        if (!_currentScene.activeSelf)
        {
            //_currentScene = Instantiate(planePrefab, atPosition, Quaternion.identity, parent);
            Vector3 offset = (meshdata.position - _currentScene.transform.position);
            _currentScene.transform.position = atPosition;
            meshdata.position = atPosition;
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(meshdata);
            
            //_currentScene.transform.parent = parent;
            _currentScene.SetActive(true);
            state = ARState.PlacedScene;
        }
    }

    void RemoveScene()
    {
        Destroy(_currentScene);
        state = ARState.ScanPlane;
    }
}