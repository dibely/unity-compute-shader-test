using UnityEngine;

public class SimplePolygon : MonoBehaviour {

    // Vertex definition.  Must match equivalent struct definition in the compute shader.
    private struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    private const int VERTEX_COUNT = 4;
    private const int THREAD_COUNT_X = 2; // Must match numthreads parameter in the compute shader
    private const int THREAD_COUNT_Y = 2; // Must match numthreads parameter in the compute shader
    private const int THREAD_GROUP_X = VERTEX_COUNT / (THREAD_COUNT_X * THREAD_COUNT_Y);
    private const int THREAD_GROUP_Y = 1;

    public ComputeShader computeShader;

    private ComputeBuffer vertexBuffer;

    private void Awake() {
        BuildVertexData();
    }

    private void BuildVertexData() {
        // The array size is the total number of threads that will be executed.  
        // The value comes from the parameters for the Dispatch call multiplied with the numthreads specified in the compute shader. 
        int arraySize = THREAD_GROUP_X * THREAD_COUNT_X * THREAD_GROUP_Y * THREAD_COUNT_Y;

        Vertex[] vertexArray = new Vertex[arraySize];
        vertexBuffer = new ComputeBuffer(vertexArray.Length, sizeof(float)*8, ComputeBufferType.Default);
        vertexBuffer.SetData(vertexArray);

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetInt("DispatchSizeX", THREAD_GROUP_X);
        computeShader.SetInt("DispatchSizeY", THREAD_GROUP_Y);

        computeShader.SetBuffer(kernelHandle, "VertexBuffer", vertexBuffer);
        computeShader.Dispatch(kernelHandle, THREAD_GROUP_X, THREAD_GROUP_Y, 1);

        vertexBuffer.GetData(vertexArray);
        vertexBuffer.Dispose();

        foreach (Vertex data in vertexArray) {
            Debug.Log("Vertex data:");
            Debug.Log(data.position);
            Debug.Log(data.normal);
            Debug.Log(data.uv);
        }
    }
}
