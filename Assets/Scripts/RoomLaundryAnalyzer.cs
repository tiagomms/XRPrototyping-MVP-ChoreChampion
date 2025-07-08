using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PassthroughCameraSamples;

[System.Serializable]
public class RoomLaundryAnalysisResult
{
    public string RoomTidyStatus { get; set; } = "Unknown";
    public Dictionary<string, int> ClothingCounts = new Dictionary<string, int>();

    public int TotalClothingCount => ClothingCounts.Values.Sum();

    public string GetClothingCount(string clothingType) =>
        ClothingCounts.TryGetValue(clothingType, out int count) ? count.ToString() : "0";

    public List<string> GetDetectedClothingTypes() => ClothingCounts.Keys.ToList();
}

public class RoomLaundryAnalyzer : MonoBehaviour
{
    [Header("Gemini API Configuration")]
    public string apiKey = "YOUR_API_KEY_HERE";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    [Header("Image Source")]
    public Texture2D testImage;
    public WebCamTextureManager webCamTextureManager;

    public event Action<RoomLaundryAnalysisResult> OnAnalysisComplete;

    public RoomLaundryAnalysisResult CurrentAnalysisResult { get; private set; }

    void Start()
    {
        if (testImage != null)
        {
            AnalyzeRoom(testImage);
        }
        else if (webCamTextureManager != null)
        {
            StartCoroutine(InitializeAndAnalyzeLiveCamera());
        }
    }

    // Coroutine to wait for the webcam to be ready (if using live camera)
    IEnumerator InitializeAndAnalyzeLiveCamera()
    {
        if (webCamTextureManager == null)
        {
            Debug.LogError("[RoomLaundryAnalyzer] WebCamTextureManager not assigned for live camera analysis.");
            yield break;
        }

        Debug.Log("[RoomLaundryAnalyzer] Waiting for webcam texture...");
        while (webCamTextureManager.WebCamTexture == null || !webCamTextureManager.WebCamTexture.isPlaying)
        {
            yield return null;
        }
        Debug.Log("[RoomLaundryAnalyzer] Webcam texture ready.");
    }


    public void AnalyzeRoom(Texture2D texture)
    {
        StopAllCoroutines();
        StartCoroutine(AnalyzeTextureCoroutine(texture));
    }

    // Public method to capture and analyze from the WebCamTextureManager
    public void AnalyzeRoomFromWebcam()
    {
        if (webCamTextureManager == null || webCamTextureManager.WebCamTexture == null || !webCamTextureManager.WebCamTexture.isPlaying)
        {
            Debug.LogError("[RoomLaundryAnalyzer] Webcam is not ready or WebCamTextureManager is not assigned.");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(CaptureAndAnalyzeWebcamCoroutine(webCamTextureManager.WebCamTexture));
    }

    IEnumerator AnalyzeTextureCoroutine(Texture2D texture)
    {
        string json = CreateGeminiRequest(texture);
        yield return SendToGeminiAPI(json);
    }

    IEnumerator CaptureAndAnalyzeWebcamCoroutine(WebCamTexture passthroughTexture)
    {
        Texture2D screenshot = new Texture2D(passthroughTexture.width, passthroughTexture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(passthroughTexture, screenshot); // Copies the content directly

        string json = CreateGeminiRequest(screenshot);
        yield return SendToGeminiAPI(json);

        Destroy(screenshot); // Clean up the screenshot texture
    }

    string CreateGeminiRequest(Texture2D screenshot)
    {
        byte[] imageBytes = screenshot.EncodeToJPG(quality: 75);
        string base64Image = Convert.ToBase64String(imageBytes);

        // This is the prompt for Gemini to get room tidy status and clothing counts
        string promptText = @"Analyze this room for the presence and state of laundry.
        
        First, determine if the room appears untidy primarily due to scattered or unorganized laundry. Respond with 'RoomTidyStatus: Tidy' or 'RoomTidyStatus: Untidy' on a new line.

        Second, list all visible clothing items with their counts. If there are multiple items of the same type, count them. If no clothing is found, state 'No clothing found.'.
        
        Respond ONLY with the RoomTidyStatus line, followed by lines in the format: ""[ClothingType]: [Count]""
        ";

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

        Debug.Log("[RoomLaundryAnalyzer] Request JSON: " + json);
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
                Debug.LogError($"[RoomLaundryAnalyzer] API Error: {www.error}\nResponse: {www.downloadHandler.text}");
                CurrentAnalysisResult = null; // Clear result on error
                OnAnalysisComplete?.Invoke(null); // Notify subscribers of error
                yield break;
            }

            Debug.Log("[RoomLaundryAnalyzer] API Response: " + www.downloadHandler.text);
            ProcessAIResponse(www.downloadHandler.text);
            // Notify any subscribers that the analysis is complete
            OnAnalysisComplete?.Invoke(CurrentAnalysisResult);
        }
    }

    void ProcessAIResponse(string jsonResponse)
    {
        CurrentAnalysisResult = new RoomLaundryAnalysisResult(); // Initialize new result object
        CurrentAnalysisResult.RoomTidyStatus = "Unknown"; // Default value
        
        try
        {
            // Parse the JSON response (using your existing helper classes)
            var response = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
            
            if (response?.candidates == null || response.candidates.Length == 0)
            {
                Debug.LogError("[RoomLaundryAnalyzer] No candidates in response from Gemini.");
                return;
            }

            string content = response.candidates[0].content.parts[0].text;
            
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("[RoomLaundryAnalyzer] Empty content in response from Gemini.");
                return;
            }

            Debug.Log("[RoomLaundryAnalyzer] Extracted content:\n" + content);

            // Parse each line of the response
            foreach (string line in content.Split('\n'))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed.StartsWith("RoomTidyStatus:", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = trimmed.Split(':');
                    if (parts.Length == 2)
                    {
                        CurrentAnalysisResult.RoomTidyStatus = parts[1].Trim();
                    }
                }
                else if (trimmed.Equals("No clothing found.", StringComparison.OrdinalIgnoreCase))
                {
                    // No clothing, so counts remain empty/zero
                }
                else // Assume it's a clothing item line
                {
                    string[] parts = trimmed.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
                    {
                        string clothingType = parts[0].Trim();
                        CurrentAnalysisResult.ClothingCounts[clothingType] = count;
                    }
                    else
                    {
                         Debug.LogWarning($"[RoomLaundryAnalyzer] Could not parse line: {trimmed}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoomLaundryAnalyzer] Response parsing failed: {e.Message}");
            CurrentAnalysisResult = null; // Mark result as invalid on parsing error
        }
    }

    // Keep your JSON parsing helper classes here or in a separate file if preferred
    [System.Serializable] private class GeminiResponse { public Candidate[] candidates; public UsageMetadata usageMetadata; public string modelVersion; public string responseId; }
    [System.Serializable] private class Candidate { public Content content; public string finishReason; public float avgLogprobs; }
    [System.Serializable] private class Content { public Part[] parts; public string role; }
    [System.Serializable] private class Part { public string text; }
    [System.Serializable] private class UsageMetadata { public int promptTokenCount; public int candidatesTokenCount; public int totalTokenCount; public TokenDetail[] promptTokensDetails; public TokenDetail[] candidatesTokensDetails; }
    [System.Serializable] private class TokenDetail { public string modality; public int tokenCount; }
}