using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Jumping;
using OpenSkiJumping.New;
using OpenSkiJumping.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

namespace OpenSkiJumping.ScriptableObjects
{
    public class SkiJumperDataController : MonoBehaviour
    {
        public Competitor competitor;
        public RuntimeCompetitorsList competitors;
        public Material helmetMaterial;
        public JumperController2 jumperController;
        public CompetitionRunner competitionRunner;
        public JumperModel jumperFemale;
        public JumperModel jumperMale;
        public RuntimeResultsManager resultsManager;
        public Material bibMaterial;
        public Material skisMaterial;
        public Material suitBottomBackMaterial;
        public Material suitBottomFrontMaterial;
        public Material suitTopBackMaterial;
        public Material suitTopFrontMaterial;
        public GameObject helmetObject;
        public GameObject customHelmetObject;
        public GameObject leftSkiObject;
        public GameObject rightSkiObject;
        public GameObject customLeftSkiObject;
        public GameObject customRightSkiObject;
        public GameObject leftSkiCloneObject;
        public GameObject rightSkiCloneObject;
        public GameObject customLeftSkiCloneObject;
        public GameObject customRightSkiCloneObject;
        public Renderer customHelmetRenderer;
        public Renderer customLeftSkiRenderer;
        public Renderer customRightSkiRenderer;
        public Renderer customLeftSkiCloneRenderer;
        public Renderer customRightSkiCloneRenderer;
        public Material transparentHelmetMaterial; // For fallback if texture is missing
        public float mipMapBiasHelmet = -1f;
        public float mipMapBiasSkis = -1f;
        public bool hasCustomSkiTexture = false;
        private static readonly int Color = Shader.PropertyToID("_BaseColor");

        public void GetValues()
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            competitor = competitors.competitors[id];
            //Debug.Log("Od SkiJumperDataController competitor.normalskill: " + competitor.normalHillSkill + "id: " + id + "normalHillSKill: " + competitor.normalHillSkill);
        }

