using HarmonyLib;
using AIChara;
using UnityEngine;

namespace AI_PovX
{
    public partial class AI_PovX
    {
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice_SetupPOV()
        {
            if (Camera.main == null)
                return;

            mainCamera = Camera.main;
            mainCameraTransform = mainCamera.transform;
            
            inH = true;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "Update")]
        public static void HScene_Update_DisablePOV(HScene __instance)
        {
            if (!inH && !povEnabled) 
                return;

            if (__instance.ctrlFlag.click != HSceneFlagCtrl.ClickKind.SceneEnd)
                return;
            
            if (povEnabled)
                TogglePOV();
            
            inH = false;
            currentChara = null;
            availableCharas = null;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
        public static bool NeckLookControllerVer2_LateUpdate_ControlPOVNeck(NeckLookControllerVer2 __instance)
        {
            if (!povEnabled || !inH || currentChara == null || mainCamera == null)
                return true;

            var comp = __instance.neckLookScript.gameObject.GetComponent<Correct.Process.IKBeforeOfDankonProcess>();
            if (comp == null || comp.enabled == false)
                return true;

            ChaControl currentNeckCha = comp.chaCtrl;
            if(currentNeckCha == null || currentNeckCha != currentChara) 
                return true;

            __instance.neckLookScript.aBones[1].neckBone.rotation = Quaternion.identity;
            __instance.neckLookScript.aBones[1].neckBone.Rotate(viewRotation);
            
            currentChara.objHead.SetActive(false);
            foreach (var obj in currentChara.objHair)
                if(obj != null)
                    obj.SetActive(false);

            var camNeckBone = __instance.neckLookScript.aBones[0].neckBone;
            var eyeObjs = currentChara.eyeLookCtrl.eyeLookScript.eyeObjs;
            
            mainCameraTransform.position = Vector3.Lerp(eyeObjs[0].eyeTransform.position, eyeObjs[1].eyeTransform.position, 0.5f);
            mainCameraTransform.rotation = currentChara.objHeadBone.transform.rotation;
            
            mainCameraTransform.Translate(camNeckBone.forward * ForwardOffset.Value);
            mainCameraTransform.Translate(camNeckBone.up * UpOffset.Value);
            mainCameraTransform.Translate(camNeckBone.right * RightOffset.Value);
            
            mainCamera.fieldOfView = FOV.Value;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
        public static bool CameraControl_Ver2_LateUpdate_StopNormalCameraData()
        {
            return !povEnabled || !inH || currentChara == null || mainCamera == null;
        }
    }
}