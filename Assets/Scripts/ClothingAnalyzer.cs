using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PassthroughCameraSamples;
using TMPro;

[System.Serializable]
public class ClothingAnalysisResult
{
    // For laundry toss game
    public string RoomTidyStatus { get; set; } = "Unknown";
    
    // For folding game
    public Dictionary<string, int> ClothingCounts = new Dictionary<string, int>();
    
    public int TotalClothingCount => ClothingCounts.Values.Sum();
    
    public int GetCount(string clothingType) => 
        ClothingCounts.TryGetValue(clothingType, out int count) ? count : 0;
    
    public List<string> AllDetectedClothingTypes => ClothingCounts.Keys.ToList();
}

public class ClothingAnalyzer : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiKey = "YOUR_API_KEY_HERE";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    [Header("Analysis Mode")]
    public AnalyzeMode currentMode = AnalyzeMode.LiveCamera;
    public AnalysisType analysisType = AnalysisType.LaundryToss;
    public Texture2D testImage;

    [Header("Camera Setup")]
    public WebCamTextureManager webCamTextureManager;

  // --- NEW: TextMeshPro UI References ---
    [Header("UI Display")]
    public TextMeshProUGUI roomTidyStatusText; // For "Tidy" / "Untidy"
    public TextMeshProUGUI clothingDetailsText; // For "T-shirt: 2, Socks: 5" etc.
    // --- END NEW ---

    public ClothingAnalysisResult CurrentResult { get; private set; }
    public event Action<ClothingAnalysisResult> OnAnalysisComplete;

    public enum AnalyzeMode { LiveCamera, UseTestImage }
    public enum AnalysisType { 
        LaundryToss, // Just needs room status
        FoldingGame  // Needs detailed clothing counts
    }

  void Start()
    {
        // Subscribe to your own event when the script starts
        OnAnalysisComplete += DisplayResultsOnUI;
        StartCoroutine(InitializeAndAnalyze());
    }

    // --- NEW: Unsubscribe OnDisable to prevent memory leaks ---
    void OnDisable()
    {
        OnAnalysisComplete -= DisplayResultsOnUI;
    }
    // --- END NEW ---
    IEnumerator InitializeAndAnalyze()
    {
        Debug.Log("[UnifiedAnalyzer] Initializing...");

        if (currentMode == AnalyzeMode.LiveCamera)
        {
            while (webCamTextureManager == null || 
                   webCamTextureManager.WebCamTexture == null || 
                   !webCamTextureManager.WebCamTexture.isPlaying)
            {
                yield return null;
            }
        }
        else if (testImage == null)
        {
            Debug.LogError("[UnifiedAnalyzer] No test image assigned for UseTestImage mode");
            yield break;
        }
        
        Analyze();
    }

    public void Analyze()
    {
        StopAllCoroutines();
        StartCoroutine(currentMode == AnalyzeMode.LiveCamera ? 
            CaptureAndAnalyze() : 
            AnalyzeTexture(testImage));
    }

    IEnumerator AnalyzeTexture(Texture2D texture)
    {
        string json = CreateGeminiRequest(texture);
        yield return SendToGeminiAPI(json);
    }

    IEnumerator CaptureAndAnalyze()
    {
        Debug.Log("Analyzing passthrough camera");

        // Wait until webcam is properly initialized
        while (webCamTextureManager == null ||
               webCamTextureManager.WebCamTexture == null ||
               !webCamTextureManager.WebCamTexture.isPlaying)
        {
            yield return null;
        }

        WebCamTexture webCamTex = webCamTextureManager.WebCamTexture;

        // Create a new texture matching the webcam dimensions
        Texture2D capturedTexture = new Texture2D(webCamTex.width, webCamTex.height, TextureFormat.RGBA32, false);

        // Get the pixels from the webcam texture
        Color[] pixels = webCamTex.GetPixels();

        // Apply to our texture
        capturedTexture.SetPixels(pixels);
        capturedTexture.Apply();

        Debug.Log($"Captured texture: {webCamTex.width}x{webCamTex.height}");

        string json = CreateGeminiRequest(capturedTexture);
        yield return SendToGeminiAPI(json);

        // Clean up
        Destroy(capturedTexture);
    }


    string CreateGeminiRequest(Texture2D screenshot)
    {
        byte[] imageBytes = screenshot.EncodeToJPG(quality: 75);
        string base64Image = Convert.ToBase64String(imageBytes);

        string promptText = analysisType switch
        {
            AnalysisType.LaundryToss => 
                @"Analyze this room for laundry. Respond with:
                RoomTidyStatus: [Tidy/NOT SO TIDY]
                Then list any clothing items as: [ClothingType]: [Count]",
                
            AnalysisType.FoldingGame =>
                @"List all visible clothing items with counts. 
                Respond ONLY with lines in format: ""[ClothingType]: [Count]""",
                
            _ => throw new ArgumentOutOfRangeException()
        };

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
                OnAnalysisComplete?.Invoke(null);
                yield break;
            }

            Debug.Log("API Response: " + www.downloadHandler.text);
            ProcessAIResponse(www.downloadHandler.text);
            OnAnalysisComplete?.Invoke(CurrentResult);
        }
    }

    void ProcessAIResponse(string jsonResponse)
    {
        CurrentResult = new ClothingAnalysisResult();
        
        try
        {
            var response = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
            
            if (response?.candidates == null || response.candidates.Length == 0)
            {
                Debug.LogError("No candidates in response");
                return;
            }

            string content = response.candidates[0].content.parts[0].text;
            
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("Empty content in response");
                return;
            }

            Debug.Log("Extracted content:\n" + content);

            foreach (string line in content.Split('\n'))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Handle room status line (for LaundryToss)
                if (analysisType == AnalysisType.LaundryToss && 
                    trimmed.StartsWith("RoomTidyStatus:", StringComparison.OrdinalIgnoreCase))
                {
                    string status = trimmed.Substring("RoomTidyStatus:".Length).Trim();
                    CurrentResult.RoomTidyStatus = status;
                    continue;
                }

                // Handle clothing counts (for both modes)
                string[] parts = trimmed.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
                {
                    string clothingType = parts[0].Trim();
                    CurrentResult.ClothingCounts[clothingType] = count;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Response parsing failed: {e.Message}");
            CurrentResult = null;
        }
    }

    // --- NEW: Method to display results on UI ---
    void DisplayResultsOnUI(ClothingAnalysisResult result)
    {
        if (result == null)
        {
            if (roomTidyStatusText != null) roomTidyStatusText.text = "Analysis Failed";
            if (clothingDetailsText != null) clothingDetailsText.text = "";
            return;
        }

        if (roomTidyStatusText != null)
        {
            // For LaundryToss, display the tidy status
            if (analysisType == AnalysisType.LaundryToss)
            {
                roomTidyStatusText.text = result.RoomTidyStatus;
            }
            else // For FoldingGame, maybe show a generic status or total count here
            {
                roomTidyStatusText.text = $"Total Items: {result.TotalClothingCount}";
            }
        }

        if (clothingDetailsText != null)
        {
            StringBuilder sb = new StringBuilder();
            if (result.ClothingCounts.Count > 0)
            {
                foreach (var item in result.ClothingCounts)
                {
                    sb.AppendLine($"{item.Key}: {item.Value}");
                }
            }
            else
            {
                sb.AppendLine("No specific clothing items detected.");
            }
            clothingDetailsText.text = sb.ToString();
        }
        
        LogResults(); // Keep your debug log for convenience
    }
    // --- END NEW ---


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
        if (CurrentResult == null)
        {
            Debug.Log("No analysis results available");
            return;
        }

        Debug.Log("=== Analysis Results ===");

        if (analysisType == AnalysisType.LaundryToss)
        {
            Debug.Log($"Room Status: {CurrentResult.RoomTidyStatus}");
        }

        foreach (var item in CurrentResult.ClothingCounts)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }

        Debug.Log($"TOTAL: {CurrentResult.TotalClothingCount} items");
        Debug.Log("========================");
    }
}