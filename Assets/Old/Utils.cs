using UnityEngine;

static class ComputeShaderExtensions
{
    public static (int x, int y, int z) GetKernelSizes
      (this ComputeShader compute, int kernel)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        return ((int)x, (int)y, (int)z);
    }

    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        var sizes = compute.GetKernelSizes(kernel);
        compute.Dispatch(kernel, (x + sizes.x - 1) / sizes.x,
                                 (y + sizes.y - 1) / sizes.y,
                                 (z + sizes.z - 1) / sizes.z);
    }
}
