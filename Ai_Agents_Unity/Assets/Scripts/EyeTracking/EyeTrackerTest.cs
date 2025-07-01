using System.Collections;
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

        private void Awake()
        {
            m_Text = GetComponent<Text>();

            layerMask = ~(1 << ignoreEyetracking);
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
            /* if (!leftGaze.isValid ||!rightGaze.isValid
                 ||leftGazeTransform == null || rightGazeTransform == null)
             {
                 return;
             } */

            // This code is still needed to update the transform data for the SphereCast logic.
            // But since we disabled the Mesh Renderers, nothing will be visible.
            leftGazeTransform.position = Camera.main.transform.position;
            leftGazeTransform.rotation = Quaternion.Euler(XRRig.InverseTransformDirection(leftGaze.gazePose.orientation.ToUnityQuaternion().eulerAngles));
            rightGazeTransform.position = Camera.main.transform.position;
            rightGazeTransform.rotation = Quaternion.Euler(XRRig.InverseTransformDirection(rightGaze.gazePose.orientation.ToUnityQuaternion().eulerAngles));

            
            

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

            // REMOVED: All other text outputs (gaze position/rotation, geometric data, etc.) have been commented out or deleted.

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
            Vector3 gazeOrigin;
            Vector3 gazeDirection;

            bool isLeftValid = leftGaze.isValid;
            bool isRightValid = rightGaze.isValid;

            if (isLeftValid && isRightValid)
            {
                gazeOrigin = (leftGazeTransform.position + rightGazeTransform.position) / 2f;
                gazeDirection = (leftGazeTransform.forward + rightGazeTransform.forward).normalized;
            }
            else if (isLeftValid)
            {
                gazeOrigin = leftGazeTransform.position;
                gazeDirection = leftGazeTransform.forward;
            }
            else if (isRightValid)
            {
                gazeOrigin = rightGazeTransform.position;
                gazeDirection = rightGazeTransform.forward;
            }
            else
            {
                return;
            }

            RaycastHit[] hit =Physics.RaycastAll(gazeOrigin, gazeDirection);

            if (hit.Length > 0)
            {
                foreach (RaycastHit rcHit in hit) {
                    // CHANGED: Check if the hit object is one of our registered AOIs.
                    GameObject hitObject = rcHit.collider.gameObject;
                    if (gazeTimers.ContainsKey(hitObject))
                    {
                        // If it is, increment the timer for that specific object.
                        gazeTimers[hitObject] += Time.deltaTime;
                    }
                }
            }
        }
    }
}