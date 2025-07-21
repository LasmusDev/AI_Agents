﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.EyeTracker;

namespace VIVE.OpenXR.Samples.EyeTracker
{
    public class EyeTrackerTest : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Samples.EyeTracker.EyeTrackerText";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        public Transform leftGazeTransform = null;
        public Transform rightGazeTransform = null;
        public Transform XRRig;

        // CHANGED: From a single GameObject to a List to hold all our AOIs.
        public List<GameObject> areasOfInterest = new List<GameObject>();

        private Text m_Text = null;

        // CHANGED: From a single float to a Dictionary to store a timer for each AOI.
        private Dictionary<GameObject, float> gazeTimers;

        public float gazeDetectionRadius = 0.1f;
        private int layerMask;
        public LayerMask ignoreEyetracking;

        // NEW: Gaze visualizer variables
        public GameObject gazePointerPrefab; // Assign a small sphere or crosshair prefab here
                                            // IMPORTANT: This prefab should NOT have a Collider or Rigidbody component.
                                            // It is purely for visual representation.
        private GameObject currentGazePointer; // To hold the instantiated gaze pointer

        // NEW: Boolean to control gaze pointer visibility
        private bool isGazePointerActive = true; // Default to inactive

        private void Awake()
        {
            m_Text = GetComponent<Text>();

            // Ensure layerMask correctly filters out ignored layers
            layerMask = ~ignoreEyetracking.value; // Correct way to invert a LayerMask
        }

        void Start()
        {
            // NEW: Initialize the dictionary.
            gazeTimers = new Dictionary<GameObject, float>();
            if (areasOfInterest.Count > 0)
            {
                // Populate the dictionary with all specified AOIs, each starting at 0 seconds.
                foreach (var aoi in areasOfInterest)
                {
                    if (aoi != null && !gazeTimers.ContainsKey(aoi))
                    {
                        gazeTimers.Add(aoi, 0f);
                    }
                }
            }
            else
            {
                Debug.LogError(LOG_TAG + " No Areas of Interest have been assigned in the Inspector!");
            }

            // NEW: Instantiate the gaze pointer at the start
            if (gazePointerPrefab != null)
            {
                currentGazePointer = Instantiate(gazePointerPrefab);
                // Set initial active state based on isGazePointerActive
                currentGazePointer.SetActive(isGazePointerActive); 
            }
            else
            {
                Debug.LogWarning(LOG_TAG + " Gaze Pointer Prefab is not assigned. Gaze visualization will not work.");
            }
        }

        private XrSingleEyeGazeDataHTC leftGaze;
        private XrSingleEyeGazeDataHTC rightGaze;

