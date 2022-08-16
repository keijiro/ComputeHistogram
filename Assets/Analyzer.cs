using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;

sealed class Analyzer : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ComputeShader _compute = null;
    [SerializeField] RawImage _inputView = null;
    [SerializeField] RawImage _outputView = null;
    [SerializeField] Shader _outputShader = null;

    GraphicsBuffer _buffer;
    Material _material;

    void Start()
    {
        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 256, 4);
        _material = new Material(_outputShader);
        _outputView.material = _material;
    }

    void OnDestroy()
    {
        _buffer?.Dispose();
        Destroy(_material);
    }

    void Update()
    {
        var tex = _source.Texture;

        _compute.SetTexture(0, "Source", tex);
        _compute.SetBuffer(0, "Output", _buffer);

        _compute.DispatchThreads(0, 1, 1, 1);

        _inputView.texture = tex;
        _material.SetBuffer("_Histogram", _buffer);
    }
}
