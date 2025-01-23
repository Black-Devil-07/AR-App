using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI; // For UI display

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceMultipleObjectsOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject placedPrefab;

    [SerializeField]
    RawImage displayImage;  // UI RawImage to display captured image (optional)

    ARRaycastManager aRRaycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    Camera arCamera;
    Texture2D capturedImage;

    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;  // Use the AR camera for capturing images

        // Initially hide the RawImage
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check if there is existing touch
        if (Input.touchCount == 0)
            return;

        // Store the current touch input
        Touch touch = Input.GetTouch(0);

        // Check if the touch input just touched the screen
        if (touch.phase == TouchPhase.Began)
        {
            // Check if the ray cast hit any trackable
            if (aRRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                // Ray cast hits are sorted by distance, so the first hit means the closest.
                var hitPose = hits[0].pose;

                // Instantiate the prefab.
                GameObject spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);

                // Assign a random color
                AssignRandomColor(spawnedObject);

                // Start coroutine to capture image after 10 frames
                StartCoroutine(CaptureImageAfterDelay());
            }
        }
    }

    void AssignRandomColor(GameObject obj)
    {
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null)
        {
            objRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }

    IEnumerator CaptureImageAfterDelay()
    {
        // Hide the RawImage UI temporarily
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(false);
        }

        // Wait for 10 frames (1 frame = 1/60 seconds assuming 60fps)
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }

        // Capture image from AR camera (excluding UI elements)
        CaptureARImage();

        // Optionally display the image in UI
        if (displayImage != null && capturedImage != null)
        {
            // Show the RawImage only after the image has been captured
            displayImage.gameObject.SetActive(true);
            displayImage.texture = capturedImage;
        }

        // Optionally, save the image to file (for example, save to persistent data path)
        SaveImageToFile();
    }

    void CaptureARImage()
    {
        // Create a texture with the same resolution as the camera view
        int width = Screen.width;
        int height = Screen.height;

        // Capture the AR camera's view (not using screen shots, but reading the pixels from the AR camera)
        capturedImage = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Get AR Camera's current view (Capture only what the AR camera is seeing)
        RenderTexture renderTexture = arCamera.targetTexture;  // Assuming the AR Camera has a RenderTexture attached
        RenderTexture.active = renderTexture;
        capturedImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedImage.Apply();
        RenderTexture.active = null;  // Reset the active texture
    }

    void SaveImageToFile()
    {
        if (capturedImage != null)
        {
            // Convert the image to bytes (PNG)
            byte[] imageBytes = capturedImage.EncodeToPNG();

            // Save it to the persistent data path (you can customize the path if needed)
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "CapturedImage.png");
            System.IO.File.WriteAllBytes(filePath, imageBytes);

            Debug.Log("Image saved to: " + filePath);
        }
    }
}
