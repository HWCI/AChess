using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ParticlePainter : MonoBehaviour
{
    public ColorPicker colorPicker;
    private Color currentColor = Color.white;
    private List<Vector3> currentPaintVertices;
    private ParticleSystem currentPS;
    private bool frameUpdated;
    public float maxDistanceThreshold;
    public float minDistanceThreshold;
    public ParticleSystem painterParticlePrefab;
    private int paintMode; //0 = off, 1 = pick color, 2 = paint
    private List<ParticleSystem> paintSystems;
    private ParticleSystem.Particle[] particles;
    public float particleSize = .1f;
    public float penDistance = 0.2f;
    private Vector3 previousPosition = Vector3.zero; //camera starts from origin

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
        currentPS = Instantiate(painterParticlePrefab);
        currentPaintVertices = new List<Vector3>();
        paintSystems = new List<ParticleSystem>();
        frameUpdated = false;
        colorPicker.onValueChanged.AddListener(newColor => currentColor = newColor);
        colorPicker.gameObject.SetActive(false);
    }

    public void ARFrameUpdated(UnityARCamera camera)
    {
        var matrix = new Matrix4x4();
        matrix.SetColumn(3, camera.worldTransform.column3);

        var currentPositon = UnityARMatrixOps.GetPosition(matrix) + Camera.main.transform.forward * penDistance;
        if (Vector3.Distance(currentPositon, previousPosition) > minDistanceThreshold)
        {
            if (paintMode == 2) currentPaintVertices.Add(currentPositon);
            frameUpdated = true;
            previousPosition = currentPositon;
        }
    }

    private void OnGUI()
    {
        var modeString = paintMode == 0 ? "OFF" : paintMode == 1 ? "PICK" : "PAINT";
        if (GUI.Button(new Rect(Screen.width - 100.0f, 0.0f, 100.0f, 50.0f), modeString))
        {
            paintMode = (paintMode + 1) % 3;
            colorPicker.gameObject.SetActive(paintMode == 1);
            if (paintMode == 2)
                RestartPainting();
        }
    }

    private void RestartPainting()
    {
        paintSystems.Add(currentPS);
        currentPS = Instantiate(painterParticlePrefab);
        currentPaintVertices = new List<Vector3>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (frameUpdated && paintMode == 2)
        {
            if (currentPaintVertices.Count > 0)
            {
                var numParticles = currentPaintVertices.Count;
                var particles = new ParticleSystem.Particle[numParticles];
                var index = 0;
                foreach (var currentPoint in currentPaintVertices)
                {
                    particles[index].position = currentPoint;
                    particles[index].startColor = currentColor;
                    particles[index].startSize = particleSize;
                    index++;
                }

                currentPS.SetParticles(particles, numParticles);
            }
            else
            {
                var particles = new ParticleSystem.Particle[1];
                particles[0].startSize = 0.0f;
                currentPS.SetParticles(particles, 1);
            }

            frameUpdated = false;
        }
    }
}