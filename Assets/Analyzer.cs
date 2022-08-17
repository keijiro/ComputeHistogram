using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;

sealed class Analyzer : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ComputeShader _compute = null;
    [SerializeField] bool _verticalScan = false;
    [SerializeField] RawImage _inputView = null;
    [SerializeField] RawImage _outputView = null;
    [SerializeField] Shader _outputShader = null;

    (GraphicsBuffer lines, GraphicsBuffer total) _buffers;
    Material _material;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    void Start()
    {
        var dims = _source.OutputResolution;
        _buffers.lines = NewBuffer(256 * (_verticalScan ? dims.x : dims.y));
        _buffers.total = NewBuffer(256);
        _material = new Material(_outputShader);
        _outputView.material = _material;
    }

    void OnDestroy()
    {
        _buffers.lines?.Dispose();
        _buffers.total?.Dispose();
        Destroy(_material);
    }

    void Update()
    {
        var src = _source.Texture;

        var (pass, lineCount) =
          _verticalScan ? (1, src.width) : (0, src.height);

        _compute.SetInts("Dims", src.width, src.height);
        _compute.SetTexture(pass, "Source", src);
        _compute.SetBuffer(pass, "PerLineOut", _buffers.lines);
        _compute.DispatchThreads(pass, lineCount, 1, 1);

        _compute.SetInts("LineCount", lineCount);
        _compute.SetBuffer(2, "PerLineIn", _buffers.lines);
        _compute.SetBuffer(2, "TotalOut", _buffers.total);
        _compute.DispatchThreads(2, 256, 1, 1);

        _inputView.texture = src;
        _material.SetBuffer("_Histogram", _buffers.total);
    }
}
