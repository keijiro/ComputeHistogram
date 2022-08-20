using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;

sealed class Analyzer2 : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ComputeShader _compute = null;
    [SerializeField] RawImage _inputView = null;
    [SerializeField] RawImage _outputView = null;
    [SerializeField] Shader _outputShader = null;

    const int ThreadGroupSize = 32;
    const int BinCount = 128;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    (GraphicsBuffer temp, GraphicsBuffer total) _buffer;
    Material _viewMaterial;

    void Start()
    {
        var dims = _source.OutputResolution;

        _buffer.temp = NewBuffer(dims.y / ThreadGroupSize * BinCount);
        _buffer.total = NewBuffer(BinCount);

        _viewMaterial = new Material(_outputShader);
        _viewMaterial.SetBuffer("_Histogram", _buffer.total);
        _viewMaterial.SetInteger("_BinCount", BinCount);
        _viewMaterial.SetFloat("_VScale", 0.2f * BinCount / (dims.x * dims.y));
    }

    void OnDestroy()
    {
        _buffer.temp?.Dispose();
        _buffer.total?.Dispose();
        Destroy(_viewMaterial);
    }

    void Update()
    {
        var src = _source.Texture;
        _inputView.texture = src;
        _outputView.material = _viewMaterial;

        _compute.SetInts("Dims", src.width, src.height);
        _compute.SetTexture(0, "Source", src);
        _compute.SetBuffer(0, "TempOut", _buffer.temp);
        _compute.DispatchThreads(0, src.height, 1, 1);

        _compute.SetBuffer(1, "TempIn", _buffer.temp);
        _compute.SetBuffer(1, "TotalOut", _buffer.total);
        _compute.DispatchThreads(1, BinCount, 1, 1);
    }
}