        public int GetNormalHillSkill()
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            competitor = competitors.competitors[id];
            return competitor.normalHillSkill;
        }

        public int GetControl()
        {
            var id = resultsManager.Value.GetCurrentJumperId(); 
            competitor = competitors.competitors[id];
            return (int)competitor.control;
        }


        public int GetSkill(float hillsize)
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            competitor = competitors.competitors[id];
            if (hillsize < 115)
            {
                return competitor.normalHillSkill;
            }
            if (hillsize >= 115 && hillsize < 160)
            {
                return competitor.largeHillSkill;
            }
            return competitor.skiFlyingHillSkill;
                    
        }

        private void LoadHelmetTexture()
        {

            string textureName = competitor.helmetTexture;

            Debug.Log($"LoadHelmetTexture before return");

            if (string.IsNullOrEmpty(textureName))
            {
                UseDefaultHelmet();
                return;
            }
            Debug.Log($"LoadHelmetTexture after return");

            // Just build the full path and try to load it; do NOT use File.Exists
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "textures", "helmet", textureName);
            StartCoroutine(LoadCustomHelmetTextureCoroutine(fullPath));
        }


        private void UseDefaultHelmet()
        {
            helmetObject.SetActive(true);
            customHelmetObject.SetActive(false);
        }

        private IEnumerator LoadCustomHelmetTextureCoroutine(string filePath)
        {
            Debug.Log($"Trying to load helmet texture from: {filePath}");
            string uri = new System.Uri(filePath).AbsoluteUri;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Failed to load helmet texture: " + www.error);
                    UseDefaultHelmet();
                    yield break;
                }

                Texture2D rawTex = DownloadHandlerTexture.GetContent(www);
                Texture2D tex = new Texture2D(rawTex.width, rawTex.height, rawTex.format, true);
                tex.SetPixels(rawTex.GetPixels());
                tex.Apply(true); // generate mipmaps

                tex.mipMapBias = mipMapBiasHelmet;

                // Assign to customHelmetRenderer
                Material[] mats = customHelmetRenderer.materials;
                if (mats.Length >= 1)
                {
                    Material mat = new Material(mats[0]); // duplicate to avoid global edits
                    mat.mainTexture = tex;
                    mat.mainTexture.mipMapBias = mipMapBiasHelmet;
                    mats[0] = mat;

                    customHelmetRenderer.materials = mats;

                    customHelmetObject.SetActive(true);
                    helmetObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Custom helmet renderer has no material slots!");
                    UseDefaultHelmet();
                }
            }
        }

        private void LoadSkisTexture()
        {
            string textureName = competitor.skiTexture;

            Debug.Log($"SkiTexture before return");

            if (string.IsNullOrEmpty(textureName))
            {
                UseDefaultSkis();
                return;
            }
            Debug.Log($"SkiTexture after return");

            // Just build the full path and try to load it; do NOT use File.Exists
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "textures", "skis", textureName);
            StartCoroutine(LoadCustomSkisTextureCoroutine(fullPath));
        }

        private void UseDefaultSkis()
        {
            leftSkiObject.SetActive(true);
            rightSkiObject.SetActive(true);
            leftSkiCloneObject.SetActive(true);
            rightSkiCloneObject.SetActive(true);
            customLeftSkiObject.SetActive(false);
            customRightSkiObject.SetActive(false);
            customLeftSkiCloneObject.SetActive(false);
            customRightSkiCloneObject.SetActive(false);
        }



        private IEnumerator LoadCustomSkisTextureCoroutine(string filePath)
        {
            Debug.Log($"Trying to load skis texture from: {filePath}");
            string uri = new System.Uri(filePath).AbsoluteUri;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Failed to load skis texture: " + www.error);
                    UseDefaultHelmet();
                    hasCustomSkiTexture = false;
                    yield break;
                }

                hasCustomSkiTexture = true;
                Texture2D rawTex = DownloadHandlerTexture.GetContent(www);
                Texture2D tex = new Texture2D(rawTex.width, rawTex.height, rawTex.format, true);
                tex.SetPixels(rawTex.GetPixels());
                tex.Apply(true); // generate mipmaps

                tex.mipMapBias = mipMapBiasSkis;

                // Assign to customHelmetRenderer
                Material[] matsLeft = customLeftSkiRenderer.materials;
                Material[] matsRight = customRightSkiRenderer.materials;
                Material[] matsCloneLeft = customLeftSkiCloneRenderer.materials;
                Material[] matsCloneRight = customRightSkiCloneRenderer.materials;
                if (matsLeft.Length >= 1)
                {
                    Material matl = new Material(matsLeft[1]); // duplicate to avoid global edits
                    //Material matr = new Material(matsRight[1]);
                    matl.mainTexture = tex;
                   // matr.mainTexture = tex;
                    matl.mainTexture.mipMapBias = mipMapBiasSkis;
                    //matr.mainTexture.mipMapBias = mipMapBiasSkis;
                    matsLeft[1] = matl;
                    matsRight[1] = matl;
                    matsCloneLeft[1] = matl;
                    matsCloneRight[1] = matl;
                    customLeftSkiRenderer.materials = matsLeft;
                    customRightSkiRenderer.materials = matsLeft;
                    customLeftSkiCloneRenderer.materials = matsLeft;
                    customRightSkiCloneRenderer.materials = matsCloneRight;
                    customLeftSkiObject.SetActive(true);
                    customRightSkiObject.SetActive(true);
                    leftSkiObject.SetActive(false);
                    rightSkiObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Custom helmet renderer has no material slots!");
                    UseDefaultHelmet();
                }
            }
        }

        public void ApplySkiCloneVisuals()
        {
            if (hasCustomSkiTexture)
            {

                // Use textured ski clones
                customLeftSkiCloneObject.SetActive(true);
                customRightSkiCloneObject.SetActive(true);
                leftSkiCloneObject.SetActive(false);
               rightSkiCloneObject.SetActive(false);
            
                }
            else
            {
                // Use default-colored ski clones
                customLeftSkiCloneObject.SetActive(false);
                customRightSkiCloneObject.SetActive(false);
                leftSkiCloneObject.SetActive(true);
                rightSkiCloneObject.SetActive(true);
            }
        }


        public void SetValues(Color bibColor)
        {
            jumperMale.gameObject.SetActive(competitor.gender == Gender.Male);
            jumperFemale.gameObject.SetActive(competitor.gender == Gender.Female);
            jumperController.jumperModel = (competitor.gender == Gender.Male ? jumperMale : jumperFemale);
            bibMaterial.SetColor(Color, bibColor);
            helmetMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.helmetColor));
            suitTopFrontMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.suitTopFrontColor));
            suitTopBackMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.suitTopBackColor));
            suitBottomFrontMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.suitBottomFrontColor));
            suitBottomBackMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.suitBottomBackColor));
            skisMaterial.SetColor(Color, SimpleColorPicker.Hex2Color(competitor.skisColor));
            LoadHelmetTexture();
            LoadSkisTexture();
        }


    }
}