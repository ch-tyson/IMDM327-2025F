using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    private const float G = 6.67430e-11f;
    public BodyProperty[] bp;
    GameObject[] planets;
    Vector3[] real_position;
    DataCSV dataLoader;
    private bool initialized = false;

    public struct BodyProperty
    {
        public float mass;
        public float distance;
        public float initial_velocity;
        public Vector3 velocity;
        public Vector3 acceleration;
    }

    void Start()
    {
        Camera.main.transform.position = new Vector3(0, 0, -200);
        dataLoader = gameObject.AddComponent<DataCSV>();
    }

    void Update()
    {
        if (!initialized && dataLoader.bp != null && dataLoader.bp.Length > 0)
        {
            InitializeBodies();
            initialized = true;
        }

        if (!initialized) return;

        for (int i = 0; i < bp.Length; i++)
        {
            bp[i].acceleration = Vector3.zero;
        }

        for (int i = 0; i < bp.Length; i++)
        {
            for (int j = 0; j < bp.Length; j++)
            {
                if (i == j) continue;

                Vector3 temp = real_position[j] - real_position[i];
                Vector3 force = CalculateGravity(temp, bp[i].mass, bp[j].mass);
                bp[i].acceleration += force / bp[i].mass;
            }
        }

        for (int i = 0; i < bp.Length; i++)
        {
            bp[i].velocity += bp[i].acceleration * 5000f;
            real_position[i] += bp[i].velocity * 5000f;

            float r = real_position[i].magnitude;
            float scaledDistance = Mathf.Sqrt(r / 1e8f);
            Vector3 dir = (r > 0f) ? (real_position[i] / r) : Vector3.zero;
            planets[i].transform.position = dir * scaledDistance;
        }
    }

    void InitializeBodies()
    {
        int n = dataLoader.bp.Length;
        bp = new BodyProperty[n];
        planets = new GameObject[n];
        real_position = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            bp[i].mass = dataLoader.bp[i].mass;
            bp[i].distance = dataLoader.bp[i].distance;
            bp[i].initial_velocity = dataLoader.bp[i].initial_velocity;
            bp[i].acceleration = Vector3.zero;

            planets[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planets[i].transform.localScale = new Vector3(5, 5, 5);
            planets[i].GetComponent<Renderer>().material.color = GetPlanetColor(i);

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            real_position[i] = radial * bp[i].distance;

            Vector3 tangential = new Vector3(-radial.y, radial.x, 0f);
            float vmag = (i == 0) ? 0f : bp[i].initial_velocity;
            bp[i].velocity = tangential.normalized * vmag;

            float r = real_position[i].magnitude;
            float scaledDistance = Mathf.Sqrt(r / 1e8f);
            Vector3 dir = (r > 0f) ? (real_position[i] / r) : Vector3.zero;
            planets[i].transform.position = dir * scaledDistance;

            TrailRenderer trailRenderer = planets[i].AddComponent<TrailRenderer>();
            trailRenderer.time = 100.0f;
            trailRenderer.startWidth = 1f;
            trailRenderer.endWidth = 0.1f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0.0f),
                    new GradientColorKey(GetPlanetColor(i), 0.80f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trailRenderer.colorGradient = gradient;
        }
    }

    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        float r = distanceVector.magnitude;
        if (r <= 0f) return Vector3.zero;

        Vector3 force = distanceVector.normalized * (G * m1 * m2 / (r * r));
        return force;
    }

    Color GetPlanetColor(int index)
    {
        Color[] colors = {
            Color.yellow,              // Sun
            Color.gray,                // Mercury
            new Color(1f, 0.8f, 0f),   // Venus
            Color.blue,                // Earth
            Color.red,                 // Mars
            new Color(1f, 0.6f, 0.2f), // Jupiter
            new Color(1f, 0.9f, 0.5f), // Saturn
            Color.cyan,                // Uranus
            Color.blue,                // Neptune
            Color.gray                 // Pluto
        };
        return colors[index];
    }
}