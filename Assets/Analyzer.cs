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

    (GraphicsBuffer image, GraphicsBuffer lines, GraphicsBuffer total) _buffer;
    Material _material;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    void Start()
    {
        var dims = _source.OutputResolution;
        _buffer.image = NewBuffer(dims.x * dims.y);
        _buffer.lines = NewBuffer(256 * (_verticalScan ? dims.x : dims.y));
        _buffer.total = NewBuffer(256);
        _material = new Material(_outputShader);
        _outputView.material = _material;
    }

    void OnDestroy()
    {
        _buffer.image?.Dispose();
        _buffer.lines?.Dispose();
        _buffer.total?.Dispose();
        Destroy(_material);
    }

    void Update()
    {
        var src = _source.Texture;

        var (pass2, lineCount) =
          _verticalScan ? (2, src.width) : (1, src.height);

        _compute.SetInts("Dims", src.width, src.height);
        _compute.SetInts("LineCount", lineCount);

        _compute.SetTexture(0, "Source", src);
        _compute.SetBuffer(0, "ImageOut", _buffer.image);
        _compute.DispatchThreads(0, src.width, src.height, 1);

        _compute.SetBuffer(pass2, "ImageIn", _buffer.image);
        _compute.SetBuffer(pass2, "PerLineOut", _buffer.lines);
        _compute.DispatchThreads(pass2, lineCount, 1, 1);

        _compute.SetBuffer(3, "PerLineIn", _buffer.lines);
        _compute.SetBuffer(3, "TotalOut", _buffer.total);
        _compute.DispatchThreads(3, 256, 1, 1);

        _inputView.texture = src;
        _material.SetBuffer("_Histogram", _buffer.total);
    }
}
