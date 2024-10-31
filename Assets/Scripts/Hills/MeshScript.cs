using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Hills.Guardrails;
using OpenSkiJumping.Hills.InrunTracks;
using OpenSkiJumping.Hills.LandingAreas;
using OpenSkiJumping.Hills.Stairs;
using OpenSkiJumping.ScriptableObjects.Variables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI.Extensions.ColorPicker;
using Microsoft.SqlServer.Server;

namespace OpenSkiJumping.Hills
{
    [Serializable]
    public class HillModel
    {
        //Hill construction
        public GameObject gateStairs;

        Mesh gateStairsMesh;
        public GameObject inrun;
        public GameObject inrunConstruction;

        Mesh inrunConstructionMesh;
        Mesh inrunMesh;

        public Vector3 jumperPosition;
        public Quaternion jumperRotation;
        public GameObject landingArea;
        Mesh landingAreaMesh;

        public LineRenderer[] lineRenderers;
        public GameObject outrun;
        public GameObject startGate;
        Mesh startGateMesh;

        //Meshes
        public GameObject terrain;
        public Terrain[] terrains;
    }


    [Serializable]
    public class ModelData
    {
        public GameObject gObj;
        public Material[] materials;

        [HideInInspector] public Mesh mesh;
    }

    public enum TerrainBase
    {
        currentTerrain,
        PerlinNoise,
        flat
    }

    public enum InrunStairsTexture
    {
        Default,
        Plain,
        WhitePlanks,
    }

    public enum InrunOuterGuardrailTexture
    {
        Default,
        Transparent,
        WhitePlanks,
        PlainWhite,
        Glass
    }

    public enum InrunGuardrailTexture
    {
        Default,
        LightYellowPlanks,
        DarkBrownPlanks,
        Transparent,
        WhitePlanks
    }

    public enum InrunConstructionTexture
    {
        Default,
        DefaultWhite,
        WhitePlanks,
        MetalPlate,
        PlainWhite
    }
    public enum LandingAreaGuardrailTexture
    {
        Default,
        DefaultPlanks,
        WhitePlanks,
        Glass,
        ThickGlass,
        SuperThickGlass,
        Transparent
    }
    public enum PoleTexture
    {
        Default,
        PlainWhite,
        Metal,
    }
    public class MeshScript : MonoBehaviour
    {
        public Material daySkybox;
        public ModelData digitsMarks;

        [SerializeField]
        private GameObject polesObject;
        /* Stairs */
        [Space][Header("Stairs")] public ModelData gateStairsL;

        public ModelData gateStairsR;
        public GateStairs gateStairsSO;
        public bool generateGateStairsL;
        public bool generateGateStairsR;
        public bool generateInrunStairsL;
        public bool generateInrunStairsR;
        public bool generateLandingAreaGuardrailL;
        public bool generateLandingAreaGuardrailR;

        /* Settings */
        [Space][Header("Settings")] public bool generateTerrain;

        public Hill hill;

        /* Hill profile */
        [Space][Header("Hill profile")] public HillModel[] hills;

        public ModelData inrun;
        public ModelData inrunConstruction;

        /* Guardrails */
        [Space][Header("Guardrails")] public ModelData inrunGuardrailL;

        public ModelData inrunGuardrailR;
        public Guardrail inrunGuardrailSO;
        public GameObject inrunLampPrefab;
        public ModelData inrunOuterGuardrailL;
        public ModelData inrunOuterGuardrailR;
        public Guardrail InrunOuterGuardrailSO;

        [Range(0.0001f, 1)] public float inrunStairsAngle;

        public ModelData inrunStairsL;
        public ModelData inrunStairsR;

        [FormerlySerializedAs("inrunStairsStepHeigth")]
        [Range(0.01f, 1)]
        public float inrunStairsStepHeight;


        [Range(0, 1)] public float inrunTerrain;

        public InrunTrack inrunTrackSO;
        public Vector3 jumperPosition;
        public Quaternion jumperRotation;
        public GameObject lampPrefab;

        List<GameObject> lamps;
        public ModelData landingArea;
        public ModelData landingAreaGuardrailL;
        public ModelData landingAreaGuardrailR;
        public Guardrail LandingAreaGuardrailSO;
        public LandingArea landingAreaSO;
        public Material nightSkybox;
        public ModelData outrunGuardrail;

        public HillProfileVariable profileData;

        [Header("Hill data")] public bool saveMesh;

        /* Hill construction */
        [Space][Header("Hill construction")] public GameObject startGate;

        //Lighting

        public Light sunLight;


        public TerrainBase terrainBase;

        /* Terrain */
        [Space][Header("Terrain")] public GameObject terrainObject;

        private Terrain[] terrains;
        public int time;

        private Dictionary<Material, Color> originalMaterialColors = new Dictionary<Material, Color>();

        public void SetGate(Hill hill, int nr)
        {
            jumperPosition = new Vector3(hill.GatePoint(nr).x, hill.GatePoint(nr).y, 0);
            jumperRotation.eulerAngles = new Vector3(0, 0, -hill.gamma);
        }

        public float GetKPointInCompetition()
        {
            hill.SetValues(profileData.Value);
            return hill.w;
        }

        public float GetHSInCompetition()
        {
            hill.SetValues(profileData.Value);
            return hill.hS;
        }