        void FixedUpdate()
        {
            // --- Cleaned up UI Text ---
            m_Text.text = ""; // Start with a clean slate
            
            XR_HTC_eye_tracker.Interop.GetEyeGazeData(out XrSingleEyeGazeDataHTC[] out_gazes);

            leftGaze = out_gazes[(int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC];
            rightGaze = out_gazes[(int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC];
            /* Removed the early return here to ensure gaze transforms are always updated
             * and gaze data is displayed, even if transforms are null or gaze is invalid.
             * The gaze pointer and AOI logic will handle invalid gaze data.
             */

            // This code is still needed to update the transform data for the SphereCast logic.
            // But since we disabled the Mesh Renderers, nothing will be visible.
            // Ensure Camera.main is not null before accessing its transform
        
            if (Camera.main != null)
            {
                // The gaze origin is always the camera's position
                leftGazeTransform.position = Camera.main.transform.position;
                rightGazeTransform.position = Camera.main.transform.position;

                // leftGazeTransform.rotation = Quaternion.Euler(XRRig.InverseTransformDirection(leftGaze.gazePose.orientation.ToUnityQuaternion().eulerAngles));
                // rightGazeTransform.rotation = Quaternion.Euler(XRRig.InverseTransformDirection(rightGaze.gazePose.orientation.ToUnityQuaternion().eulerAngles));

            }
            else
            {
                Debug.LogWarning(LOG_TAG + " Main Camera not found. Gaze transforms may not update correctly.");
                // If Camera.main is null, we can't get accurate gaze origin, so deactivate pointer
                if (currentGazePointer != null)
                {
                    currentGazePointer.SetActive(false);
                }
                m_Text.text += "Main Camera not found!\n";
                return; // Exit FixedUpdate if no main camera
            }
            
            // --- Display Requested Data ---

            m_Text.text += "[Eye Status]\n";
            m_Text.text += "Left Gaze Valid: " + leftGaze.isValid + "\n";
            m_Text.text += "Right Gaze Valid: " + rightGaze.isValid + "\n\n";

            XR_HTC_eye_tracker.Interop.GetEyePupilData(out XrSingleEyePupilDataHTC[] out_pupils);
            XrSingleEyePupilDataHTC leftPupil = out_pupils[(int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC];
            XrSingleEyePupilDataHTC rightPupil = out_pupils[(int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC];

            m_Text.text += "[Pupil Dilation]\n";
            m_Text.text += "Left: " + leftPupil.pupilDiameter.ToString("F4") + "mm\n";
            m_Text.text += "Right: " + rightPupil.pupilDiameter.ToString("F4") + "mm\n\n";

            CheckGazeOnTarget();

            // NEW: Display the gaze time for each AOI.
            m_Text.text += "[AOI Focus Time]\n";
            if (gazeTimers != null)
            {
                foreach (var aoiTimerPair in gazeTimers)
                {
                    // Display the name of the AOI and its formatted timer.
                    m_Text.text += aoiTimerPair.Key.name + ": " + aoiTimerPair.Value.ToString("F2") + "s\n";
                }
            }
        }

        private void CheckGazeOnTarget()
        {
            Vector3 gazeDirection = Vector3.zero;

            bool isLeftValid = leftGaze.isValid;
            bool isRightValid = rightGaze.isValid;

            if (isLeftValid && isRightValid)
            {
                // gazeOrigin = (leftGazeTransform.position + rightGazeTransform.position) / 2f;
                // gazeDirection = (leftGazeTransform.forward + rightGazeTransform.forward).normalized;
                
                Quaternion combinedRotation = Quaternion.Slerp(
                    leftGaze.gazePose.orientation.ToUnityQuaternion(),
                    rightGaze.gazePose.orientation.ToUnityQuaternion(),
                    0.5f
                );
                gazeDirection = combinedRotation * Vector3.forward;
            }
            else if (isLeftValid)
            {
                // gazeDirection = leftGazeTransform.forward;
                gazeDirection = leftGaze.gazePose.orientation.ToUnityQuaternion() * Vector3.forward;

            }
            else if (isRightValid)
            {
                // gazeDirection = rightGazeTransform.forward;
                gazeDirection = rightGaze.gazePose.orientation.ToUnityQuaternion() * Vector3.forward;

            }
            else
            {
                // Deactivate gaze pointer if no valid gaze data or if it's meant to be inactive
                if (currentGazePointer != null && currentGazePointer.activeSelf) // Only deactivate if currently active
                {
                    currentGazePointer.SetActive(false);
                }
                return;
            }
            Vector3 gazeOrigin = Camera.main.transform.position;


            // --- Debugging Aid: Draw the ray in the Scene view ---
            // This will help you visualize where the eye gaze ray is going.
            // You'll see a blue line in the Unity Scene view when running.
            Debug.DrawRay(gazeOrigin, gazeDirection * 10f, Color.blue);


            // Reverted to RaycastAll to get all hits along the ray
            RaycastHit[] hits = Physics.RaycastAll(gazeOrigin, gazeDirection, Mathf.Infinity, layerMask);

            // Sort hits by distance to ensure the gaze pointer is placed at the closest hit
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

            // bool hitSomething = false;
            if (hits.Length > 0)
            {
                // Position the gaze pointer at the first (closest) hit
                if (currentGazePointer != null && isGazePointerActive) // Only update position if it should be active
                {
                    currentGazePointer.transform.position = hits[0].point;
                    // currentGazePointer.transform.forward = -hits[0].normal; // Orient towards the camera
                    currentGazePointer.SetActive(true); // Ensure it's active if it hits something and isGazePointerActive is true
                }
                // hitSomething = true;

                foreach (RaycastHit rcHit in hits)
                {
                    // Check if the hit object is one of our registered AOIs.
                    GameObject hitObject = rcHit.collider.gameObject;
                    if (gazeTimers.ContainsKey(hitObject))
                    {
                        // If it is, increment the timer for that specific object.
                        gazeTimers[hitObject] += Time.deltaTime;
                    }
                }
            }
            else
            {
                // Deactivate gaze pointer if no hit or if it's meant to be inactive
                if (currentGazePointer != null && currentGazePointer.activeSelf) // Only deactivate if currently active
                {
                    currentGazePointer.SetActive(false);
                }
            }
        }

        // NEW PUBLIC METHOD: Call this from a UI Button to toggle gaze pointer visibility
        public void ToggleGazePointerVisibility()
        {
            isGazePointerActive = !isGazePointerActive; // Flip the boolean state
            if (currentGazePointer != null)
            {
                currentGazePointer.SetActive(isGazePointerActive); // Apply the new state
            }
            DEBUG("Gaze Pointer visibility toggled to: " + isGazePointerActive);
        }
    }
}