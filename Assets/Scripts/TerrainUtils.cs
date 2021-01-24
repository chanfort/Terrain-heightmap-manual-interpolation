using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public class TerrainUtils
{
    public static Vector3 TerrainVectorUnity(Vector3 origin, Terrain ter1)
    {
        if (ter1 == null)
        {
            return origin;
        }

        Vector3 planeVect = new Vector3(origin.x, 0f, origin.z);
        float y1 = ter1.SampleHeight(planeVect);

        y1 = y1 + ter1.transform.position.y;

        Vector3 tv = new Vector3(origin.x, y1, origin.z);
        return tv;
    }

    public static Vector3 TerrainVectorBilinear(Vector3 origin, float[,] heightmap, float xbeg, float zbeg, float yoffset, float length, float height, int res)
    {
        if (heightmap == null)
        {
            return origin;
        }

        float y = GetTerrainHeightBilinear(
            heightmap,
            xbeg,
            zbeg,
            length,
            origin.x,
            origin.z,
            height,
            res
        );

        y = y + yoffset;
        Vector3 tv = new Vector3(origin.x, y, origin.z);

        return tv;
    }

    public static Vector3 TerrainVectorInlined(Vector3 origin, float[,] heightmap, float xbeg, float zbeg, float yoffset, float length, float height, int res)
    {
        if (heightmap == null)
        {
            return origin;
        }

        float y = GetTerrainHeightInlined(
            heightmap,
            xbeg,
            zbeg,
            length,
            origin.x,
            origin.z,
            height,
            res
        );

        y = y + yoffset;
        Vector3 tv = new Vector3(origin.x, y, origin.z);

        return tv;
    }

    public static Vector3 TerrainVectorNativeArray(Vector3 origin, NativeArray<float> heightmap, float xbeg, float zbeg, float yoffset, float length, float height, int res)
    {
        float y = GetTerrainHeightNativeArray(
            heightmap,
            xbeg,
            zbeg,
            length,
            origin.x,
            origin.z,
            height,
            res
        );

        y = y + yoffset;
        Vector3 tv = new Vector3(origin.x, y, origin.z);

        return tv;
    }

    public static Vector3 TerrainVectorBarycentric(Vector3 origin, NativeArray<float> heightmap, float xbeg, float zbeg, float yoffset, float length, float height, int res)
    {
        float y = GetTerrainHeightBarycentric(
            heightmap,
            xbeg,
            zbeg,
            length,
            origin.x,
            origin.z,
            height,
            res
        );

        y = y + yoffset;
        Vector3 tv = new Vector3(origin.x, y, origin.z);

        return tv;
    }

    public static float GetTerrainHeightBilinear(float[,] heightmap, float xbeg, float ybeg, float world_size, float world_point_x, float world_point_y, float height, int res)
    {
        float x = ((world_point_y - ybeg) * res) / world_size;
        float z = ((world_point_x - xbeg) * res) / world_size;

        int fx = (int)math.floor(x);
        int cx = (int)math.ceil(x);
        int fz = (int)math.floor(z);
        int cz = (int)math.ceil(z);

        float hx0z0 = heightmap[fx, fz] * height; // known height (x0, z0)
        float hx1z0 = heightmap[cx, fz] * height; // known height (x1, z0)
        float hx0z1 = heightmap[fx, cz] * height; // known height (x0, z1)
        float hx1z1 = heightmap[cx, cz] * height; // known height (x1, z1)

        float u0v0 = hx0z0 * (cx - x) * (cz - z); // interpolated (x0, z0)
        float u1v0 = hx1z0 * (x - fx) * (cz - z); // interpolated (x1, z0)
        float u0v1 = hx0z1 * (cx - x) * (z - fz); // interpolated (x0, z1)
        float u1v1 = hx1z1 * (x - fx) * (z - fz); // interpolated (x1, z1)

        float h = u0v0 + u1v0 + u0v1 + u1v1;

        return h;
    }

    public static float GetTerrainHeightInlined(float[,] heightmap, float xbeg, float ybeg, float world_size, float world_point_x, float world_point_y, float height, int res)
    {
        float x = ((world_point_y - ybeg) * res) / world_size;
        float z = ((world_point_x - xbeg) * res) / world_size;

        int fx = (int)math.floor(x);
        int cx = (int)math.ceil(x);
        int fz = (int)math.floor(z);
        int cz = (int)math.ceil(z);

        float hx0z0 = heightmap[fx, fz] * height;
        float hx1z0 = heightmap[cx, fz] * height;
        float hx0z1 = heightmap[fx, cz] * height;
        float hx1z1 = heightmap[cx, cz] * height;

        float h = hx0z0 + (hx1z0 - hx0z0) * (x - (int)x) + (hx0z1 - hx0z0) * (z - (int)z) + (hx0z0 - hx1z0 - hx0z1 + hx1z1) * (x - (int)x) * (z - (int)z);

        return h;
    }

    public static float GetTerrainHeightNativeArray(NativeArray<float> heightmap, float xbeg, float ybeg, float world_size, float world_point_x, float world_point_y, float height, int res)
    {
        float x = ((world_point_y - ybeg) * res) / world_size;
        float z = ((world_point_x - xbeg) * res) / world_size;

        int fx = (int)math.floor(x);
        int cx = (int)math.ceil(x);
        int fz = (int)math.floor(z);
        int cz = (int)math.ceil(z);

        int nRes = res + 1;

        float hx0z0 = heightmap[fx * nRes + fz] * height;
        float hx1z0 = heightmap[cx * nRes + fz] * height;
        float hx0z1 = heightmap[fx * nRes + cz] * height;
        float hx1z1 = heightmap[cx * nRes + cz] * height;

        return hx0z0 + (hx1z0 - hx0z0) * (x - (int)x) + (hx0z1 - hx0z0) * (z - (int)z) + (hx0z0 - hx1z0 - hx0z1 + hx1z1) * (x - (int)x) * (z - (int)z);
    }

    public static float GetTerrainHeightBarycentric(NativeArray<float> heightmap, float xbeg, float ybeg, float world_size, float world_point_x, float world_point_y, float height, int res)
    {
        float x = ((world_point_y - ybeg) * res) / world_size;
        float z = ((world_point_x - xbeg) * res) / world_size;

        int fx = (int)math.floor(x);
        int cx = fx + 1;
        int fz = (int)math.floor(z);
        int cz = fz + 1;

        int nRes = res + 1;

        float hx0z0 = heightmap[fx * nRes + fz] * height;
        float hx1z0 = heightmap[cx * nRes + fz] * height;
        float hx0z1 = heightmap[fx * nRes + cz] * height;
        float hx1z1 = heightmap[cx * nRes + cz] * height;

        float dx = x - fx;
        float dy = z - fz;

        float h = 0;
        if (dx > dy)
        {
            h = Barycentric(fx, cx, cx, x, fz, fz, cz, z, hx0z0, hx1z0, hx1z1);
        }
        else
        {
            h = Barycentric(fx, fx, cx, x, fz, cz, cz, z, hx0z0, hx0z1, hx1z1);
        }

        return h;
    }

    public static float Barycentric(
        float x1,
        float x2,
        float x3,
        float x,

        float z1,
        float z2,
        float z3,
        float z,

        float y1,
        float y2,
        float y3
    )
    {
        float ax = x2 - x3;
        float bx = x1 - x3;
        float cx = x - x3;

        float az = z2 - z3;
        float bz = z1 - z3;
        float cz = z - z3;

        float aLen = ax * ax + az * az;
        float bLen = bx * bx + bz * bz;

        float ab = ax * bx + az * bz;
        float ac = ax * cx + az * cz;
        float bc = bx * cx + bz * cz;

        float d = aLen * bLen - ab * ab;

        float u = (aLen * bc - ab * ac) / d;
        float v = (bLen * ac - ab * bc) / d;
        float w = 1.0f - u - v;

        return y1 * u + y2 * v + y3 * w;
    }

    public static float TerrainSteepnessUnity(TerrainData terrainData, float3 p, float3 offset)
    {
        p -= offset;
        float xn = p.x / terrainData.size.x;
        float zn = p.z / terrainData.size.z;
        return terrainData.GetSteepness(xn, zn);
    }

    public static float GetTerrainSteepnessTriangle(NativeArray<float> heightmap, float xbeg, float ybeg, float world_size, float world_point_x, float world_point_y, float height, int res)
    {
        float x = ((world_point_y - ybeg) * res) / world_size;
        float z = ((world_point_x - xbeg) * res) / world_size;

        int fx = (int)math.floor(x);
        int cx = fx + 1;
        int fz = (int)math.floor(z);
        int cz = fz + 1;

        int nRes = res + 1;

        float hx0z0 = heightmap[fx * nRes + fz] * height;
        float hx1z0 = heightmap[cx * nRes + fz] * height;
        float hx0z1 = heightmap[fx * nRes + cz] * height;
        float hx1z1 = heightmap[cx * nRes + cz] * height;

        float dx = x - fx;
        float dy = z - fz;

        float steepness = 0;

        if (dx > dy)
        {
            float3 a = new float3(fx * world_size / res, hx0z0, fz * world_size / res);
            float3 b = new float3(cx * world_size / res, hx1z0, fz * world_size / res);
            float3 c = new float3(cx * world_size / res, hx1z1, cz * world_size / res);

            float3 n = math.normalize(math.cross(b - a, c - a));
            steepness = math.acos(math.dot(n, new float3(0f, -1f, 0f))) * 57.29578f;
        }
        else
        {
            float3 a = new float3(fx * world_size / res, hx0z0, fz * world_size / res);
            float3 b = new float3(cx * world_size / res, hx1z1, cz * world_size / res);
            float3 c = new float3(fx * world_size / res, hx0z1, cz * world_size / res);

            float3 n = math.normalize(math.cross(b - a, c - a));
            steepness = math.acos(math.dot(n, new float3(0f, -1f, 0f))) * 57.29578f;
        }

        return steepness;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<float> ManagedToNativeHeightmap(float[,] heightmapManaged)
    {
        int n = heightmapManaged.GetLength(0);
        NativeArray<float> heightmapNative = new NativeArray<float>(n * n, Allocator.Persistent);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                heightmapNative[i * n + j] = heightmapManaged[i, j];
            }
        }

        return heightmapNative;
    }
}
