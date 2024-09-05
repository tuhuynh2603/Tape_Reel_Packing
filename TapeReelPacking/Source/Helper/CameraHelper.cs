using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.Source.Model;

namespace TapeReelPacking.Source.Helper
{
    public static class CameraHelper
    {

        public static void LoadCamSetting(int nTrack, CameraParameter cameraParam)
        {
            if (nTrack != 0)
                return;
            #region USB Camera
            string strRecipePath = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
            string pathCam = Path.Combine(strRecipePath, "camera_Track" + (nTrack + 1).ToString() + ".cam");
            IniFile ini = new IniFile(pathCam);
            cameraParam.gain = (float)ini.ReadValue("Camera Setting", "gain", 5.0);
            cameraParam.exposureTime = (int)ini.ReadValue("Camera Setting", "exposure time", 10000);
            cameraParam.softwareTrigger = ini.ReadValue("Camera Setting", "software trigger", false);
            cameraParam.frameRate = (float)ini.ReadValue("Camera Setting", "FrameRate", 17);
            #endregion

            if (!Directory.Exists(strRecipePath))
            {
                Directory.CreateDirectory(strRecipePath);
                WriteCamSetting(nTrack, cameraParam);
            }
        }
        public static void WriteCamSetting(int nTrack, CameraParameter cameraParam)
        {
            #region USB Camera
            string pathCam = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
            string fullpathCam = Path.Combine(pathCam, "camera_Track" + (nTrack + 1).ToString() + ".cam");
            IniFile ini = new IniFile(fullpathCam);
            ini.WriteValue("Camera Setting", "gain", cameraParam.gain);
            ini.WriteValue("Camera Setting", "exposure time", cameraParam.exposureTime);
            ini.WriteValue("Camera Setting", "software trigger", cameraParam.softwareTrigger);
            ini.WriteValue("Camera Setting", "FrameRate", cameraParam.frameRate);
            #endregion
            if (!Directory.Exists(pathCam))
            {
                Directory.CreateDirectory(pathCam);
                WriteCamSetting(nTrack, cameraParam);
            }


        }

    }
}
