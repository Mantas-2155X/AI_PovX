using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using AIChara;
using UnityEngine;

namespace AI_PovX
{
    [BepInPlugin(nameof(AI_PovX), nameof(AI_PovX), "1.0.0")]
    public partial class AI_PovX : BaseUnityPlugin
    {
        private static Camera mainCamera;
        private static Transform mainCameraTransform;
        
        private static ConfigEntry<float> ForwardOffset { get; set; }
        private static ConfigEntry<float> UpOffset { get; set; }
        private static ConfigEntry<float> RightOffset { get; set; }
        
        private static ConfigEntry<float> FOV { get; set; }
        private static ConfigEntry<float> MouseSens { get; set; }
        
        private static ConfigEntry<KeyboardShortcut> TogglePOVKey { get; set; }
        private static ConfigEntry<KeyboardShortcut> ChangeCharaKey { get; set; }

        private static Vector3 viewRotation;
        
        private static Vector3 backupPos;
        private static float backupFOV;
        
        private static List<ChaControl> availableCharas;
        private static ChaControl currentChara;

        private static bool inH;
        private static bool povEnabled;

        private void Awake()
        {
            ForwardOffset = Config.AddSetting("General", "ForwardOffset", 0f);
            UpOffset = Config.AddSetting("General", "UpOffset", 0.25f);
            RightOffset = Config.AddSetting("General", "RightOffset", 0f);
            
            FOV = Config.AddSetting("General", "FOV", 75f);
            MouseSens = Config.AddSetting("General", "MouseSens", 3f);
            
            TogglePOVKey = Config.AddSetting("Keyboard Shortcuts", "TogglePOV", new KeyboardShortcut(KeyCode.Comma));
            ChangeCharaKey = Config.AddSetting("Keyboard Shortcuts", "ChangeChara", new KeyboardShortcut(KeyCode.Period));
            
            HarmonyWrapper.PatchAll(typeof(AI_PovX));
        }

        private void Update()
        {
            if (!inH) 
                return;
            
            if (TogglePOVKey.Value.IsDown())
                TogglePOV();

            if (!povEnabled) 
                return;
            
            if (ChangeCharaKey.Value.IsDown())
                ChangeCharacter();
            
            if (currentChara == null) 
                return;

            if (!Input.GetMouseButton(0)) 
                return;
            
            var x = Input.GetAxis("Mouse X") * MouseSens.Value;
            var y = -Input.GetAxis("Mouse Y") * MouseSens.Value;

            viewRotation += new Vector3(y, x, 0f);
        }

        private static void ChangeCharacter()
        {
            int index = availableCharas.IndexOf(currentChara);
            
            currentChara = index >= availableCharas.Count - 1 ? availableCharas[0] : availableCharas[index + 1];
            SetViewRotation();
        }

        private static void SetViewRotation()
        {
            if (currentChara == null)
                return;
            
            var angles = currentChara.neckLookCtrl.neckLookScript.aBones[1].neckBone.eulerAngles;
            viewRotation = new Vector3(angles.x, angles.y, 0f);
        }
        
        private static void TogglePOV()
        {
            if (mainCamera == null)
                return;
            
            if (povEnabled)
            {
                mainCamera.fieldOfView = backupFOV;
                mainCameraTransform.position = backupPos;

                povEnabled = false;
                return;
            }
            
            backupFOV = mainCamera.fieldOfView;
            backupPos = mainCameraTransform.position;
            
            var hScene = Singleton<HScene>.Instance;
            var females = hScene.GetFemales();
            var males = hScene.GetMales();
            
            availableCharas = new List<ChaControl>();
            
            if(females != null)
                foreach(ChaControl female in females.Where(f => f != null && f.visibleAll))
                    availableCharas.Add(female);
        
            if(males != null)
                foreach(ChaControl male in males.Where(m => m != null && m.visibleAll))
                    availableCharas.Add(male);

            currentChara = availableCharas.Find(x => x != null && x.isPlayer) ?? availableCharas[0];
            SetViewRotation();

            povEnabled = true;
        }
    }
}