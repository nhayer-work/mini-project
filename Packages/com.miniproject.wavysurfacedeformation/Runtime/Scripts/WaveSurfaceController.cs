using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteInEditMode]
public class WaveSurfaceController : MonoBehaviour
{
    private const int MAX_SOURCES_COUNT = 80;
    private const float EPSILON = 0.0001f;
    class WaveSourceParam
    {
        public Vector3 Origin;
        public Vector3 Force;
        public Vector3 Velocity;
        public float ImpactTime;
        public float Mass;
        public float Radius;
        public float RadiusPower;
        public float Volume;
        public float BulkModulus { private get; set; }
        public float Damping { private get; set; }

        public bool IsDirty
        {
            get
            {
                if (_expirationTime < 0)
                {
                    // TODO: Calculate the time required to achieve an amplitude of zero
                    //var v = Mathf.Sqrt(BulkModulus / Mass);
                    //var epsi = Mathf.Exp(-Damping / (8 * Mass * v)) + EPSILON;
                    //_expirationTime = (1 + Mathf.Sqrt(1 + 8 * Mass * v * Mathf.Log(epsi) / Damping)) / (2 * v);
                    _expirationTime = Velocity.magnitude * 5f; // PLACEHOLDER
                }
                
                return Time.time - ImpactTime >= _expirationTime;
            }
        }

        private float _expirationTime = -1;
    }

    [SerializeField] private float _density = 9.97f;
    [SerializeField] private float _volume = 1f;
    [SerializeField] private float _bulkModulus = 22000;
    [SerializeField] private float _environmentDamping = 1f;
    [SerializeField] private bool _WaveTowardsNormal = false;
    
    private Material _surfaceMat;
    private List<WaveSourceParam> _activeSources = new List<WaveSourceParam>();
    private float[] _cleanableSources;

    private Coroutine _sourceControlRoutine;
    private Vector4[] _impactPoints = new Vector4[MAX_SOURCES_COUNT];
    private Vector4[] _impactForces = new Vector4[MAX_SOURCES_COUNT];
    private Vector4[] _impactVelocities = new Vector4[MAX_SOURCES_COUNT];
    private float[] _impactVolumes = new float[MAX_SOURCES_COUNT];
    private Vector4[] _impactParameters = new Vector4[MAX_SOURCES_COUNT];

    // Start is called before the first frame update
    void Start()
    {
        _surfaceMat = GetComponent<Renderer>().sharedMaterial;
        //_sourceControlRoutine = StartCoroutine(SourceControl());
    }

    private void OnEnable()
    {
        if(_surfaceMat == null)
            _surfaceMat = GetComponent<Renderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        _activeSources.RemoveAll(src => src.IsDirty);
        
        _surfaceMat.SetFloat("_Density", _density);
        _surfaceMat.SetFloat("_BulkModulus", _bulkModulus);
        _surfaceMat.SetFloat("_Damping", _environmentDamping);
        _surfaceMat.SetVector("_Gravity", Physics.gravity);
        _surfaceMat.SetFloat("_WaveTowardsNormal", _WaveTowardsNormal ? 1f : 0f);

        _surfaceMat.SetInt("_ImpactsCount", _activeSources.Count);
        for (int i = 0; i < Math.Min(_activeSources.Count, MAX_SOURCES_COUNT); i++)
        {
            _impactPoints[i] = _activeSources[i].Origin;
            _impactForces[i] = _activeSources[i].Force;
            _impactVelocities[i] = _activeSources[i].Velocity;
            _impactParameters[i] = new Vector4(_activeSources[i].ImpactTime, _activeSources[i].Mass, _activeSources[i].Radius, _activeSources[i].RadiusPower);
            _impactVolumes[i] = _activeSources[i].Volume;
        }
        
        _surfaceMat.SetVectorArray("_ImpactPoints", _impactPoints);
        _surfaceMat.SetVectorArray("_ImpactForces", _impactForces);
        _surfaceMat.SetVectorArray("_ImpactVelocities", _impactVelocities);
        _surfaceMat.SetFloatArray("_ImpactVolumes", _impactVolumes);
        _surfaceMat.SetVectorArray("_ImpactParameters", _impactParameters);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.TryGetComponent<WaveSource>(out var waveSource))
            return;

        if (_activeSources.Count >= MAX_SOURCES_COUNT)
            return;
        
        var size = collision.collider.bounds.size;
        WaveSourceParam sourceParam = new WaveSourceParam()
        {
            Origin = collision.GetContact(0).point,
            Force = collision.impulse,
            Velocity = collision.relativeVelocity,
            ImpactTime = Time.time,
            Mass = collision.rigidbody.mass,
            Radius = waveSource.Radius,
            RadiusPower = waveSource.RadiusPower,
            Volume = size.x * size.y * size.z,
            Damping = _environmentDamping,
            BulkModulus = _bulkModulus
        };

        _activeSources.Add(sourceParam);
    }
}