        public void GenerateMesh()
        {
            hill.SetValues(profileData.Value);
            inrunTerrain = profileData.Value.terrainSteepness;
            generateGateStairsL = profileData.Value.gateStairsLeft;
            generateGateStairsR = profileData.Value.gateStairsRight;
            generateInrunStairsL = profileData.Value.inrunStairsLeft;
            generateInrunStairsR = profileData.Value.inrunStairsRight;
            inrunStairsAngle = profileData.Value.inrunStairsAngle;

            GenerateInrunCollider();
            GenerateInrunTrack();
            GenerateLandingAreaCollider();
            GenerateLandingArea();

            GenerateGateStairs(gateStairsL, 0, generateGateStairsL);
            GenerateGateStairs(gateStairsR, 1, generateGateStairsR);
            var stepsCount = Mathf.RoundToInt((hill.A.y - hill.T.y) / inrunStairsStepHeight);
            inrunStairsStepHeight = (hill.A.y - hill.T.y) / stepsCount;
            GenerateInrunStairs(inrunStairsL, 0, generateInrunStairsL, generateGateStairsL, stepsCount);
            GenerateInrunStairs(inrunStairsR, 1, generateInrunStairsR, generateGateStairsR, stepsCount);
            GenerateLandingAreaGuardrail(landingAreaGuardrailL, 0, generateLandingAreaGuardrailL);
            GenerateLandingAreaGuardrail(landingAreaGuardrailR, 1, generateLandingAreaGuardrailR);
            GenerateInrunGuardrail(inrunGuardrailL, 0, true);
            GenerateInrunGuardrail(inrunGuardrailR, 1, true);
            GenerateInrunOuterGuardrail(inrunOuterGuardrailL, 0, true, generateGateStairsL);
            GenerateInrunOuterGuardrail(inrunOuterGuardrailR, 1, true, generateGateStairsR);
            GenerateInrunConstruction();
            GeneratePoles();
            GenerateMarks();

            if (generateTerrain)
            {
                // Debug.Log("GENERATING TERRAIN");
                GetTerrain();
                // Transform hillTransform = GetComponent<Transform>().transform;
                const float offset = 300f;
                const float inrunFlatLength = 5f;
                //Terrain
                foreach (var terr in terrains)
                {
                    var center = terr.GetComponent<Transform>().position;
                    var tab = new float[terr.terrainData.heightmapResolution,
                        terr.terrainData.heightmapResolution];
                    for (var i = 0; i < terr.terrainData.heightmapResolution; i++)
                    {
                        for (var j = 0; j < terr.terrainData.heightmapResolution; j++)
                        {
                            var x = (float)(j) / (terr.terrainData.heightmapResolution - 1) *
                                (terr.terrainData.size.x) + center.x;
                            var z = (float)(i) / (terr.terrainData.heightmapResolution - 1) *
                                (terr.terrainData.size.z) + center.z;


                            float hillY = 0;
                            float b = 15;
                            float c = 0;

                            if (x < hill.A.x)
                            {
                                c = hill.A.x + inrunFlatLength - x;
                                hillY = hill.A.y * inrunTerrain - hill.s;
                            }
                            else if (x < hill.T.x)
                            {
                                hillY = hill.Inrun(x) * inrunTerrain - hill.s;
                                if (hill.A.x <= x) b = 15;
                            }
                            else if (hill.T.x <= x && x <= hill.U.x)
                            {
                                hillY = hill.LandingArea(x);
                                b = 15;
                            }
                            else if (x <= hill.U.x + hill.a)
                            {
                                hillY = hill.U.y;
                                b = 15;
                            }
                            else
                            {
                                c = x - hill.U.x - hill.a;
                                hillY = hill.U.y;
                            }

                            // float terrainY = 200 * Mathf.PerlinNoise(x / 200.0f + 2000, z / 200.0f + 2000);
                            var terrainY = hill.U.y;
                            if (terrainBase == TerrainBase.PerlinNoise)
                            {
                                terrainY = 200 * Mathf.PerlinNoise(x / 200.0f + 2000, z / 200.0f + 2000);
                            }
                            else if (terrainBase == TerrainBase.currentTerrain)
                            {
                                terrainY = terr.terrainData.GetHeight(j, i) + center.y;
                            }

                            var blendFactor = Mathf.SmoothStep(0, 1,
                                Mathf.Clamp01(new Vector2(Mathf.Clamp01((Mathf.Abs(z) - b) / offset),
                                    Mathf.Clamp01(c / offset)).magnitude));
                            var y = hillY * (1 - blendFactor) + terrainY * blendFactor;

                            // y += (Mathf.Abs((Mathf.Abs(z) - b)) <= 50 ? 2 * Mathf.Abs((Mathf.Abs(z) - b)) : 100) * 0.5f);


                            y = (y - center.y - 1) / terr.terrainData.size.y;


                            // if (i == 200 && j == 200) Debug.Log(x + " " + y);
                            // Debug.Log(x + " " + y);

                            tab[i, j] = Mathf.Clamp(y, 0, 1);
                        }
                    }

                    terr.terrainData.SetHeights(0, 0, tab);
                }
            }

            if (saveMesh)
            {
                SaveMesh(inrun.gObj, "Inrun", true);
                SaveMesh(inrun.gObj, "InrunTrack");
                SaveMesh(landingArea.gObj, "LandingAreaCollider", true);
                SaveMesh(landingArea.gObj, "LandingArea");
                SaveMesh(gateStairsL.gObj, "GateStairsL");
                SaveMesh(gateStairsR.gObj, "GateStairsR");
                SaveMesh(inrunStairsL.gObj, "InrunStairsL");
                SaveMesh(inrunStairsR.gObj, "InrunStairsR");
                SaveMesh(landingAreaGuardrailL.gObj, "LandingAreaGuardrailL");
                SaveMesh(landingAreaGuardrailR.gObj, "LandingAreaGuardrailR");
                SaveMesh(inrunGuardrailL.gObj, "InrunGuardrailL");
                SaveMesh(inrunConstruction.gObj, "InrunConstruction");
                SaveMesh(digitsMarks.gObj, "DigitsMarks");
            }

            SetGate(hill, 1);
            DestroyLamps();
            if (time == 0)
            {
                sunLight.intensity = 0.1f;
                RenderSettings.skybox = nightSkybox;
                GenerateLamps();
            }
            else if (time == 1)
            {
                sunLight.intensity = 1f;
                RenderSettings.skybox = daySkybox;
            }
        }

        public void GetTerrain()
        {
            terrains = terrainObject.GetComponentsInChildren<Terrain>();
        }

        public void ObjectUpdate(GameObject gameObject, Mesh mesh, Material material, Vector3[] vertices, int[] triangles, Vector2[] uvs, bool hasCollider)
        {
            // Set the mesh data
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            // Apply the mesh to the gameObject
            gameObject.GetComponent<MeshFilter>().mesh = mesh;

            // Handle texture and color for different game objects
            if (gameObject.name.Contains("Inrun Stairs"))
            {
                // Load material based on the texture in hill.inrunStairsTexture
                if (hill.inrunStairsTexture != "Default")
                {
                    if (System.Enum.TryParse(hill.inrunStairsTexture, out InrunStairsTexture stairsTextureEnum))
                    {
                        int materialIndex = (int)stairsTextureEnum;

                        // Ensure the index is within bounds
                        if (materialIndex >= 0 && materialIndex < gateStairsSO.materials.Length)
                        {
                            var newMaterial = gateStairsSO.materials[materialIndex];

                            // Store original color if not already stored
                            if (!originalMaterialColors.ContainsKey(newMaterial))
                            {
                                originalMaterialColors[newMaterial] = newMaterial.GetColor("_BaseColor");
                            }

                            // Parse and apply color from hill.inrunStairsColor
                            if (ColorUtility.TryParseHtmlString(hill.inrunStairsColor, out Color inrunStairsColor))
                            {
                                Color originalColor = newMaterial.GetColor("_BaseColor");
                                inrunStairsColor.a = originalColor.a; // Preserve alpha channel
                                newMaterial.SetColor("_BaseColor", inrunStairsColor);
                            }
                            else
                            {
                                Debug.LogError("Invalid hex color string: " + hill.inrunStairsColor);
                            }

                            // Assign the material to the renderer
                            gameObject.GetComponent<MeshRenderer>().material = newMaterial;
                        }
                        else
                        {
                            Debug.LogError("Material index out of bounds for inrun stairs.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid texture string for inrun stairs: " + hill.inrunStairsTexture);
                    }
                }
                else
                {
                    gameObject.GetComponent<MeshRenderer>().material = material;
                }
            }

            // Handle distance plates
            if (gameObject.name == "Marks Object")
            {
                Color distancePlatesColor;
                if (ColorUtility.TryParseHtmlString(hill.distancePlatesColor, out distancePlatesColor))
                {
                    material.color = distancePlatesColor;
                }
                else
                {
                    material.color = Color.white;
                }
                gameObject.GetComponent<MeshRenderer>().material = material;
            }

            // Handle inrun construction
            if (gameObject.name == "Inrun Construction")
            {
                // Load material based on the texture in hill.inrunConstructionTexture
                if (hill.inrunConstructionTexture != "Default")
                {
                    if (System.Enum.TryParse(hill.inrunConstructionTexture, out InrunConstructionTexture constructionTextureEnum))
                    {
                        int materialIndex = (int)constructionTextureEnum;

                        // Ensure the index is within bounds
                        if (materialIndex >= 0 && materialIndex < inrunConstruction.materials.Length)
                        {
                            var newMaterial = inrunConstruction.materials[materialIndex];

                            // Store original color if not already stored
                            if (!originalMaterialColors.ContainsKey(newMaterial))
                            {
                                originalMaterialColors[newMaterial] = newMaterial.GetColor("_BaseColor");
                            }

                            // Parse and apply color from hill.inrunConstructionColor
                            if (ColorUtility.TryParseHtmlString(hill.inrunConstructionColor, out Color inrunColor))
                            {
                                Color originalColor = newMaterial.GetColor("_BaseColor");
                                inrunColor.a = originalColor.a; // Preserve alpha channel
                                newMaterial.SetColor("_BaseColor", inrunColor);
                            }
                            else
                            {
                                Debug.LogError("Invalid hex color string: " + hill.inrunConstructionColor);
                            }

                            gameObject.GetComponent<MeshRenderer>().material = newMaterial; // Assign the new material
                        }
                        else
                        {
                            Debug.LogError("Material index out of bounds for inrun construction.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid texture string for inrun construction: " + hill.inrunConstructionTexture);
                    }
                }
            }

            // Add a MeshCollider if specified
            if (hasCollider)
            {
                gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }


        // Coroutine to load and apply a texture from the user-provided path
        private IEnumerator LoadTextureForObject(GameObject gameObject, string filePath)
        {
            // Format the file path for local file access
            string fileUri = $"file:///{filePath}";

            // Create a UnityWebRequest to load the texture from the file path
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileUri);
            yield return request.SendWebRequest();

            // Check if the request succeeded
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Get the loaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                // Apply the texture to the MeshRenderer of the "Inrun Construction" object
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null && texture != null)
                {
                    meshRenderer.material.mainTexture = texture;
                }
                else
                {
                    Debug.LogError("MeshRenderer or texture is null.");

                }
            }
            else
            {
                Debug.LogError($"Failed to load texture from {filePath}. Error: {request.error}");
            }
        }


