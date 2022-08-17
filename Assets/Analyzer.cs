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

    (GraphicsBuffer rows, GraphicsBuffer total) _buffers;
    Material _material;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    void Start()
    {
        _buffers.rows = NewBuffer(256 * _source.OutputResolution.y);
        _buffers.total = NewBuffer(256);
        _material = new Material(_outputShader);
        _outputView.material = _material;
    }

    void OnDestroy()
    {
        _buffers.rows?.Dispose();
        _buffers.total?.Dispose();
        Destroy(_material);
    }

    void Update()
    {
        var src = _source.Texture;

        _compute.SetInts("Dims", src.width, src.height);

        _compute.SetTexture(0, "Source", src);
        _compute.SetBuffer(0, "PerRowOut", _buffers.rows);
        _compute.DispatchThreads(0, src.height, 1, 1);

        _compute.SetBuffer(1, "PerRowIn", _buffers.rows);
        _compute.SetBuffer(1, "TotalOut", _buffers.total);
        _compute.DispatchThreads(1, 256, 1, 1);

        _inputView.texture = src;
        _material.SetBuffer("_Histogram", _buffers.total);
    }
}
