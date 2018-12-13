using UnityEngine;

public class SimpleMesh : MonoBehaviour {

    // Vertex definition.  Must match equivalent struct definition in the compute shader.
    private struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    private const int VERTEX_COUNT = 4;
    private const int THREAD_COUNT_X = 2; // Must match numthreads parameter in the compute shader
    private const int THREAD_COUNT_Y = 2; // Must match numthreads parameter in the compute shader
    private const int THREAD_GROUP_X = 1;
    private const int THREAD_GROUP_Y = 1;

    public ComputeShader computeShader;

    private ComputeBuffer vertexBuffer;
    private MeshFilter meshFilter;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        BuildVertexData();
    }

    private void BuildVertexData() {
        // The array size is the total number of threads that will be executed.  
        // The value comes from the parameters for the Dispatch call multiplied with the numthreads specified in the compute shader. 
        int arraySize = THREAD_GROUP_X * THREAD_COUNT_X * THREAD_GROUP_Y * THREAD_COUNT_Y;
        Debug.Log("Array Size: " + arraySize);

        Vertex[] vertexArray = new Vertex[arraySize];
        vertexBuffer = new ComputeBuffer(vertexArray.Length, sizeof(float) * 8, ComputeBufferType.Default);
        vertexBuffer.SetCounterValue(0);
        vertexBuffer.SetData(vertexArray);

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetInt("DispatchSizeX", THREAD_GROUP_X);
        computeShader.SetInt("DispatchSizeY", THREAD_GROUP_Y);
        computeShader.SetFloat("Size", 5.0f);

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

        BuildMesh(vertexArray);
        vertexBuffer.Release();
    }

    private void BuildMesh(Vertex[] vertexData) {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[vertexData.Length];
        int[] indices = new int[vertexData.Length];
        Vector3[] normals = new Vector3[vertexData.Length];
        Vector2[] uvs = new Vector2[vertexData.Length];

        int index = 0;
        foreach(Vertex vertex in vertexData) {
            vertices[index] = vertex.position;
            normals[index] = vertex.normal;
            uvs[index] = vertex.uv;
            indices[index] = index;

            ++index;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }
}
