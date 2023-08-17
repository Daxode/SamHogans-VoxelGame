using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    //chunk size
    public const int chunkWidth = 16;
    public const int chunkHeight = 64;

    //0 = air, 1 = land
    public NativeArray<BlockType> blocks;
    
    void Awake()
    {
        blocks = new NativeArray<BlockType>((chunkWidth + 2) * chunkHeight * (chunkWidth + 2), Allocator.Persistent);
    }

    public static int GetArrayIndex(int x, int y, int z)
    {
        return x * chunkHeight * (chunkWidth + 2) + y * (chunkWidth + 2) + z;
    }

    public void BuildMesh(JobHandle blocksWriteJobHandle)
    {
        Mesh mesh = new Mesh();
        
        var fillMeshJob = new FillMeshDataJob { 
            blocks = blocks, 
            verts = new NativeList<Vector3>(Allocator.TempJob), 
            tris = new NativeList<int>(Allocator.TempJob), 
            uvs = new NativeList<Vector2>(Allocator.TempJob)
        };
        var fillMeshJobHandle = fillMeshJob.Schedule(blocksWriteJobHandle);
        fillMeshJobHandle.Complete();

        mesh.SetVertices(fillMeshJob.verts.AsArray());
        mesh.SetUVs(0, fillMeshJob.uvs.AsArray());
        mesh.SetIndices(fillMeshJob.tris.AsArray(), MeshTopology.Triangles, 0);

        fillMeshJob.verts.Dispose();
        fillMeshJob.uvs.Dispose();
        fillMeshJob.tris.Dispose();
        
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
    [BurstCompile]
    struct FillMeshDataJob : IJob
    {
        public NativeArray<BlockType> blocks;
        public NativeList<Vector3> verts;
        public NativeList<int> tris;
        public NativeList<Vector2> uvs;
        
        public void Execute()
        {
            for (int x = 1; x < chunkWidth + 1; x++)
            for (int z = 1; z < chunkWidth + 1; z++)
            for (int y = 0; y < chunkHeight; y++)
            {
                if (blocks[GetArrayIndex(x, y, z)] != BlockType.Air)
                {
                    Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                    int numFaces = 0;

                    var currentBlock = Block.blocks[blocks[GetArrayIndex(x, y, z)]];

                    //no land above, build top face
                    if (y < chunkHeight - 1 && blocks[GetArrayIndex(x, y + 1, z)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        numFaces++;

                        uvs.Add(currentBlock.topPos.uv0);
                        uvs.Add(currentBlock.topPos.uv1);
                        uvs.Add(currentBlock.topPos.uv2);
                        uvs.Add(currentBlock.topPos.uv3);
                    }

                    
                    //bottom
                    if (y > 0 && blocks[GetArrayIndex(x, y - 1, z)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        numFaces++;

                        uvs.Add(currentBlock.bottomPos.uv0);
                        uvs.Add(currentBlock.bottomPos.uv1);
                        uvs.Add(currentBlock.bottomPos.uv2);
                        uvs.Add(currentBlock.bottomPos.uv3);
                    }

                    //front
                    if (blocks[GetArrayIndex(x, y, z - 1)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        numFaces++;

                        uvs.Add(currentBlock.sidePos.uv0);
                        uvs.Add(currentBlock.sidePos.uv1);
                        uvs.Add(currentBlock.sidePos.uv2);
                        uvs.Add(currentBlock.sidePos.uv3);
                    }

                    //right
                    if (blocks[GetArrayIndex(x + 1, y, z)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        numFaces++;

                        uvs.Add(currentBlock.sidePos.uv0);
                        uvs.Add(currentBlock.sidePos.uv1);
                        uvs.Add(currentBlock.sidePos.uv2);
                        uvs.Add(currentBlock.sidePos.uv3);
                    }

                    //back
                    if (blocks[GetArrayIndex(x, y, z + 1)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        numFaces++;

                        uvs.Add(currentBlock.sidePos.uv0);
                        uvs.Add(currentBlock.sidePos.uv1);
                        uvs.Add(currentBlock.sidePos.uv2);
                        uvs.Add(currentBlock.sidePos.uv3);
                    }

                    //left
                    if (blocks[GetArrayIndex(x - 1, y, z)] == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        numFaces++;

                        uvs.Add(currentBlock.sidePos.uv0);
                        uvs.Add(currentBlock.sidePos.uv1);
                        uvs.Add(currentBlock.sidePos.uv2);
                        uvs.Add(currentBlock.sidePos.uv3);
                    }

                    int tl = verts.Length - 4 * numFaces;
                    for (int i = 0; i < numFaces; i++)
                    {
                        tris.Add(tl + i * 4);
                        tris.Add(tl + i * 4 + 1);
                        tris.Add(tl + i * 4 + 2);
                        tris.Add(tl + i * 4);
                        tris.Add(tl + i * 4 + 2);
                        tris.Add(tl + i * 4 + 3);
                    }
                }
            }
        }
    }

    void AddSquare(List<Vector3> verts, List<int> tris)
    {

    }
}
