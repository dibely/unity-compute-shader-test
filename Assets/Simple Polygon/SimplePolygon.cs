using UnityEngine;

public class SimplePolygon : MonoBehaviour {

    // Vertex definition.  Must match equivalent struct definition in the compute shader.
    private struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    private const int VERTEX_COUNT = 6;
    private const int THREAD_COUNT_X = 1; // Must match numthreads parameter in the compute shader
    private const int THREAD_COUNT_Y = 1; // Must match numthreads parameter in the compute shader
    private const int THREAD_GROUP_X = 1;
    private const int THREAD_GROUP_Y = 1;

    public ComputeShader computeShader;

    private ComputeBuffer vertexBuffer;
    private ComputeBuffer argBuffer;
    public Material material;

    private void Awake() {
        BuildVertexData();

        // Setup the material
        material.SetBuffer("vertexBuffer", vertexBuffer);
    }

    private void BuildVertexData() {
        // The array size is the total number of threads that will be executed.  
        // The value comes from the parameters for the Dispatch call multiplied with the numthreads specified in the compute shader. 
        int arraySize = THREAD_GROUP_X * THREAD_COUNT_X * THREAD_GROUP_Y * THREAD_COUNT_Y * VERTEX_COUNT;
        Debug.Log("Array Size: " + arraySize);

        Vertex[] vertexArray = new Vertex[arraySize];
        vertexBuffer = new ComputeBuffer(vertexArray.Length, sizeof(float)*8, ComputeBufferType.Append);
        vertexBuffer.SetCounterValue(0);
        vertexBuffer.SetData(vertexArray);

        argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        int[] args = new int[] { 0, 1, 0, 0 };
        argBuffer.SetData(args);

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetInt("DispatchSizeX", THREAD_GROUP_X);
        computeShader.SetInt("DispatchSizeY", THREAD_GROUP_Y);
        computeShader.SetFloat("Size", 2.0f);

        computeShader.SetBuffer(kernelHandle, "VertexBuffer", vertexBuffer);
        computeShader.Dispatch(kernelHandle, THREAD_GROUP_X, THREAD_GROUP_Y, 1);

        // DEBUG
        vertexBuffer.GetData(vertexArray);
        foreach (Vertex data in vertexArray) {
            Debug.Log("Vertex data:");
            Debug.Log(data.position);
            Debug.Log(data.normal);
            Debug.Log(data.uv);
        }

        ComputeBuffer.CopyCount(vertexBuffer, argBuffer, 0);

        // DEBUG
        argBuffer.GetData(args);
        Debug.Log("Vertex count: " + args[0]);
        Debug.Log("Instance count: " + args[1]);
        Debug.Log("Start vertex: " + args[2]);
        Debug.Log("Start instance: " + args[3]);
    }

    private void OnRenderObject() {
        if(material == null) {
            Debug.LogError("Material is invalid");
            return;
        }

        if(vertexBuffer == null) {
            return;
        }

        if(vertexBuffer.count == 0) {
            Debug.LogWarning("No verteices in buffer");
            return;
        }
        //Debug.Log("vertexBuffer count: " + vertexBuffer.count);
        material.SetPass(0);
        material.SetBuffer("vertexBuffer", vertexBuffer);
        Graphics.DrawProceduralIndirect(MeshTopology.Triangles, argBuffer, 0);
    }

    private void OnDestroy() {
        vertexBuffer.Release();
        argBuffer.Release();
    }
}
