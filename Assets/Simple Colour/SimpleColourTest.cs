using UnityEngine;

public class SimpleColourTest : MonoBehaviour {
    private const int TEXTURE_SIZE = 256;
    public ComputeShader computeShader;
    private RenderTexture renderTexture;

    private void Awake() {
        CreateRenderTexture();

        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer.material;
        material.SetTexture("_MainTex", renderTexture);
        UpdateTextureFromComputeShader();
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

    void Update() {

    }

    private void UpdateTextureFromComputeShader() {
        int kernelHandle = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        computeShader.Dispatch(kernelHandle, TEXTURE_SIZE / 8, TEXTURE_SIZE / 8, 1);
    }
}