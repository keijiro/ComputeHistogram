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

    const int ThreadGroupSize = 32;
    const int BinCount = 256;

    GraphicsBuffer NewBuffer(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, 4);

    (GraphicsBuffer temp, GraphicsBuffer total) _buffer;
    Material _viewMaterial;

    void Start()
    {
        var dims = _source.OutputResolution;

        _buffer.temp = NewBuffer(dims.x / ThreadGroupSize * BinCount);
        _buffer.total = NewBuffer(BinCount);

        _viewMaterial = new Material(_outputShader);
        _viewMaterial.SetBuffer("_Histogram", _buffer.total);
        _viewMaterial.SetInteger("_BinCount", BinCount);
        _viewMaterial.SetFloat("_VScale", 3.0f * BinCount / (dims.x * dims.y));
        _outputView.material = _viewMaterial;
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

        _compute.SetInts("Dims", src.width, src.height);
        _compute.SetTexture(0, "Source", src);
        _compute.SetBuffer(0, "TempOut", _buffer.temp);
        _compute.DispatchThreads(0, src.width, 1, 1);

        _compute.SetBuffer(1, "TempIn", _buffer.temp);
        _compute.SetBuffer(1, "TotalOut", _buffer.total);
        _compute.DispatchThreads(1, BinCount, 1, 1);
    }
}
