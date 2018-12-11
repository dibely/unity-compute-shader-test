using UnityEngine;

public class AverageColour : MonoBehaviour {
    private const int TEXTURE_SIZE = 32;
    private const int THREAD_GROUP_X = TEXTURE_SIZE / 8;
    private const int THREAD_GROUP_Y = TEXTURE_SIZE / 8;
    private const int THREAD_COUNT_X = 4;
    private const int THREAD_COUNT_Y = 4;

    public ComputeShader computeShader;
    public Texture2D sourceTexture;
    private RenderTexture renderTexture;

    private ComputeBuffer floatBuffer;
    public Vector3 average = new Vector3();

    private void Awake() {
        CreateRenderTexture();

        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer.material;
        material.SetTexture("_MainTex", renderTexture);

        CalculateAverage();
    }

    private void CreateRenderTexture() {
        renderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    private void OnDestroy() {
        renderTexture.Release();
        renderTexture = null;
    }

    private void CalculateAverage() {
        // The array size is the total number of threads that will be executed.  
        // The value comes from the parameters for the Dispatch call multiplied with the numthreads specified in the compute shader. 
        int arraySize = THREAD_GROUP_X * THREAD_COUNT_X * THREAD_GROUP_Y * THREAD_COUNT_Y;
        floatBuffer = new ComputeBuffer(arraySize, sizeof(float)*3, ComputeBufferType.Default);

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "RenderTexture", renderTexture);
        computeShader.SetTexture(kernelHandle, "SourceTexture", sourceTexture);
        computeShader.SetBuffer(kernelHandle, "FloatBuffer", floatBuffer);
        computeShader.Dispatch(kernelHandle, THREAD_GROUP_X, THREAD_GROUP_Y, 1);

        Vector3[] result = new Vector3[arraySize];
        floatBuffer.GetData(result);
        floatBuffer.Dispose();

        // Calculate the final average from the result data
        average = new Vector3(0, 0, 0);
        foreach (Vector3 data in result) {
            Debug.Log(data);
            average += data;
        }

        Debug.Log("Average: " + average/result.Length);
    }
}
