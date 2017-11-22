using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BentParticles : MonoBehaviour {

    //Number of particles
    #region Basic Properties

    [SerializeField]
    int _maxParticles = 100;

    public int maxParticles
    {
        get { return _maxParticles; }
        set { _maxParticles = value; }
    }

    #endregion

    //Center of emitter
    //"Throttle"
    #region Emitter Parameters

    [SerializeField]
    Vector3 _emitterCenter = Vector3.zero;

    public Vector3 emitterCenter
    {
        get { return _emitterCenter; }
        set { _emitterCenter = value; }
    }


    [SerializeField]
    Vector3 _emitterSize = Vector3.one;

    public Vector3 emitterSize
    {
        get { return _emitterSize; }
        set { _emitterSize = value; }
    }

    #endregion

    //Life time, random Life
    #region Particle Life Parameters

    [SerializeField]
    float _life = 4.0f;

    public float life
    {
        get { return _life; }
        set { _life = value; }
    }

    #endregion

    //Advanced Parameters That I'll wait on
    /*
     Velocity Parameters
     Acceleration Parameters
     Rotation Parameters
     Turbulent Noise Parameters
         */

    //Mesh data
    //Material Initialization
    //Shadows
    #region Render Settings

    [SerializeField]
    Mesh[] _shapes = new Mesh[1];


    [SerializeField]
    float _scale = 1.0f;


    public float scale
    {
        get { return _scale; }
        set { _scale = value; }
    }


    [SerializeField]
    Material _material;
    bool _owningMaterial; //whether or not is owning the material


    public Material sharedMaterial
    {
        get { return _material; }
        set { _material = value; }
    }


    public Material material
    {
        get
        {
            if (!_owningMaterial)
            {
                _material = Instantiate<Material>(_material);
                _owningMaterial = true;
            }
            return _material;
        }
        set
        {
            if (_owningMaterial) Destroy(_material, 0.1f);
            _material = value;
            _owningMaterial = false;
        }
    }

    [SerializeField]
    ShadowCastingMode _castShadows;

    public ShadowCastingMode shadowCastingMode
    {
        get { return _castShadows; }
        set { _castShadows = value; }
    }

    [SerializeField]
    bool _receiveShadows = false;

    public bool receiveShadows
    {
        get { return _receiveShadows; }
        set { _receiveShadows = value; }
    }

    #endregion

    //Skipping for now...
    #region Misc Settings
    #endregion


    //Hard coding resources into Script
    #region Built In Resources
        
    [SerializeField] Material _defaultMaterial;
    [SerializeField] Shader _kernelShader;
    [SerializeField] Shader _debugShader;

    #endregion

    //Variables and properties that will drive particles
    #region Private Variables and Properties 

    RenderTexture _positionBuffer1;
    RenderTexture _positionBuffer2;
    Material _kernelMaterial;
    Material _debugMaterial;
    MaterialPropertyBlock _props;
    bool _needsReset = true;

    static float deltaTime
    {
        get
        {
            var isEditor = !Application.isPlaying || Time.frameCount < 2;
            return isEditor ? 1.0f / 10 : Time.deltaTime;
        }
    }

    #endregion

    //Functions for updating the Kernal shader
    //UpdateKernal
    //Create buffers and Materials
    //Reset all Resources
    //Swapping buffers and management 
    #region Resource Management 
    
    public void NotifyConfigChange()
    {
        _needsReset = true;
    }

    Material CreateMaterial(Shader shader)
    {
        var material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        return material;
    }


    RenderTexture CreateBuffer()
    {
        var width = _maxParticles;
        var height = _maxParticles;
        var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        buffer.hideFlags = HideFlags.DontSave;
        buffer.filterMode = FilterMode.Point;
        buffer.wrapMode = TextureWrapMode.Repeat;
        return buffer;
    }

    void UpdateKernelShader()
    {
        var m = _kernelMaterial;

        m.SetVector("_EmitterPos", _emitterCenter);
        m.SetVector("_EmitterSize", _emitterSize);

    }

    void ResetResources()
    {
        if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
        if (_positionBuffer2) DestroyImmediate(_positionBuffer2);

        _positionBuffer1 = CreateBuffer();
        _positionBuffer2 = CreateBuffer();

        //Creation Kernel Material
        if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);

        InitializeAndPrewarmBuffers();

        _needsReset = false;
    }

    void InitializeAndPrewarmBuffers()
    {
        UpdateKernelShader();

        //Blit - copies source texture into desitnation render texture with a shader
        Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);

        for(var i = 0; i < 8; i++)
        {
            SwapAndInvokeKernels();
            UpdateKernelShader();
        }
    }

    void SwapAndInvokeKernels()
    {
        //Swap Buffers
        var tempPosition = _positionBuffer1;

        _positionBuffer1 = _positionBuffer2;

        _positionBuffer2 = tempPosition;

        //Invoke the position update Kernel
        _kernelMaterial.SetTexture("_PositionBuffer", _positionBuffer1);
        Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);

        //Animation Only // 
        //Invoke updated Kernel
        //With updateed position
        _kernelMaterial.SetTexture("_PositionBuffer", _positionBuffer2);


    }

    #endregion


    #region MonoBehaviour Functions

    void Reset()
    {
        _needsReset = true;
    }
	
    void OnDestroy()
    {
        if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
        if (_positionBuffer2) DestroyImmediate(_positionBuffer2);
        if (_kernelMaterial) DestroyImmediate(_kernelMaterial);
    }


	// Update is called once per frame
	void Update () {

        if (_needsReset) ResetResources();

        if (Application.isPlaying)
        {
            UpdateKernelShader();
            SwapAndInvokeKernels();
        }
        else
        {
            InitializeAndPrewarmBuffers();
        }

        //Make Material Property Block for following drawCalls
        if(_props == null)
        {
            _props = new MaterialPropertyBlock();
        }

        var props = _props;
        props.SetTexture("_PositionBuffer", _positionBuffer2);

        //Temp Variables
        var mesh = _shapes[0];
        var position = transform.position;
        var rotation = transform.rotation;
        var material = _material ? _material : _defaultMaterial;

        for (var i = 0; i < _positionBuffer2.height; i++)
        {
            Graphics.DrawMesh(mesh, position, rotation, material, 0, null, 0, props, _castShadows, _receiveShadows);
        }

	}

    #endregion
}
