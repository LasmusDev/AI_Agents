using System;
using UnityEngine;
using VIVE.OpenXR;

namespace EyeTracking{
    
    public class ActiveVisionFoveation : MonoBehaviour
    {
        [SerializeField] VIVE.OpenXR.Foveation.XrFoveationModeHTC UsingMode;
        VIVE.OpenXR.Foveation.XrFoveationConfigurationHTC[] Configs = new VIVE.OpenXR.Foveation.XrFoveationConfigurationHTC[2];
        void Start()
        {
    
            bool FoveationIsDynamic_bit01 = true;
            bool FoveationIsDynamic_bit02 = true;
            bool FoveationIsDynamic_bit04 = true;
    
            /////////////////////Setting for left eye/////////////////////////////
            Configs[0].level = VIVE.OpenXR.Foveation.XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC; //
            Configs[0].clearFovDegree = 0;                                      //
            Configs[0].focalCenterOffset.x = 0.0f;                              // 
            Configs[0].focalCenterOffset.y = 0.0f;                              //
                                                                                //////////////////////////////////////////////////////////////////////
    
            ////////////////////Setting for right eye/////////////////////////////
            Configs[1].level = VIVE.OpenXR.Foveation.XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC; //
            Configs[1].clearFovDegree = 0;                                      //
            Configs[1].focalCenterOffset.x = 0.0f;                              //
            Configs[1].focalCenterOffset.y = 0.0f;                              //
                                                                                //////////////////////////////////////////////////////////////////////
    
            UInt64 flags = (FoveationIsDynamic_bit01 ? ViveFoveation.XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC : 0x00) |
                    (FoveationIsDynamic_bit02 ? ViveFoveation.XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC : 0x00) |
                    (FoveationIsDynamic_bit04 ? ViveFoveation.XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC : 0x00);
    
    
            switch (UsingMode)
            {
                //XR_FOVEATION_MODE_FIXED_HTC: The position of foveation is fixed
                case VIVE.OpenXR.Foveation.XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC:
                    ViveFoveation.ApplyFoveationHTC(VIVE.OpenXR.Foveation.XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC, 0, null);
                    break;
                //XR_FOVEATION_MODE_DYNAMIC_HTC: the position of foveation can be adjust
                case VIVE.OpenXR.Foveation.XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC:
                    ViveFoveation.ApplyFoveationHTC(VIVE.OpenXR.Foveation.XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC, 0, null, flags);
                    break;
                //XR_FOVEATION_MODE_CUSTOM_HTC: the foveation will use the custom setting
                case VIVE.OpenXR.Foveation.XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC:
                    ViveFoveation.ApplyFoveationHTC(UsingMode, 2, Configs);
                    break;
            }
        }
    }
    
}
