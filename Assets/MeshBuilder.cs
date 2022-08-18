using UnityEngine;
using UnityEngine.Rendering;

sealed class MeshBuilder : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ComputeShader _compute = null;
    [SerializeField] Vector2Int _resolution = new Vector2Int(512, 512);

    #endregion

    #region Pirvate objects

    Mesh _mesh;
    (GraphicsBuffer v, GraphicsBuffer i) _buffer;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        InitializeMesh();
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
        _mesh = null;

        _buffer.v?.Dispose();
        _buffer.i?.Dispose();
        _buffer = (null, null);
    }

    void Update()
      => BuildMesh();

    #endregion

    #region Private utility properties

    int VertexCount => _resolution.x * _resolution.y;
    int IndexCount => (_resolution.x - 1) * (_resolution.y - 1) * 2 * 3;

    #endregion

    #region Pirvate methods

    void InitializeMesh()
    {
        // Mesh object
        _mesh = new Mesh();
        _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
          (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
          (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        _mesh.SetVertexBufferParams(VertexCount, vp, vn);
        _mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount),
                         MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references
        _buffer.v = _mesh.GetVertexBuffer(0);
        _buffer.i = _mesh.GetIndexBuffer();

        // Big bounds (no culling)
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    void BuildMesh()
    {
        _compute.SetInts("Dims", _resolution.x, _resolution.y);
        _compute.SetFloat("Time", Time.time);

        _compute.SetBuffer(0, "Vertices", _buffer.v);
        _compute.DispatchThreads(0, _resolution.x, _resolution.y, 1);

        _compute.SetBuffer(1, "Indices", _buffer.i);
        _compute.DispatchThreads(1, _resolution.x - 1, _resolution.y - 1, 1);
    }

    #endregion
}