        public int[] FacesToTriangles(List<(int, int, int, int)> facesList)
        {
            var triangles = new List<int>();
            foreach (var face in facesList)
            {
                triangles.Add(face.Item1);
                triangles.Add(face.Item2);
                triangles.Add(face.Item3);
                triangles.Add(face.Item2);
                triangles.Add(face.Item4);
                triangles.Add(face.Item3);
            }

            return triangles.ToArray();
        }

        public void GenerateInrunCollider()
        {
            var mesh = new Mesh();

            var vertices = new Vector3[hill.inrunPoints.Length * 2];
            var uvs = new Vector2[hill.inrunPoints.Length * 2];
            var triangles = new int[(hill.inrunPoints.Length - 1) * 6];

            var len = new float[hill.inrunPoints.Length];

            for (var i = 1; i < hill.inrunPoints.Length; i++)
            {
                len[i] = len[i - 1] + (hill.inrunPoints[i] - hill.inrunPoints[i - 1]).magnitude;
            }

            for (var i = 0; i < hill.inrunPoints.Length; i++)
            {
                vertices[2 * i] = new Vector3(hill.inrunPoints[i].x, hill.inrunPoints[i].y, -profileData.Value.b1 / 2);
                vertices[2 * i + 1] =
                    new Vector3(hill.inrunPoints[i].x, hill.inrunPoints[i].y, profileData.Value.b1 / 2);
                uvs[2 * i] = new Vector2(len[i], -profileData.Value.b1);
                uvs[2 * i + 1] = new Vector2(len[i], profileData.Value.b1);
            }

            for (var i = 0; i < hill.inrunPoints.Length - 1; i++)
            {
                triangles[6 * i + 0] = 2 * i + 0;
                triangles[6 * i + 1] = 2 * i + 3;
                triangles[6 * i + 2] = 2 * i + 1;
                triangles[6 * i + 3] = 2 * i + 0;
                triangles[6 * i + 4] = 2 * i + 2;
                triangles[6 * i + 5] = 2 * i + 3;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            inrun.gObj.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public void GenerateLandingAreaCollider()
        {
            var mesh = new Mesh();
            var verticesList = new List<Vector3>();
            var uvsList = new List<Vector2>();
            var facesList = new List<(int, int, int, int)>();

            var b = new float[hill.landingAreaPoints.Length];

            for (var i = 0; i < hill.landingAreaPoints.Length; i++)
            {
                b[i] = hill.landingAreaPoints[i].x <= hill.K.x
                    ? (profileData.Value.b2 / 2) + hill.landingAreaPoints[i].x / hill.K.x *
                    ((profileData.Value.bK - profileData.Value.b2) / 2)
                    : hill.landingAreaPoints[i].x >= hill.U.x
                        ? (profileData.Value.bU / 2)
                        : (profileData.Value.bK / 2) + (hill.landingAreaPoints[i].x - hill.K.x) /
                        (hill.U.x - hill.K.x) * ((profileData.Value.bU - profileData.Value.bK) / 2);
            }

            for (var i = 0; i < hill.landingAreaPoints.Length; i++)
            {
                verticesList.Add(new Vector3(hill.landingAreaPoints[i].x, hill.landingAreaPoints[i].y, -b[i]));
                uvsList.Add(new Vector2(i, -b[i]));

                verticesList.Add(new Vector3(hill.landingAreaPoints[i].x, hill.landingAreaPoints[i].y, b[i]));
                uvsList.Add(new Vector2(i, b[i]));

                if (i > 0)
                {
                    var x = verticesList.Count;
                    facesList.Add((x - 4, x - 3, x - 2, x - 1));
                }
            }

            mesh.vertices = verticesList.ToArray();
            mesh.triangles = FacesToTriangles(facesList);
            mesh.uv = uvsList.ToArray();
            landingArea.gObj.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public void GenerateInrunConstruction()
        {
            var mesh = new Mesh();
            var verticesList = new List<Vector3>();
            var uvsList = new List<Vector2>();
            var facesList = new List<(int, int, int, int)>();

            // Define the width of the inrun using a function.
            Func<float, float> width = (xx => 0.5f / hill.A.y / hill.A.y * (xx - hill.A.x) * (xx - hill.A.x) * (xx - hill.A.x) + 3f);

            // Calculate the critical point on the inrun based on its X value.
            var criticalPointX = Mathf.Lerp(hill.GatePoint(-1).x, hill.T.x, inrunStairsAngle);
            var p1 = hill.inrunPoints.Last(it => it.x > criticalPointX);
            var p2 = hill.inrunPoints.First(it => it.x <= criticalPointX);
            var criticalPoint = Vector2.Lerp(p1, p2, (criticalPointX - p1.x) / (p2.x - p1.x));

            // Create a list of points to define the inrun's shape.
            var tmpList = new List<Vector2>();
            var tmpListPoles = new List<Vector2>();
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x > criticalPoint.x));
            tmpList.Add(criticalPoint);
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x <= criticalPoint.x && it.x > hill.GatePoint(-1).x));
            tmpList.Add(hill.GatePoint(-1));
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x <= hill.GatePoint(-1).x));

            tmpListPoles.AddRange(hill.inrunPolePoints);

            // Prepare the bounds for the left and right sides of the inrun.
            var b0 = (!generateGateStairsL
                ? tmpList.Select(it => -(hill.b1 / 2 + 0.7f)).ToArray()
                : tmpList.Select(it => -(it.x > hill.GatePoint(-1).x
                    ? (it.x > criticalPoint.x
                        ? hill.b1 / 2 + 0.7f
                        : Mathf.Lerp(hill.b1 / 2 + 0.7f, hill.b1 / 2 + gateStairsSO.StepWidth,
                            (it.x - criticalPointX) / (criticalPointX - hill.GatePoint(-1).x)))
                    : (hill.b1 / 2 + gateStairsSO.StepWidth))).ToArray());

            var b1 = (!generateGateStairsR
                ? tmpList.Select(it => hill.b1 / 2 + 0.7f).ToArray()
                : tmpList.Select(it => (it.x > hill.GatePoint(-1).x
                    ? (it.x > criticalPoint.x
                        ? hill.b1 / 2 + 0.7f
                        : Mathf.Lerp(hill.b1 / 2 + 0.7f, hill.b1 / 2 + gateStairsSO.StepWidth,
                            (it.x - criticalPointX) / (criticalPointX - hill.GatePoint(-1).x)))
                    : (hill.b1 / 2 + gateStairsSO.StepWidth))).ToArray());

            var b2 = (!generateGateStairsL
                ? tmpListPoles.Select(it => -(hill.b1 / 2 + 0.7f)).ToArray()
                : tmpListPoles.Select(it => -(it.x > hill.GatePoint(-1).x
                    ? (it.x > criticalPoint.x
                        ? hill.b1 / 2 + 0.7f
                        : Mathf.Lerp(hill.b1 / 2 + 0.7f, hill.b1 / 2 + gateStairsSO.StepWidth,
                            (it.x - criticalPointX) / (criticalPointX - hill.GatePoint(-1).x)))
                    : (hill.b1 / 2 + gateStairsSO.StepWidth))).ToArray());

            var b3 = (!generateGateStairsR
                ? tmpListPoles.Select(it => hill.b1 / 2 + 0.7f).ToArray()
                : tmpListPoles.Select(it => (it.x > hill.GatePoint(-1).x
                    ? (it.x > criticalPoint.x
                        ? hill.b1 / 2 + 0.7f
                        : Mathf.Lerp(hill.b1 / 2 + 0.7f, hill.b1 / 2 + gateStairsSO.StepWidth,
                            (it.x - criticalPointX) / (criticalPointX - hill.GatePoint(-1).x)))
                    : (hill.b1 / 2 + gateStairsSO.StepWidth))).ToArray());

            // Calculate the length of the inrun.
            var len = new float[tmpList.Count];
            for (var i = 1; i < tmpList.Count; i++)
            {
                len[i] = len[i - 1] + (tmpList[i] - tmpList[i - 1]).magnitude;
            }

            float inrunMinHeight = hill.inrunMinHeight;
            Debug.Log("tmpList.Count: " + tmpList.Count + " verticesList.Count: " + verticesList.Count);


            // Generate vertices and UVs for the mesh.
            for (var i = 0; i < tmpList.Count; i++)
            {
                var tmp = verticesList.Count;
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - 0.1f, b0[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y));
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - 0.1f, b1[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y));

                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y, b0[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y));
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y, b1[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y));

                //change 3,5f to a variable loaded from hill/the json file later on
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - Math.Max((tmpList[i].y) * 0.1f, inrunMinHeight), b0[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y - width(tmpList[i].x)));
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - Math.Max((tmpList[i].y) * 0.1f, inrunMinHeight), b1[i]));
                uvsList.Add(new Vector2(tmpList[i].x, tmpList[i].y - width(tmpList[i].x)));

                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - width(tmpList[i].x), b0[i]));
                uvsList.Add(new Vector2(tmpList[i].x, -2));
                verticesList.Add(new Vector3(tmpList[i].x, tmpList[i].y - width(tmpList[i].x), b1[i]));
                uvsList.Add(new Vector2(tmpList[i].x, 2));

               /* if (i == 5)
                {
                    Debug.Log($"Before running faceadding i = {i} verticesList.Count = {verticesList.Count} tmp: {tmp}");
                    for (int j = 16; j > 0; j--)
                    {
                        Debug.Log($"VerticeList postion {48 - j} = {verticesList[48 - j]}");
                    }

                }
               */


                tmp = verticesList.Count - tmp;
                if (i > 0)
                {
                    int x = verticesList.Count;
                    facesList.Add((x - 4, x - 6, x - (tmp + 4), x - (tmp + 6)));
                    facesList.Add((x - 5, x - 3, x - (tmp + 5), x - (tmp + 3)));
                    facesList.Add((x - 1, x - 2, x - (tmp + 1), x - (tmp + 2)));
                    facesList.Add((x - 8, x - 7, x - (tmp + 8), x - (tmp + 7)));
                }


            /*    if (i == 5)
                {
                    Debug.Log($"After faceadding i = {i} verticesList.Count = {verticesList.Count} tmp: {tmp}");
                    for (int j = facesList.Count; j > facesList.Count - 8; j--)
                    {
                        Debug.Log($"FacesList[{j-1}]:  = {facesList[j-1]}");
                    }
                }
            */

            }


            /*
            float zWidthCoefficient = 1.0f; // Adjust as necessary or load from hill/json
            float poleXWidth = 2;
            int numberOfPoles = 4; //Mathf.FloorToInt(tmpList.Count / 5f);
            */



            int poleSpacing = 7;
            int poleThickness = 2;
            float poleHeight = 30;
            var poleSegments = new List<int>();

            for(int i = 0; i < tmpListPoles.Count; i++)
            {
                if (i % poleSpacing == 0)
                {
                    poleSegments.Add(i);
                    for (int j = 1; j <= poleThickness; j++)
                    {
                        poleSegments.Add(i - j);
                    }
                }

            }

            for (int i = 0; i < poleSegments.Count; i++)
            {
                Debug.Log("Valid pole segment = " + poleSegments[i]);
            }

            for (int i = 0; i < tmpListPoles.Count; i++)
            {
                Debug.Log("tmpListPoles b3 dla i = " + i + " = " + b3[i]);
            }

            Debug.Log($"tmpList count {tmpList.Count} tmpListPoles count {hill.inrunPolePoints.Length}");




            /*
            //Generate the poles the high is resting on
            for (int i = 0; i < tmpListPoles.Count; i++)
            {
                var tmp = verticesList.Count;
                //if(i > 0 && ((i+1) % 5 == 0 || i % 5 == 0) && (i+1) < tmpList.Count-1)
                if (poleSegments.Contains(i) && i > 0)
                {
                    Debug.Log("The pole method was called when i = " + i);
                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - inrunMinHeight - 0.1f, b2[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y));
                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - inrunMinHeight - 0.1f, b3[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y));

                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - inrunMinHeight, b2[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y));
                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - inrunMinHeight, b3[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y));

                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - poleHeight, b2[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y - width(tmpListPoles[i].x)));
                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - poleHeight, b3[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, tmpListPoles[i].y - width(tmpListPoles[i].x)));

                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - width(tmpListPoles[i].x), b2[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, -2));
                    verticesList.Add(new Vector3(tmpListPoles[i].x, tmpListPoles[i].y - width(tmpListPoles[i].x), b3[i]));
                    uvsList.Add(new Vector2(tmpListPoles[i].x, 2));
                }

                tmp = verticesList.Count - tmp;
                int x = verticesList.Count;
                if (poleSegments.Contains(i - 1) && i > 0)
                {
                    Debug.Log("The faces were created when i = " + i);
                    facesList.Add((x - 4, x - 6, x - (tmp + 4), x - (tmp + 6)));
                    facesList.Add((x - 5, x - 3, x - (tmp + 5), x - (tmp + 3)));
                    facesList.Add((x - 1, x - 2, x - (tmp + 1), x - (tmp + 2)));
                    facesList.Add((x - 8, x - 7, x - (tmp + 8), x - (tmp + 7)));


                }

                if (i % poleSpacing == 0) { 
                facesList.Add((x - 6, x - 5, x - 3, x - 4));
                facesList.Add((x - 8, x - 7, x - 3, x - 4));
                facesList.Add((x - 8, x - 7, x - 1, x - 2));
                facesList.Add((x - 6, x - 5, x - 1, x - 2));
                }

                if ((i + poleThickness + 1) % poleSpacing == 0)
                {
                    facesList.Add((x - 6, x - 5, x - 3, x - 4));
                    facesList.Add((x - 8, x - 7, x - 3, x - 4));
                    facesList.Add((x - 8, x - 7, x - 1, x - 2));
                    facesList.Add((x - 6, x - 5, x - 1, x - 2));
                }

                /*if (poleSegments.Contains(i) && !poleSegments.Contains(i-2))
                {
                    Debug.Log("i % poleSpacing == poleThickness was run when i =  " + i);
                    facesList.Add((x - 8, x - 7, x - 3, x - 4));
                }*/



                /*
                Vector3 polePosition = new Vector3(tmpList[i].x, tmpList[i].y - Math.Max((tmpList[i].y) * 0.09f, 3.5f), 0);

                // Calculate pole height based on tmpList[i] values
                float upperPoint = tmpList[i].y - tmpList[i].y - Math.Max((tmpList[i].y) * 0.09f, 3.5f); // Upper point just below tmpList[i].y
                float lowerPoint = tmpList[i].y - width(tmpList[i].x * 2); // Lower point at tmpList[i].y - width(tmpList[i].x)
                float poleHeight = Mathf.Abs(upperPoint - lowerPoint);

                // Width along z-axis between b0 and b1, adjusted with coefficient
                float poleZWidth = Mathf.Abs(b1[i] - b0[i]) * zWidthCoefficient;
            }*/



            // Create the take-off table.
            verticesList.Add(new Vector3(0, 0, b0[0]));
            uvsList.Add(new Vector2(-2, 0));
            verticesList.Add(new Vector3(0, 0, b1[0]));
            uvsList.Add(new Vector2(2, 0));
            verticesList.Add(new Vector3(0, -inrunMinHeight, b0[0]));
            uvsList.Add(new Vector2(-2, -width(0)));
            verticesList.Add(new Vector3(0, -inrunMinHeight, b1[0]));
            uvsList.Add(new Vector2(2, -width(0)));



            







                /*

                // Create poles at defined intervals

                float poleWidth = 10.2f; // Pole width, adjust as necessary
                Vector3 inrunDirection = (hill.E2 - hill.E1).normalized; // Direction from E1 to E2
                float inrunLength = Vector3.Distance(hill.E1, hill.E2);
                int numberOfPoles = Mathf.FloorToInt(inrunLength / 5f); // Adjust for pole spacing
                                                                        // Adjustable coefficient for pole width along the z-axis
                float zWidthCoefficient = 1.0f; // Adjust as necessary or load from hill/json

                // Create poles along the inrun
                for (int i = 0; i < tmpList.Count; i++)
                {


                    // Calculate pole height based on tmpList[i] values
                    float upperPoint = tmpList[i].y - tmpList[i].y - Math.Max((tmpList[i].y) * 0.09f,3.5f); // Upper point just below tmpList[i].y
                    float lowerPoint = tmpList[i].y - width(tmpList[i].x*2); // Lower point at tmpList[i].y - width(tmpList[i].x)
                    float poleHeight = Mathf.Abs(upperPoint - lowerPoint);
                                    Vector3 polePosition = new Vector3(tmpList[i].x, tmpList[i].y - Math.Max((tmpList[i].y) * 0.09f, 3.5f), 0);
                    // Width along z-axis between b0 and b1, adjusted with coefficient
                    float poleZWidth = Mathf.Abs(b1[i] - b0[i]) * zWidthCoefficient;

                    // Convert texture string to enum safely
                    if (Enum.TryParse(hill.inrunConstructionTexture, out InrunConstructionTexture textureEnum))
                    {
                        Color poleColor = ColorUtility.TryParseHtmlString(hill.inrunConstructionColor, out var color) ? color : Color.white;
                        CreatePole(polePosition, poleHeight, poleZWidth, textureEnum, poleColor);
                    }
                    else
                    {
                        Debug.LogError("Invalid texture string: " + hill.inrunConstructionTexture);
                    }
                }
                */



                // Create the top of the hill.
                verticesList.Add(new Vector3(hill.A.x, hill.A.y, b0[b0.Length - 1]));
            uvsList.Add(new Vector2(-2, 0));
            verticesList.Add(new Vector3(hill.A.x, hill.A.y, b1[b1.Length - 1]));
            uvsList.Add(new Vector2(2, 0));
            verticesList.Add(new Vector3(hill.A.x, hill.A.y - width(hill.A.x), b0[b0.Length - 1]));
            uvsList.Add(new Vector2(-2, -width(hill.A.x)));
            verticesList.Add(new Vector3(hill.A.x, hill.A.y - width(hill.A.x), b1[b1.Length - 1]));
            uvsList.Add(new Vector2(2, -width(hill.A.x)));



            // Add the final face connections.
            int finalX = verticesList.Count;
            facesList.Add((finalX - 8, finalX - 7, finalX - 6, finalX - 5));
            facesList.Add((finalX - 3, finalX - 4, finalX - 1, finalX - 2));

            // Convert the faces to triangles.
            var vertices = verticesList.ToArray();
            var uvs = uvsList.ToArray();
            var triangles = FacesToTriangles(facesList);




            // Apply the changes to the object using the ObjectUpdate method.
            ObjectUpdate(inrunConstruction.gObj, mesh, inrunConstruction.materials[0], vertices, triangles, uvs, false);
        }


        private void GeneratePoles()
        {
            // Define pole dimensions and properties
            float poleThickness = hill.poleThickness;
            float maxPoleHeight = 50f;
            float inrunMinHeight = hill.inrunMinHeight;
            float uvStretchFactor = 100f;
            float poleSpacing = hill.poleSpacing;

            // Get the shared materials list from Poles object
            Renderer polesRenderer = polesObject.GetComponent<Renderer>();
            if (polesRenderer == null)
            {
                Debug.LogError("Renderer not found on Poles GameObject.");
                return;
            }

            Material[] materials = polesRenderer.sharedMaterials;
            Material selectedMaterial = null;

            // Select material based on hill.poleTexture enum-like string
            // Select material based on hill.poleTexture enum-like string
            if (hill.poleTexture == "Default")
            {
                selectedMaterial = materials[0];  // Default to the first material
            }
            else if (Enum.IsDefined(typeof(PoleTexture), hill.poleTexture))
            {
                PoleTexture textureEnum = (PoleTexture)Enum.Parse(typeof(PoleTexture), hill.poleTexture);
                int textureIndex = (int)textureEnum;

                if (textureIndex >= 0 && textureIndex < materials.Length)
                {
                    selectedMaterial = materials[textureIndex];
                    Debug.Log("Assigned material: " + hill.poleTexture);
                }
                else
                {
                    Debug.LogError("Texture index out of range for Poles materials.");
                    return;
                }
            }
            else
            {
                Debug.LogError("Invalid texture string: " + hill.poleTexture);
                return;
            }

            // Define the pole positions
            List<int> poleSegments = new List<int>();
            for (int i = 0; i < hill.inrunPolePoints.Length; i++)
            {
                if (i % poleSpacing == 0)
                {
                    poleSegments.Add(i);
                }
            }

            // Generate poles
            for (int i = 0; i < hill.inrunPolePoints.Length; i++)
            {
                if (poleSegments.Contains(i) && i > poleThickness)
                {
                    float poleZWidth = Mathf.Abs(hill.b1 + 1.4f);
                    Vector2 position = hill.inrunPolePoints[i];
                    float heightFactor = 1f - (float)i / (hill.inrunPolePoints.Length - 1);
                    float adjustedHeight = Mathf.Lerp(maxPoleHeight, maxPoleHeight * 0.2f, heightFactor);
                    float lowerPoint = position.y - adjustedHeight / 2;

                    Vector3 polePosition = new Vector3(position.x, lowerPoint, 0);

                    GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pole.transform.position = polePosition;
                    pole.transform.localScale = new Vector3(poleThickness, adjustedHeight - inrunMinHeight - 0.5f, poleZWidth - 0.1f);

                    // Assign the selected material and adjust UV
                    Renderer poleRenderer = pole.GetComponent<Renderer>();
                    poleRenderer.material = selectedMaterial;
                    float uvStretch = Mathf.Lerp(1, uvStretchFactor, heightFactor);
                    poleRenderer.material.mainTextureScale = new Vector2(1, uvStretch);

                    // Obscure base of poles at bottom of hill
                    if (i > hill.inrunPolePoints.Length * 0.8f)
                    {
                        float partialHeight = adjustedHeight * heightFactor;
                        pole.transform.localScale = new Vector3(poleThickness, partialHeight, poleZWidth);
                        pole.transform.position = new Vector3(position.x, lowerPoint - (adjustedHeight - partialHeight) / 2, 0);
                    }
                }
            }
        }
        private void CreatePole(Vector3 position, float height, float zWidth, InrunConstructionTexture texture, Color color)
        {
            // Create the pole GameObject
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Positioning the pole to start from the given position (just below tmpList[i].y)
            pole.transform.position = position - new Vector3(0, height / 2, 0); // Adjust to center vertically based on height
            pole.transform.localScale = new Vector3(1.0f, height, zWidth); // Set zWidth as width along z-axis

            // Assign the material and color
            Material poleMaterial = new Material(Shader.Find("Standard"));

            // Load material based on the texture enum
            int materialIndex = (int)texture;
            if (materialIndex >= 0 && materialIndex < inrunConstruction.materials.Length)
            {
                poleMaterial = inrunConstruction.materials[materialIndex];
            }

            poleMaterial.color = color; // Set the pole color
            pole.GetComponent<Renderer>().material = poleMaterial; // Apply the material to the pole
        }

        public void GenerateInrunTrack()
        {
            var mesh = inrunTrackSO.Generate(profileData.Value.b1, hill.inrunPoints);
            inrun.gObj.GetComponent<MeshFilter>().mesh = mesh;
            inrun.gObj.GetComponent<MeshRenderer>().materials = inrunTrackSO.GetMaterials();
        }

        public void GenerateGateStairs(ModelData gateStairs, int side, bool generate)
        {
            /* 0 - Left, 1 - Right */
                if (!generate)
            {
                gateStairs.gObj.GetComponent<MeshFilter>().mesh = null;
                gateStairs.gObj.GetComponent<MeshRenderer>().material = null;
                return;
            }

            var mesh = gateStairsSO.Generate(side, hill.A, hill.B, hill.b1, hill.gates);
            gateStairs.gObj.GetComponent<MeshFilter>().mesh = mesh;

            // Load the material based on inrunStairsTexture
            Material material = null;
            if (hill.inrunStairsTexture == "Default")
            {
                material = gateStairsSO.GetMaterial(0);  // Assume 0 is the index for the default material
                Debug.Log("Assigned default material.");
            }
            else if (System.Enum.TryParse(hill.inrunStairsTexture, out InrunStairsTexture textureEnum))
            {
                int textureIndex = (int)textureEnum;
                material = gateStairsSO.GetMaterial(textureIndex);
                Debug.Log("Assigned material: " + hill.inrunStairsTexture);
            }
            else
            {
                Debug.LogError("Invalid texture string: " + hill.inrunStairsTexture);
                return;  // Exit the method early if the texture string is invalid
            }

            // Now, handle the color from inrunStairsColor
            if (ColorUtility.TryParseHtmlString(hill.inrunStairsColor, out Color stairsColor))
            {
                Color originalColor = material.GetColor("_BaseColor"); // Preserve the original alpha channel
                stairsColor.a = originalColor.a;  // Retain the original alpha transparency
                material.SetColor("_BaseColor", stairsColor);  // Set the color to the material
                Debug.Log("Assigned color: " + hill.inrunStairsColor);
            }
            else
            {
                Debug.LogError("Invalid color string: " + hill.inrunStairsColor);
            }

            // Apply the material to the MeshRenderer
            gateStairs.gObj.GetComponent<MeshRenderer>().material = material;
        }


        public void GenerateInrunStairs(ModelData inrunStairs, int side, bool generate, bool generate2, int stepsNumber)
        {
            /* 0 - Left, 1 - Right */
            var mesh = new Mesh();
            var verticesList = new List<Vector3>();
            var uvsList = new List<Vector2>();
            var trianglesList = new List<int>();

            if (generate)
            {
                var width = 0.7f;
                var heightDiff = (generate2 ? hill.B.y - hill.T.y : hill.A.y - hill.T.y);
                // Debug.Log(heightDiff);
                var stepHeight = heightDiff / stepsNumber;
                var offset = ((side == 1) ? (width + profileData.Value.b1) : 0);
                var it = 0;
                var pos = new Vector2[stepsNumber + 1];
                for (var i = 0; i < pos.Length; i++)
                {
                    while (it < hill.inrunPoints.Length - 2 && (hill.inrunPoints[it + 1].y < i * stepHeight)) it++;

                    pos[i] = new Vector2(
                        (stepHeight * i - hill.inrunPoints[it].y) /
                        (hill.inrunPoints[it + 1].y - hill.inrunPoints[it].y) *
                        (hill.inrunPoints[it + 1].x - hill.inrunPoints[it].x) + hill.inrunPoints[it].x, stepHeight * i);
                }

                var b = profileData.Value.b1 / 2;

                for (var i = 0; i < pos.Length - 1; i++)
                {
                    verticesList.Add(new Vector3(pos[i].x, pos[i].y, -(b + width) + offset));
                    uvsList.Add(new Vector2(0, 0));
                    verticesList.Add(new Vector3(pos[i].x, pos[i].y, -b + offset));
                    uvsList.Add(new Vector2(0, width));

                    float deltaY = pos[i + 1].y - pos[i].y, deltaX = pos[i + 1].x - pos[i].x;
                    verticesList.Add(new Vector3(pos[i].x, pos[i + 1].y, -(b + width) + offset));
                    uvsList.Add(new Vector2(deltaY, 0));
                    verticesList.Add(new Vector3(pos[i].x, pos[i + 1].y, -b + offset));
                    uvsList.Add(new Vector2(deltaY, width));


                    verticesList.Add(new Vector3(pos[i].x, pos[i + 1].y, -(b + width) + offset));
                    uvsList.Add(new Vector2(deltaY, 0));
                    verticesList.Add(new Vector3(pos[i].x, pos[i + 1].y, -b + offset));
                    uvsList.Add(new Vector2(deltaY, width));

                    verticesList.Add(new Vector3(pos[i + 1].x, pos[i + 1].y, -(b + width) + offset));
                    verticesList.Add(new Vector3(pos[i + 1].x, pos[i + 1].y, -b + offset));
                    uvsList.Add(new Vector2(deltaY + deltaX, 0));
                    uvsList.Add(new Vector2(deltaY + deltaX, width));

                    var x = verticesList.Count;
                    trianglesList.Add(x - 2);
                    trianglesList.Add(x - 1);
                    trianglesList.Add(x - 4);

                    trianglesList.Add(x - 1);
                    trianglesList.Add(x - 3);
                    trianglesList.Add(x - 4);

                    trianglesList.Add(x - 6);
                    trianglesList.Add(x - 5);
                    trianglesList.Add(x - 8);

                    trianglesList.Add(x - 5);
                    trianglesList.Add(x - 7);
                    trianglesList.Add(x - 8);
                }

                for (var i = 0; i < pos.Length - 1; i++)
                {
                    if (i > 0)
                    {
                        float deltaY = pos[i].y - pos[i - 1].y, deltaX = pos[i].x - pos[i - 1].x;
                        verticesList.Add(new Vector3(pos[i - 1].x, pos[i].y, -(b + width) + offset));
                        uvsList.Add(new Vector2(pos[i - 1].x, pos[i].y));
                        verticesList.Add(new Vector3(pos[i - 1].x, pos[i].y, -b + offset));
                        uvsList.Add(new Vector2(pos[i - 1].x, pos[i].y));
                    }


                    verticesList.Add(new Vector3(pos[i].x, pos[i].y, -(b + width) + offset));
                    uvsList.Add(new Vector2(pos[i].x, pos[i].y));
                    verticesList.Add(new Vector3(pos[i].x, pos[i].y, -b + offset));
                    uvsList.Add(new Vector2(pos[i].x, pos[i].y));

                    var x = verticesList.Count;
                    if (i > 0)
                    {
                        trianglesList.Add(x - 5);
                        trianglesList.Add(x - 3);
                        trianglesList.Add(x - 1);

                        trianglesList.Add(x - 6);
                        trianglesList.Add(x - 2);
                        trianglesList.Add(x - 4);
                    }
                }
            }

            var vertices = verticesList.ToArray();

            var uvs = uvsList.ToArray();
            var triangles = trianglesList.ToArray();
            ObjectUpdate(inrunStairs.gObj, mesh, inrunStairs.materials[0], vertices, triangles, uvs, false);
        }

        public void GenerateLandingArea()
        {
            var mesh = landingAreaSO.Generate(hill.landingAreaPoints, hill.w, hill.l1, hill.l2, hill.b2, hill.bK,
                hill.bU, hill.P, hill.K, hill.L, hill.U, hill.landingAreaData);
            landingArea.gObj.GetComponent<MeshFilter>().mesh = mesh;
            landingArea.gObj.GetComponent<MeshRenderer>().materials = landingAreaSO.GetMaterials();
        }

        public void GenerateMarks()
        {
            var mesh = new Mesh();
            var verticesList = new List<Vector3>();
            var uvsList = new List<Vector2>();
            var facesList = new List<(int, int, int, int)>();
            var b = new float[hill.landingAreaPoints.Length];

            int pLen = Mathf.RoundToInt(hill.w - hill.l1),
                kLen = Mathf.RoundToInt(hill.w),
                lLen = Mathf.RoundToInt(hill.w + hill.l2);
            var uLen = 0;
            while ((hill.landingAreaPoints[uLen + 1] - hill.U).magnitude <
                   (hill.landingAreaPoints[uLen] - hill.U).magnitude) uLen++;

            var mn = hill.landingAreaData.metersLow == 0 ? kLen / 2 : hill.landingAreaData.metersLow;
            var mx = Mathf.Min(uLen, lLen + 5);
            mx = hill.landingAreaData.metersHigh == 0 ? mx : hill.landingAreaData.metersHigh;


            for (var i = mn; i <= mx; i++)
            {
                b[i] = hill.landingAreaPoints[i].x <= hill.K.x
                    ? (profileData.Value.b2 / 2) + hill.landingAreaPoints[i].x / hill.K.x *
                    ((profileData.Value.bK - profileData.Value.b2) / 2)
                    : hill.landingAreaPoints[i].x >= hill.U.x
                        ? (profileData.Value.bU / 2)
                        : (profileData.Value.bK / 2) + (hill.landingAreaPoints[i].x - hill.K.x) /
                        (hill.U.x - hill.K.x) * ((profileData.Value.bU - profileData.Value.bK) / 2);
            }

            Vector2[] numbersUVs =
            {
                new Vector2(0, 1), new Vector2(0.25f, 1), new Vector2(0.5f, 1), new Vector2(0.75f, 1),
                new Vector2(0, 0.75f), new Vector2(0.25f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.75f, 0.75f),
                new Vector2(0, 0.5f), new Vector2(0.25f, 0.5f)
            };

            const float numberSizeX = 0.15f;
            const float numberSizeY = 0.25f;
            const float numberPlateOffset = 0.02f;

            for (var distNum = mn; distNum < mx; distNum++)
            {
                var num = distNum.ToString();
                for (var side = 0; side < 2; side++)
                {
                    var sgn = 2 * side - 1;
                    var pos = new Vector3(hill.landingAreaPoints[distNum].x, hill.landingAreaPoints[distNum].y + 0.3f,
                        (b[distNum] - numberPlateOffset) * sgn);
                    for (var j = 0; j < num.Length; j++)
                    {
                        var digit = num[j] - '0';

                        var plateAnchorX = numberSizeX * (sgn * (j - 1) + num.Length / 2.0f);
                        verticesList.Add(pos + new Vector3(plateAnchorX, numberSizeY, 0));
                        uvsList.Add(numbersUVs[digit] + new Vector2(0.05f, 0));
                        verticesList.Add(pos + new Vector3(plateAnchorX + sgn * numberSizeX, numberSizeY, 0));
                        uvsList.Add(numbersUVs[digit] + new Vector2(0.20f, 0));
                        verticesList.Add(pos + new Vector3(plateAnchorX, 0, 0));
                        uvsList.Add(numbersUVs[digit] + new Vector2(0.05f, -0.25f));
                        verticesList.Add(pos + new Vector3(plateAnchorX + sgn * numberSizeX, 0, 0));
                        uvsList.Add(numbersUVs[digit] + new Vector2(0.20f, -0.25f));
                        var x = verticesList.Count;
                        facesList.Add((x - 4, x - 3, x - 2, x - 1));
                    }
                }
            }

            var vertices = verticesList.ToArray();
            var triangles = FacesToTriangles(facesList);
            var uvs = uvsList.ToArray();
            ObjectUpdate(digitsMarks.gObj, mesh, digitsMarks.materials[0], vertices, triangles, uvs, false);
        }

        public void GenerateLandingAreaGuardrail(ModelData guardrail, int side, bool generate)
        {
            if (!generate)
            {
                guardrail.gObj.GetComponent<MeshFilter>().mesh = null;
                guardrail.gObj.GetComponent<MeshRenderer>().materials = null;
                return;
            }

            var sgn = (side == 0 ? -1 : 1);
            var points = hill.landingAreaPoints.Select(it => new Vector3(it.x, it.y,
                sgn * (it.x <= hill.K.x ? (profileData.Value.b2 / 2) +
                                          it.x / hill.K.x * ((profileData.Value.bK - profileData.Value.b2) / 2) :
                    it.x >= hill.U.x ? (profileData.Value.bU / 2) :
                    (profileData.Value.bK / 2) + (it.x - hill.K.x) / (hill.U.x - hill.K.x) *
                    ((profileData.Value.bU - profileData.Value.bK) / 2)))).ToArray();

            var mesh = LandingAreaGuardrailSO.Generate(points, side);
            var meshFilter = guardrail.gObj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.mesh = mesh;
            }
            else
            {
                Debug.LogError("No MeshFilter component found on guardrail object.");
                return;
            }

            var meshRenderer = guardrail.gObj.GetComponent<MeshRenderer>();

            // Ensure previous materials are cleared
            meshRenderer.materials = new Material[0]; // Clear all previously assigned materials


            // Get the enum value corresponding to the texture string
            if (System.Enum.TryParse(hill.landingAreaGuardrailTexture, out LandingAreaGuardrailTexture textureEnum))
            {
                int materialIndex = (int)textureEnum;

                // Ensure the index is within bounds
                if (materialIndex >= 0 && materialIndex < landingAreaGuardrailL.materials.Length)
                {
                    var material = landingAreaGuardrailL.materials[materialIndex];
                    meshRenderer.material = material; // Assign only one material

                    // Store the original base color (including alpha) if it hasn't been stored already
                    if (!originalMaterialColors.ContainsKey(material))
                    {
                        originalMaterialColors[material] = material.GetColor("_BaseColor");
                    }

                    // Try to parse the hex color string for the guardrail color
                    if (ColorUtility.TryParseHtmlString(hill.landingAreaGuardrailColor, out Color guardrailColor))
                    {
                        // Preserve the original alpha value
                        Color originalColor = material.GetColor("_BaseColor");
                        guardrailColor.a = originalColor.a;

                        // Apply only the RGB values (preserve alpha)
                        material.SetColor("_BaseColor", guardrailColor);
                    }
                    else
                    {
                        Debug.LogError("Invalid hex color string: " + hill.landingAreaGuardrailColor);
                    }
                }
                else
                {
                    Debug.LogError("Material index out of bounds.");
                }
            }
            else
            {
                Debug.LogError("Invalid texture string from JSON: " + hill.landingAreaGuardrailTexture);
            }
        }

        // Method to revert the material colors (including alpha) when the game stops
        private void OnDisable()
        {
            RestoreOriginalMaterialColors();
        }

        // Method to restore original base map colors (including alpha) of materials
        private void RestoreOriginalMaterialColors()
        {
            foreach (var entry in originalMaterialColors)
            {
                Material material = entry.Key;
                Color originalColor = entry.Value;
                material.SetColor("_BaseColor", originalColor);
            }

            // Clear the dictionary after restoration
            originalMaterialColors.Clear();
        }
    

    public void GenerateInrunGuardrail(ModelData guardrail, int side, bool generate)
        {
            if (!generate)
            {
                guardrail.gObj.GetComponent<MeshFilter>().mesh = null;
                guardrail.gObj.GetComponent<MeshRenderer>().material = null;
                return;
            }

            var sgn = (side == 0 ? -1 : 1);
            var points = hill.inrunPoints.Where(it => it.x >= hill.B.x)
                .Select(it => new Vector3(it.x, it.y, sgn * (hill.b1 / 2 - 2 * inrunGuardrailSO.Width))).Reverse()
                .ToArray();

            var mesh = inrunGuardrailSO.Generate(points, side);
            guardrail.gObj.GetComponent<MeshFilter>().mesh = mesh;
            guardrail.gObj.GetComponent<MeshRenderer>().material = inrunGuardrailSO.GetMaterial();


            if (System.Enum.TryParse(hill.inrunGuardrailTexture, out InrunGuardrailTexture textureEnum))
            {
                int materialIndex = (int)textureEnum;

                // Ensure the index is within bounds
                if (materialIndex >= 0 && materialIndex < inrunGuardrailL.materials.Length)
                {
                    // Get the current array of materials assigned to the MeshRenderer
                    var materials = guardrail.gObj.GetComponent<MeshRenderer>().materials;

                    // Ensure we're not out of bounds when assigning the material
                    if (materials.Length > 0)
                    {
                        materials[0] = inrunGuardrailL.materials[materialIndex]; // Replace the first material in the array
                        guardrail.gObj.GetComponent<MeshRenderer>().materials = materials; // Apply the modified array back
                        Debug.Log("Assigned material with index: " + materialIndex + " landingAreaGuardrailL.materials.Length " + inrunGuardrailL.materials.Length);
                    }
                    else
                    {
                        Debug.LogError("No materials found in MeshRenderer.");
                    }
                }
                else
                {
                    Debug.LogError("Material index out of bounds.");
                }
            }
            else
            {
                Debug.LogError("Invalid texture string from JSON: " + hill.inrunGuardrailTexture);
            }
        
    }

        public void GenerateInrunOuterGuardrail(ModelData guardrail, int side, bool generate, bool generate2)
        {
            if (!generate)
            {
                guardrail.gObj.GetComponent<MeshFilter>().mesh = null;
                guardrail.gObj.GetComponent<MeshRenderer>().material = null;
                return;
            }

            var criticalPointX = Mathf.Lerp(hill.GatePoint(-1).x, hill.T.x, inrunStairsAngle);
            var p1 = hill.inrunPoints.Last(it => it.x > criticalPointX);
            var p2 = hill.inrunPoints.First(it => it.x <= criticalPointX);
            var criticalPoint = Vector2.Lerp(p1, p2, (criticalPointX - p1.x) / (p2.x - p1.x));

            var tmpList = new List<Vector2>();
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x > criticalPoint.x));
            tmpList.Add(criticalPoint);
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x <= criticalPoint.x && it.x > hill.GatePoint(-1).x));
            tmpList.Add(hill.GatePoint(-1));
            tmpList.AddRange(hill.inrunPoints.Where(it => it.x <= hill.GatePoint(-1).x));

            var len = new float[tmpList.Count];
            float[] b;

            if (generate2)
            {
                b = tmpList.Select(it =>
                    (it.x > hill.GatePoint(-1).x
                        ? (it.x > criticalPoint.x
                            ? hill.b1 / 2 + 0.7f
                            : Mathf.Lerp(hill.b1 / 2 + 0.7f, hill.b1 / 2 + gateStairsSO.StepWidth,
                                (it.x - criticalPointX) / (criticalPointX - hill.GatePoint(-1).x)))
                        : (hill.b1 / 2 + gateStairsSO.StepWidth))).ToArray();
            }
            else
            {
                b = tmpList.Select(it => hill.b1 / 2 + 0.7f).ToArray();
            }

            var sgn = (side == 0 ? -1 : 1);
            var points = tmpList.Select((val, ind) => new Vector3(val.x, val.y, sgn * b[ind])).Reverse().ToArray();

            var mesh = InrunOuterGuardrailSO.Generate(points, side);
            var meshFilter = guardrail.gObj.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = guardrail.gObj.GetComponent<MeshRenderer>();

            // Ensure the previous materials are cleared
            meshRenderer.materials = new Material[0]; // Clear all previously assigned materials

            // Get the enum value corresponding to the texture string
            if (System.Enum.TryParse(hill.inrunOuterGuardrailTexture, out InrunOuterGuardrailTexture textureEnum))
            {
                int materialIndex = (int)textureEnum;

                // Ensure the index is within bounds
                if (materialIndex >= 0 && materialIndex < inrunOuterGuardrailL.materials.Length)
                {
                    var material = inrunOuterGuardrailL.materials[materialIndex];
                    meshRenderer.material = material; // Assign only one material

                    // Store the original base color if not already stored
                    if (!originalMaterialColors.ContainsKey(material))
                    {
                        originalMaterialColors[material] = material.GetColor("_BaseColor");
                    }

                    // Handle the "Default" case or other invalid color strings
                    if (hill.inrunOuterGuardrailColor == "Default")
                    {
                        // Set a default color manually (white in this case)
                        material.SetColor("_BaseColor", Color.white);
                    }
                    else if (ColorUtility.TryParseHtmlString(hill.inrunOuterGuardrailColor, out Color guardrailColor))
                    {
                        // Preserve original alpha
                        Color originalColor = material.GetColor("_BaseColor");
                        guardrailColor.a = originalColor.a;

                        // Apply RGB values only (preserving alpha)
                        material.SetColor("_BaseColor", guardrailColor);
                    }
                    else
                    {
                        Debug.LogError("Invalid hex color string: " + hill.inrunOuterGuardrailColor);
                    }
                }
                else
                {
                    Debug.LogError("Material index out of bounds.");
                }
            }
            else
            {
                Debug.LogError("Invalid texture string from JSON: " + hill.inrunOuterGuardrailTexture);
            }
        }

        public void DestroyLamps()
        {
            if (lamps != null)
                foreach (var it in lamps)
                {
                    Destroy(it);
                }

            lamps = new List<GameObject>();
        }

        public void GenerateLamps()
        {
            for (var i = 0; i < hill.inrunPoints.Length; i += 5)
            {
                lamps.Add(Instantiate(inrunLampPrefab, new Vector3(hill.inrunPoints[i].x, hill.inrunPoints[i].y, 2),
                    Quaternion.identity));
                // lamps.Add(Instantiate(inrunLampPrefab, new Vector3(hill.inrunPoints[i].x, hill.inrunPoints[i].y + 1f, -2), Quaternion.identity));
            }

            for (var i = 0; i < hill.landingAreaPoints.Length; i += 80)
            {
                lamps.Add(Instantiate(lampPrefab,
                    new Vector3(hill.landingAreaPoints[i].x, hill.landingAreaPoints[i].y, 45), Quaternion.identity));
            }
        }


        public void SaveMesh(GameObject gObj, string name, bool isCollider = false)
        {
#if UNITY_EDITOR
            var saveName = profileData.Value.name + "_" + name;
            if (isCollider)
            {
                var mc = gObj.GetComponent<MeshCollider>();
                if (mc)
                {
                    var savePath = "Assets/" + saveName + ".asset";
                    Debug.Log("Saved Mesh to:" + savePath);
                    AssetDatabase.CreateAsset(mc.sharedMesh, savePath);
                }
            }
            else
            {
                var mf = gObj.GetComponent<MeshFilter>();
                if (mf)
                {
                    var savePath = "Assets/" + saveName + ".asset";
                    Debug.Log("Saved Mesh to:" + savePath);
                    AssetDatabase.CreateAsset(mf.mesh, savePath);
                }
            }

#endif
        }
    }
}