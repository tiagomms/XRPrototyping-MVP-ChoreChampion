using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PassthroughCameraSamples;

[System.Serializable]
public class ClothingAnalysisResult
{
    public Dictionary<string, int> summaryCounts = new Dictionary<string, int>();

    public int TotalCount => summaryCounts.Values.Sum();

    public int GetCount(string clothingType) => 
        summaryCounts.TryGetValue(clothingType, out int count) ? count : 0;

    public List<string> AllDetectedClothingTypesAsList => 
        summaryCounts.SelectMany(entry => Enumerable.Repeat(entry.Key, entry.Value)).ToList();
}

public class ClothingAnalyzer : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiKey = "YOUR_API_KEY_HERE"; // Replace with your actual API key
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    [Header("Analysis Mode")]
    public AnalyzeMode currentAnalyzeMode = AnalyzeMode.LiveCamera;
    public Texture2D testImage;

    [Header("Camera Setup")]
    public WebCamTextureManager webCamTextureManager;

    public ClothingAnalysisResult CurrentAnalysis { get; private set; }

    public enum AnalyzeMode { LiveCamera, UseTestImage }

    void Start() => StartCoroutine(InitializeAndAnalyze());

    IEnumerator InitializeAndAnalyze()
    {
        Debug.Log("[ClothingAnalyzer] Initializing...");

        if (currentAnalyzeMode == AnalyzeMode.LiveCamera)
        {
            while (webCamTextureManager == null || webCamTextureManager.WebCamTexture == null || 
                  !webCamTextureManager.WebCamTexture.isPlaying)
            {
                yield return null;
            }
        }
        else if (testImage == null)
        {
            Debug.LogError("[ClothingAnalyzer] No test image assigned for UseTestImage mode");
            yield break;
        }
        
        AnalyzeClothing();
    }

    public void AnalyzeClothing()
    {
        StopAllCoroutines();
        
        switch (currentAnalyzeMode)
        {
            case AnalyzeMode.UseTestImage:
                StartCoroutine(AnalyzeTexture(testImage));
                break;
            case AnalyzeMode.LiveCamera:
                StartCoroutine(CaptureAndAnalyze());
                break;
        }
    }

    IEnumerator AnalyzeTexture(Texture2D texture)
    {
        string json = CreateGeminiRequest(texture);
        yield return SendToGeminiAPI(json);
    }

    IEnumerator CaptureAndAnalyze()
    {
        WebCamTexture passthroughTexture = webCamTextureManager.WebCamTexture;
        Texture2D screenshot = new Texture2D(passthroughTexture.width, passthroughTexture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(passthroughTexture, screenshot);

        string json = CreateGeminiRequest(screenshot);
        yield return SendToGeminiAPI(json);

        Destroy(screenshot);
    }

    string CreateGeminiRequest(Texture2D screenshot)
    {
        byte[] imageBytes = screenshot.EncodeToJPG(quality: 75);
        string base64Image = Convert.ToBase64String(imageBytes);

        string promptText = @"Analyze this image and list all visible clothing items with their counts.
Respond ONLY with lines in format: ""[ClothingType]: [Count]""
Example:
T-Shirt: 2
Jeans: 1
Sneakers: 1
Do NOT include any other text or explanations.";

        // Manually construct the JSON string to match the working format
        string json = 
        @"{
            ""contents"": [{
                ""parts"": [{
                    ""text"": """ + promptText.Replace("\"", "\\\"") + @"""
                },{
                    ""inline_data"": {
                        ""mime_type"": ""image/jpeg"",
                        ""data"": """ + base64Image + @"""
                    }
                }]
            }]
        }";

        Debug.Log("Request JSON: " + json);
        return json;
    }

    IEnumerator SendToGeminiAPI(string json)
    {
        string url = $"{apiUrl}?key={apiKey}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Error: {www.error}\nResponse: {www.downloadHandler.text}");
                yield break;
            }

            Debug.Log("API Response: " + www.downloadHandler.text);
            ProcessAIResponse(www.downloadHandler.text);
            LogResults();
        }
    }

  void ProcessAIResponse(string jsonResponse)
{
    CurrentAnalysis = new ClothingAnalysisResult();
    
    try
    {
        // Parse the JSON response
        var response = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
        
        if (response?.candidates == null || response.candidates.Length == 0)
        {
            Debug.LogError("No candidates in response");
            return;
        }

        // Get the text content from the first candidate
        string content = response.candidates[0].content.parts[0].text;
        
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("Empty content in response");
            return;
        }

        Debug.Log("Extracted content:\n" + content);

        // Parse each line of the response
        foreach (string line in content.Split('\n'))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            string[] parts = trimmed.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
            {
                string clothingType = parts[0].Trim();
                CurrentAnalysis.summaryCounts[clothingType] = count;
                Debug.Log($"Found clothing: {clothingType} - {count}");
            }
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"Response parsing failed: {e.Message}");
    }
}

// Add these classes for JSON parsing
[System.Serializable]
private class GeminiResponse
{
    public Candidate[] candidates;
    public UsageMetadata usageMetadata;
    public string modelVersion;
    public string responseId;
}

[System.Serializable]
private class Candidate
{
    public Content content;
    public string finishReason;
    public float avgLogprobs;
}

[System.Serializable]
private class Content
{
    public Part[] parts;
    public string role;
}

[System.Serializable]
private class Part
{
    public string text;
}

[System.Serializable]
private class UsageMetadata
{
    public int promptTokenCount;
    public int candidatesTokenCount;
    public int totalTokenCount;
    public TokenDetail[] promptTokensDetails;
    public TokenDetail[] candidatesTokensDetails;
}

[System.Serializable]
private class TokenDetail
{
    public string modality;
    public int tokenCount;
}
    void LogResults()
    {
        Debug.Log("=== Clothing Analysis Results ===");
        foreach (var item in CurrentAnalysis.summaryCounts)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
        Debug.Log($"TOTAL: {CurrentAnalysis.TotalCount} items");
        Debug.Log("================================");
    }
}