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
using System.IO;
using System.Collections.Generic;

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
        public Material suitOverlayMaterial;
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


        
        [System.Serializable]
        public class SuitPart
        {
            [Tooltip("Friendly id, e.g. BottomFront, TopFront")]
            public string key;

            [Tooltip("Material asset that is assigned to this submesh in the suitRenderer.sharedMaterials list.")]
            public Material material;

            [Tooltip("Color to apply to _BaseColor for this part when testing.")]
            public Color color;

            [Tooltip("Filename suffix used when loading textures from a chosen suit folder, e.g. _bottomfront.png, _topfront.png")]
            public string fileSuffix;

            [HideInInspector] public int slotIndex = -1; // resolved at runtime
        }


        [Header("Suit (MPB)")]
        [SerializeField] private SkinnedMeshRenderer suitRenderer;
        [SerializeField] private List<SuitPart> suitParts = new List<SuitPart>();
        private static readonly int Color = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock mpb;
        [SerializeField] private int suitBottomFrontIndex = -1;

        private readonly Dictionary<string, Texture2D> _texCache = new Dictionary<string, Texture2D>();
        private Dictionary<Material, int> materialIndexMap;

        public void GetValues()
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            competitor = competitors.competitors[id];
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

            if (string.IsNullOrEmpty(textureName))
            {
                UseDefaultHelmet();
                return;
            }

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

                Material[] mats = customHelmetRenderer.materials;
                if (mats.Length >= 1)
                {
                    Material mat = new Material(mats[0]); // duplicate
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

            if (string.IsNullOrEmpty(textureName))
            {
                UseDefaultSkis();
                return;
            }

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
                tex.Apply(true); // mipmaps

                tex.mipMapBias = mipMapBiasSkis;

                Material[] matsLeft = customLeftSkiRenderer.materials;
                Material[] matsRight = customRightSkiRenderer.materials;
                Material[] matsCloneLeft = customLeftSkiCloneRenderer.materials;
                Material[] matsCloneRight = customRightSkiCloneRenderer.materials;

                if (matsLeft.Length >= 1)
                {
                    Material matl = new Material(matsLeft[1]);
                    matl.mainTexture = tex;
                    matl.mainTexture.mipMapBias = mipMapBiasSkis;

                    matsLeft[1] = matl;
                    matsRight[1] = matl;
                    matsCloneLeft[1] = matl;
                    matsCloneRight[1] = matl;

                    customLeftSkiRenderer.materials = matsLeft;
                    customRightSkiRenderer.materials = matsRight;
                    customLeftSkiCloneRenderer.materials = matsCloneLeft;
                    customRightSkiCloneRenderer.materials = matsCloneRight;

                    customLeftSkiObject.SetActive(true);
                    customRightSkiObject.SetActive(true);
                    customLeftSkiCloneObject.SetActive(true);
                    customRightSkiCloneObject.SetActive(true);
                    leftSkiObject.SetActive(false);
                    rightSkiObject.SetActive(false);
                    leftSkiCloneObject.SetActive(false);
                    rightSkiCloneObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Custom ski renderer has no material slots!");
                    UseDefaultHelmet();
                }
            }
        }



        public void ApplySkiCloneVisuals()
        {
            if (hasCustomSkiTexture)
            {
                customLeftSkiCloneObject.SetActive(true);
                customRightSkiCloneObject.SetActive(true);
                leftSkiCloneObject.SetActive(false);
                rightSkiCloneObject.SetActive(false);
            }
            else
            {
                customLeftSkiCloneObject.SetActive(false);
                customRightSkiCloneObject.SetActive(false);
                leftSkiCloneObject.SetActive(true);
                rightSkiCloneObject.SetActive(true);
            }
        }

        private IEnumerator LoadSuitOverlayTexture(string filePath)
        {
            string uri = new System.Uri(filePath).AbsoluteUri;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("No suit overlay found: " + www.error);
                    yield break;
                }

                Texture2D rawTex = DownloadHandlerTexture.GetContent(www);
                Texture2D tex = new Texture2D(rawTex.width, rawTex.height, rawTex.format, true);
                tex.SetPixels(rawTex.GetPixels());
                tex.Apply(true);

                Material mat = new Material(suitOverlayMaterial);
                mat.mainTexture = tex;

                // 🔍 Get the correct renderer
                var cubeTransform = jumperController?.jumperModel?.transform.Find("Cube");
                if (cubeTransform == null)
                {
                    Debug.LogWarning("Cube object not found in jumper model!");
                    yield break;
                }

                var smr = cubeTransform.GetComponent<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    Debug.LogWarning("Cube has no SkinnedMeshRenderer!");
                    yield break;
                }

                // ✅ Add overlay as last material (rendered on top)
                var mats = smr.materials;
                Array.Resize(ref mats, mats.Length + 1);
                mats[mats.Length - 1] = mat;
                smr.materials = mats;
            }
        }


        private void Awake()
        {
            if (mpb == null) mpb = new MaterialPropertyBlock();

            // Auto-resolve suitRenderer if not set
            if (suitRenderer == null)
            {
                var cube = jumperController?.jumperModel?.transform.Find("Cube");
                if (cube != null)
                    suitRenderer = cube.GetComponent<SkinnedMeshRenderer>();
            }

            // Map SuitPart.material to submesh index
            if (suitRenderer != null)
            {
                var shared = suitRenderer.sharedMaterials;
                foreach (var part in suitParts)
                {
                    part.slotIndex = -1;
                    if (part.material == null) continue;

                    for (int i = 0; i < shared.Length; i++)
                    {
                        if (shared[i] == part.material) // reference equality
                        {
                            part.slotIndex = i;
                            break;
                        }
                    }

                    if (part.slotIndex < 0)
                        Debug.LogWarning($"[SuitParts] Could not find material '{part.material?.name}' for part '{part.key}' in suitRenderer.sharedMaterials.");
                    else
                        Debug.Log($"[SuitParts] Resolved {part.key} → slot {part.slotIndex} ({part.material?.name})");
                }
            }
            else
            {
                Debug.LogWarning("[SuitParts] suitRenderer not set. Assign the Cube's SkinnedMeshRenderer in the Inspector.");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                // Example test folder: StreamingAssets/textures/suits/testsuit/
                ApplySuitFromFolder("testsuit", alsoApplyTextures: true);
            }
        }

        /// <summary>
        /// Applies per-part colors from suitParts and (optionally) loads textures from a suit folder under StreamingAssets/textures/suits/{folderName}.
        /// Expected filenames: {folderName}{fileSuffix}, e.g. testsuit_topfront.png, testsuit_bottomfront.png
        /// </summary>
        public void ApplySuitFromFolder(string folderName, bool alsoApplyTextures)
        {

            if (suitRenderer == null)
            {
                Debug.LogWarning("[ApplySuitFromFolder] suitRenderer is null.");
                return;
            }
            if (suitParts == null || suitParts.Count == 0)
            {
                Debug.LogWarning("[ApplySuitFromFolder] No suitParts configured.");
                return;
            }

            string baseFolder = Path.Combine(Application.streamingAssetsPath, "textures", "suits", folderName);

            foreach (var part in suitParts)
            {
                if (part.slotIndex < 0) continue;

                // Get existing block for that submesh
                suitRenderer.GetPropertyBlock(mpb, part.slotIndex);

                // Color for both URP Lit and custom shader
                mpb.SetColor("_BaseColor", part.color);

                // Optional texture for custom shader
                if (alsoApplyTextures && !string.IsNullOrEmpty(part.fileSuffix))
                {
                    string fileName = folderName + part.fileSuffix;           // e.g. testsuit_topfront.png
                    string fullPath = Path.Combine(baseFolder, fileName);

                    Texture2D tex = LoadTextureCached(fullPath);
                    if (tex != null)
                    {
                        mpb.SetTexture("_SuitTex", tex);
                    }
                    else
                    {
                        Debug.LogWarning($"[ApplySuitFromFolder] Missing texture for '{part.key}': {fullPath}");
                    }
                }

                suitRenderer.SetPropertyBlock(mpb, part.slotIndex);
            }

            Debug.Log($"[ApplySuitFromFolder] Applied colors {(alsoApplyTextures ? "+ textures " : "")}from folder '{folderName}'.");
        }

        private Texture2D LoadTextureCached(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            if (_texCache.TryGetValue(fullPath, out var cached) && cached != null)
                return cached;

            if (!File.Exists(fullPath))
                return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                if (!tex.LoadImage(bytes, markNonReadable: false))
                    return null;

                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.mipMapBias = -0.5f;
                tex.Apply(true, false);

                _texCache[fullPath] = tex;
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LoadTextureCached] Failed to load '{fullPath}': {e.Message}");
                return null;
            }
        }
    

    public void ApplySuitSettings(string baseColor)
        {
            if (suitBottomFrontIndex < 0)
            {
                Debug.LogWarning("Suit bottom front index not set!");
                return;
            }

            Color actualColor = SimpleColorPicker.Hex2Color(baseColor);
            Debug.Log("Running ApplySuitSettings");

            suitRenderer.GetPropertyBlock(mpb, suitBottomFrontIndex);
            mpb.SetColor("_BaseColor", actualColor);
            suitRenderer.SetPropertyBlock(mpb, suitBottomFrontIndex);

            Debug.Log($"Applied {actualColor} to suit material slot {suitBottomFrontIndex}");
        }



        public void ApplyTestSuitColors()
        {
            Debug.Log("▶ Running ApplyTestSuitColors");

            // Example base color (you can expand later to per-part config)
            Color baseColor = SimpleColorPicker.Hex2Color("#3D5D7D");

            // Apply color
            ApplyColorToMaterialSlot(suitBottomFrontMaterial, baseColor);
            ApplyColorToMaterialSlot(suitTopFrontMaterial, baseColor);

            // Load textures for testsuit
            string suitFolder = Path.Combine(Application.streamingAssetsPath, "textures/suits/testsuit/");
            TryApplyTextureToMaterialSlot(suitBottomFrontMaterial, suitFolder, "testsuit_bottomfront.png");
            TryApplyTextureToMaterialSlot(suitTopFrontMaterial, suitFolder, "testsuit_topfront.png");
        }

        private void ApplyColorToMaterialSlot(Material targetMat, Color color)
        {
            if (targetMat == null) return;

            if (materialIndexMap.TryGetValue(targetMat, out int index))
            {
                suitRenderer.GetPropertyBlock(mpb, index);

                // ✅ Both URP Lit & custom shader use _BaseColor
                mpb.SetColor("_BaseColor", color);

                suitRenderer.SetPropertyBlock(mpb, index);

                Debug.Log($"[ApplyColorToMaterialSlot] Applied {color} to {targetMat.name} at index {index}");
            }
            else
            {
                Debug.LogWarning($"[ApplyColorToMaterialSlot] Material {targetMat.name} not found in suitRenderer.sharedMaterials");
            }
        }

        private void TryApplyTextureToMaterialSlot(Material targetMat, string folderPath, string fileName)
        {
            if (targetMat == null) return;

            string texPath = Path.Combine(folderPath, fileName);

            if (!File.Exists(texPath))
            {
                Debug.LogWarning($"[TryApplyTextureToMaterialSlot] Texture not found: {texPath}");
                return;
            }

            byte[] fileData = File.ReadAllBytes(texPath);
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(fileData))
            {
                Debug.LogWarning($"[TryApplyTextureToMaterialSlot] Failed to load PNG: {texPath}");
                return;
            }

            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();

            if (materialIndexMap.TryGetValue(targetMat, out int index))
            {
                suitRenderer.GetPropertyBlock(mpb, index);

                // ✅ Only our custom shader uses _SuitTex
                mpb.SetTexture("_SuitTex", tex);

                suitRenderer.SetPropertyBlock(mpb, index);

                Debug.Log($"[TryApplyTextureToMaterialSlot] Applied {fileName} to {targetMat.name} at index {index}");
            }
            else
            {
                Debug.LogWarning($"[TryApplyTextureToMaterialSlot] Material {targetMat.name} not found in suitRenderer.sharedMaterials");
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

            // Load optional overlay PNG (one per suit, e.g. "slovenia2425.png")

            //LoadSuitTexture(); 
        }

        private void LoadSuitTexture()
        {
            string suitOverlayFile = System.IO.Path.Combine(Application.streamingAssetsPath, "textures", "suits", "slovenia2425.png");
            if (!string.IsNullOrEmpty(suitOverlayFile))
            {
                Debug.Log("Attempt to start Coroutine slovenia2425.png. String: " + suitOverlayFile);
                StartCoroutine(LoadSuitOverlayTexture(suitOverlayFile));
            }
            else
            {
                Debug.Log("Can't find slovenia2425.png");
            }
        }
    }
}
