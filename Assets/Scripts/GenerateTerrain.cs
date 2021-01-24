using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    public int resolution = 512;
    public float size = 3000f;
    public float maxHeight = 1000f;
    public float noiseSize = 500f;
    public int numberOfPoints = 1000000;
    public uint seed = 1;

    public KeyCode repeatTestKey = KeyCode.T;

    float[,] heightmap;
    Terrain terrain;
    TerrainData terrainData;

    void Start()
    {
        Generate();
        HeightsTest();
    }

    void Update()
    {
        if (Input.GetKeyDown(repeatTestKey))
        {
            HeightsTest();
        }
    }

    void HeightsTest()
    {
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        float3[] positions = new float3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            positions[i] = (new Vector3(r.NextFloat(-size / 2f, size / 2f), 0f, r.NextFloat(-size / 2f, size / 2f)));
        }

        NativeArray<float> nativeHeightmap = TerrainUtils.ManagedToNativeHeightmap(heightmap);
        NativeArray<float3> nativePositions = new NativeArray<float3>(positions, Allocator.Persistent);
        NativeArray<float> yBarycentricJob = new NativeArray<float>(numberOfPoints, Allocator.Persistent);
        NativeArray<float> steepnessJob = new NativeArray<float>(numberOfPoints, Allocator.Persistent);

        float[] yUnity = new float[numberOfPoints];
        float[] yBilinear = new float[numberOfPoints];
        float[] yInlined = new float[numberOfPoints];
        float[] yNativeArray = new float[numberOfPoints];
        float[] yBarycentric = new float[numberOfPoints];
        float[] steepnessUnity = new float[numberOfPoints];

        // Unity;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < numberOfPoints; i++)
        {
            yUnity[i] = TerrainUtils.TerrainVectorUnity(positions[i], terrain).y;
        }

        double tUnity = stopwatch.Elapsed.TotalMilliseconds;

        // Bilinear;
        for (int i = 0; i < numberOfPoints; i++)
        {
            yBilinear[i] = TerrainUtils.TerrainVectorBilinear(positions[i], heightmap, -size / 2f, -size / 2f, 0f, size, maxHeight, resolution).y;
        }

        double tBilinear = stopwatch.Elapsed.TotalMilliseconds;

        // Inlined;
        for (int i = 0; i < numberOfPoints; i++)
        {
            yInlined[i] = TerrainUtils.TerrainVectorInlined(positions[i], heightmap, -size / 2f, -size / 2f, 0f, size, maxHeight, resolution).y;
        }

        double tInlined = stopwatch.Elapsed.TotalMilliseconds;

        // Native array;
        for (int i = 0; i < numberOfPoints; i++)
        {
            yNativeArray[i] = TerrainUtils.TerrainVectorNativeArray(positions[i], nativeHeightmap, -size / 2f, -size / 2f, 0f, size, maxHeight, resolution).y;
        }

        double tNativeArray = stopwatch.Elapsed.TotalMilliseconds;

        // Native array;
        for (int i = 0; i < numberOfPoints; i++)
        {
            yBarycentric[i] = TerrainUtils.TerrainVectorBarycentric(positions[i], nativeHeightmap, -size / 2f, -size / 2f, 0f, size, maxHeight, resolution).y;
        }

        double tBarycentric = stopwatch.Elapsed.TotalMilliseconds;

        TerrainVectorBarycentricJob terrainVectorBarycentricJob = new TerrainVectorBarycentricJob
        {
            positions = nativePositions,
            nativeHeightmap = nativeHeightmap,
            size = size,
            maxHeight = maxHeight,
            res = resolution,
            results = yBarycentricJob
        };

        terrainVectorBarycentricJob.Schedule(numberOfPoints, System.Environment.ProcessorCount).Complete();
        double tBarycentricJob = stopwatch.Elapsed.TotalMilliseconds;

        // Steepness Unity
        for (int i = 0; i < numberOfPoints; i++)
        {
            steepnessUnity[i] = TerrainUtils.TerrainSteepnessUnity(terrainData, positions[i], terrain.transform.position);
        }

        double tSteepnessUnity = stopwatch.Elapsed.TotalMilliseconds;

        // Steepness manual
        TerrainSteepnessJob terrainSteepnessJob = new TerrainSteepnessJob
        {
            positions = nativePositions,
            nativeHeightmap = nativeHeightmap,
            size = size,
            maxHeight = maxHeight,
            res = resolution,
            results = steepnessJob
        };

        terrainSteepnessJob.Schedule(numberOfPoints, System.Environment.ProcessorCount).Complete();
        double tSteepnessJob = stopwatch.Elapsed.TotalMilliseconds;

        float maxDiffBilinear = 0f;
        float maxDiffInlined = 0f;
        float maxDiffNativeArray = 0f;
        float maxDiffBarycentric = 0f;
        float maxDiffBarycentricJob = 0f;
        float maxDiffSteepnessJob = 0f;

        float maxDiffBilinearFrac = 0f;
        float maxDiffInlinedFrac = 0f;
        float maxDiffNativeArrayFrac = 0f;
        float maxDiffBarycentricFrac = 0f;
        float maxDiffBarycentricJobFrac = 0f;
        float maxDiffSteepnessJobFrac = 0f;

        for (int i = 0; i < numberOfPoints; i++)
        {
            maxDiffBilinear = math.max(maxDiffBilinear, math.abs(yBilinear[i] - yUnity[i]));
            maxDiffInlined = math.max(maxDiffInlined, math.abs(yInlined[i] - yUnity[i]));
            maxDiffNativeArray = math.max(maxDiffNativeArray, math.abs(yNativeArray[i] - yUnity[i]));
            maxDiffBarycentric = math.max(maxDiffBarycentric, math.abs(yBarycentric[i] - yUnity[i]));
            maxDiffBarycentricJob = math.max(maxDiffBarycentricJob, math.abs(yBarycentricJob[i] - yUnity[i]));
            maxDiffSteepnessJob = math.max(maxDiffSteepnessJob, math.abs(steepnessJob[i] - steepnessUnity[i]));

            maxDiffBilinearFrac = math.max(maxDiffBilinearFrac, math.abs(yBilinear[i] - yUnity[i]) / yUnity[i]);
            maxDiffInlinedFrac = math.max(maxDiffInlinedFrac, math.abs(yInlined[i] - yUnity[i]) / yUnity[i]);
            maxDiffNativeArrayFrac = math.max(maxDiffNativeArrayFrac, math.abs(yNativeArray[i] - yUnity[i]) / yUnity[i]);
            maxDiffBarycentricFrac = math.max(maxDiffBarycentricFrac, math.abs(yBarycentric[i] - yUnity[i]) / yUnity[i]);
            maxDiffBarycentricJobFrac = math.max(maxDiffBarycentricJobFrac, math.abs(yBarycentric[i] - yUnity[i]) / yUnity[i]);
            maxDiffSteepnessJobFrac = math.max(maxDiffSteepnessJobFrac, math.abs(steepnessJob[i] - steepnessUnity[i]) / 90f);
        }

        UnityEngine.Debug.Log($"Setup : resolution={resolution} size={size} maxHeight={maxHeight} numberOfPoints={numberOfPoints} noiseSize={noiseSize}");
        UnityEngine.Debug.Log($"Unity : dt={tUnity}ms");
        UnityEngine.Debug.Log($"Bilinear : diff={maxDiffBilinear} frac={maxDiffBilinearFrac * 100f}% dt={tBilinear - tUnity}ms");
        UnityEngine.Debug.Log($"Inlined : diff={maxDiffInlined} frac={maxDiffInlinedFrac * 100f}% dt={tInlined - tBilinear}ms");
        UnityEngine.Debug.Log($"NativeArray : diff={maxDiffNativeArray} frac={maxDiffNativeArrayFrac * 100f}% dt={tNativeArray - tInlined}ms");
        UnityEngine.Debug.Log($"Barycentric : diff={maxDiffBarycentric} frac={maxDiffBarycentricFrac * 100f}% dt={tBarycentric - tNativeArray}ms");
        UnityEngine.Debug.Log($"Barycentric Job : diff={maxDiffBarycentricJob} frac={maxDiffBarycentricJobFrac * 100f}% dt={tBarycentricJob - tBarycentric}ms");
        UnityEngine.Debug.Log($"Steepness Unity : dt={tSteepnessUnity - tBarycentricJob}ms");
        UnityEngine.Debug.Log($"Steepness Job : diff={maxDiffSteepnessJob} frac={maxDiffSteepnessJobFrac * 100f}% dt={tSteepnessJob - tSteepnessUnity}ms");

        nativeHeightmap.Dispose();
        nativePositions.Dispose();
        yBarycentricJob.Dispose();
        steepnessJob.Dispose();
    }

    [BurstCompile]
    public struct TerrainVectorBarycentricJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float> nativeHeightmap;
        [ReadOnly] public float size;
        [ReadOnly] public float maxHeight;
        [ReadOnly] public int res;
        [WriteOnly] public NativeArray<float> results;

        public void Execute(int i)
        {
            results[i] = TerrainUtils.TerrainVectorBarycentric(positions[i], nativeHeightmap, -size / 2f, -size / 2f, 0f, size, maxHeight, res).y;
        }
    }

    [BurstCompile]
    public struct TerrainSteepnessJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float> nativeHeightmap;
        [ReadOnly] public float size;
        [ReadOnly] public float maxHeight;
        [ReadOnly] public int res;
        [WriteOnly] public NativeArray<float> results;

        public void Execute(int i)
        {
            float3 pos = positions[i];
            results[i] = TerrainUtils.GetTerrainSteepnessTriangle(nativeHeightmap, -size / 2f, -size / 2f, size, pos.x, pos.z, maxHeight, res);
        }
    }

    void Generate()
    {
        // TerrainData
        terrainData = new TerrainData();
        terrainData.name = "Terrain";
        terrainData.heightmapResolution = resolution;
        terrainData.alphamapResolution = resolution;
        terrainData.size = new Vector3(size, maxHeight, size);

        // Terrain GameObject
        GameObject terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainGameObject.transform.position = new Vector3(-size / 2f, 0f, -size / 2f);
        terrain = terrainGameObject.GetComponent<Terrain>();
        terrain.drawInstanced = true;
        terrainGameObject.name = "Terrain";

        // Heightmap
        heightmap = GetHeightmap(resolution + 1, noiseSize);
        terrainData.SetHeights(0, 0, heightmap);
    }

    float[,] GetHeightmap(int res, float size)
    {
        float[,] heghts = new float[res, res];

        for (int i = 0; i < res; i++)
        {
            for (int j = 0; j < res; j++)
            {
                float2 pos = new float2(1f * i / size, 1f * j / size);
                heghts[i, j] = NoiseExtensions.SNoise(pos, 1f, 2f, 0.5f, 6, 1f, new float2(0, 0));
            }
        }

        return heghts;
    }
}
