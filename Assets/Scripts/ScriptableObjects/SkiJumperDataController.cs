using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Jumping;
using OpenSkiJumping.New;
using OpenSkiJumping.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
        public Renderer helmetRenderer;
        public Material transparentHelmetMaterial;
        public float mipMapBias = -1f;
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
            Debug.Log("LoadHelmetTexture");
            string textureName = competitor.helmetTexture;

            if (string.IsNullOrEmpty(textureName))
            {
                SetHelmetMaterialToTransparent();
                return;
            }

            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "textures", "helmet", textureName);
            if (!System.IO.File.Exists(fullPath))
            {
                SetHelmetMaterialToTransparent();
                return;
            }

            StartCoroutine(LoadHelmetTextureCoroutine(fullPath));
        }

        private IEnumerator LoadHelmetTextureCoroutine(string filePath)
        {
            string uri = new System.Uri(filePath).AbsoluteUri;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Failed to load helmet texture: " + www.error);
                    SetHelmetMaterialToTransparent();
                    yield break;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                tex.mipMapBias = mipMapBias;

                Material[] mats = helmetRenderer.materials;

                if (mats.Length >= 1)
                {
                    Material mat = new Material(mats[0]); // Duplicate to avoid editing the original
                    mat.mainTexture = tex;
                    mat.mainTexture.mipMapBias = mipMapBias;
                    mats[0] = mat;

                    helmetRenderer.materials = mats;
                }
            }
        }


        private void SetHelmetMaterialToTransparent()
        {
            Material[] mats = helmetRenderer.materials;
            if (mats.Length >= 1)
            {
                mats[0] = transparentHelmetMaterial;
                helmetRenderer.materials = mats;
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
           // LoadHelmetTexture();
        }
    }
}