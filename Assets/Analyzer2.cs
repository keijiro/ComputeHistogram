using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;

sealed class Analyzer2 : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] RawImage _inputView = null;
    [SerializeField] RawImage _outputView = null;
    [SerializeField] Shader _outputShader = null;

    [SerializeField, HideInInspector] ComputeShader _compute = null;

    const int ScanThreadCount = 32 * 512;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    (GraphicsBuffer image, GraphicsBuffer count, GraphicsBuffer total) _buffer;
    Material _viewMaterial;

    void Start()
    {
        var dims = _source.OutputResolution;
        _buffer.image = NewBuffer(dims.x * dims.y);
        _buffer.count = NewBuffer(256 * ScanThreadCount);
        _buffer.total = NewBuffer(256);
        _viewMaterial = new Material(_outputShader);
        _outputView.material = _viewMaterial;
    }

    void OnDestroy()
    {
        _buffer.image?.Dispose();
        _buffer.count?.Dispose();
        _buffer.total?.Dispose();
        Destroy(_viewMaterial);
    }

    void Update()
    {
        var src = _source.Texture;

        _compute.SetInt("ScanThreads", ScanThreadCount);
        _compute.SetInt("ScanLength", src.width * src.height / ScanThreadCount);

        _compute.SetTexture(0, "Source", src);
        _compute.SetBuffer(0, "ImageOut", _buffer.image);
        _compute.DispatchThreads(0, src.width, src.height, 1);

        _compute.SetBuffer(1, "ImageIn", _buffer.image);
        _compute.SetBuffer(1, "CountOut", _buffer.count);
        _compute.DispatchThreads(1, ScanThreadCount, 1, 1);

        _compute.SetBuffer(2, "CountIn", _buffer.count);
        _compute.SetBuffer(2, "TotalOut", _buffer.total);
        _compute.DispatchThreads(2, 256, 1, 1);

        _inputView.texture = src;
        _viewMaterial.SetBuffer("_Histogram", _buffer.total);
    }
}
