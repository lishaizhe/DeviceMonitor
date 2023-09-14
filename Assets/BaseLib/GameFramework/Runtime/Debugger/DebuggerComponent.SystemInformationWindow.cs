//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class SystemInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>System Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Device Unique ID:", SystemInfo.deviceUniqueIdentifier);
                    DrawItem("Device Name:", SystemInfo.deviceName);
                    DrawItem("Device Type:", SystemInfo.deviceType.ToString());
                    DrawItem("Device Model:", SystemInfo.deviceModel);
                    DrawItem("Processor Type:", SystemInfo.processorType);
                    DrawItem("Processor Count:", SystemInfo.processorCount.ToString());
                    DrawItem("Processor Frequency:", string.Format("{0} MHz", SystemInfo.processorFrequency.ToString()));
                    DrawItem("Memory Size:", string.Format("{0} MB", SystemInfo.systemMemorySize.ToString()));
#if UNITY_5_5_OR_NEWER
                    DrawItem("Operating System Family:", SystemInfo.operatingSystemFamily.ToString());
#endif
                    DrawItem("Operating System:", SystemInfo.operatingSystem);

                }
                GUILayout.EndVertical();


                GUILayout.Label("<b>Graphics Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Device ID:", SystemInfo.graphicsDeviceID.ToString());
                    DrawItem("Device Name:", SystemInfo.graphicsDeviceName);
                    DrawItem("Device Vendor ID:", SystemInfo.graphicsDeviceVendorID.ToString());
                    DrawItem("Device Vendor:", SystemInfo.graphicsDeviceVendor);
                    DrawItem("Device Type:", SystemInfo.graphicsDeviceType.ToString());
                    DrawItem("Device Version:", SystemInfo.graphicsDeviceVersion);
                    DrawItem("Memory Size:", string.Format("{0} MB", SystemInfo.graphicsMemorySize.ToString()));
                    DrawItem("Multi Threaded:", SystemInfo.graphicsMultiThreaded.ToString());
                    DrawItem("Shader Level:", GetShaderLevelString(SystemInfo.graphicsShaderLevel));
                    DrawItem("NPOT Support:", SystemInfo.npotSupport.ToString());
                    DrawItem("Max Texture Size:", SystemInfo.maxTextureSize.ToString());
#if UNITY_5_4_OR_NEWER
                    DrawItem("Copy Texture Support:", SystemInfo.copyTextureSupport.ToString());
#endif
                    DrawItem("Supported Render Target Count:", SystemInfo.supportedRenderTargetCount.ToString());
                }
                GUILayout.EndVertical();
            }

            private string GetShaderLevelString(int shaderLevel)
            {
                return string.Format("Shader Model {0}.{1}", (shaderLevel / 10).ToString(), (shaderLevel % 10).ToString());
            }

            private string GetBatteryLevelString(float batteryLevel)
            {
                if (batteryLevel < 0f)
                {
                    return "Unavailable";
                }

                return batteryLevel.ToString("P0");
            }
        }
    }
}
