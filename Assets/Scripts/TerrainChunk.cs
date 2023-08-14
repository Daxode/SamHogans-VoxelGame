using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    public void BuildMesh()
    {
        Mesh mesh = new Mesh();

        NativeList<Vector3> verts = new NativeList<Vector3>(Allocator.Temp);
        NativeList<int> tris = new NativeList<int>(Allocator.Temp);
        NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Temp);

        for(int x = 1; x < chunkWidth + 1; x++)
            for(int z = 1; z < chunkWidth + 1; z++)
                for(int y = 0; y < chunkHeight; y++)
                {
                    if(blocks[GetArrayIndex(x, y, z)] != BlockType.Air)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;

                        var currentBlock = Block.blocks[blocks[GetArrayIndex(x, y, z)]];
                        
                        //no land above, build top face
                        if(y < chunkHeight - 1 && blocks[GetArrayIndex(x, y + 1, z)] == BlockType.Air)
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
                        if(y > 0 && blocks[GetArrayIndex(x, y - 1, z)] == BlockType.Air)
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
                        if(blocks[GetArrayIndex(x, y, z - 1)] == BlockType.Air)
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
                        if(blocks[GetArrayIndex(x + 1, y, z)] == BlockType.Air)
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
                        if(blocks[GetArrayIndex(x, y, z + 1)] == BlockType.Air)
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
                        if(blocks[GetArrayIndex(x - 1, y, z)] == BlockType.Air)
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
                        for(int i = 0; i < numFaces; i++)
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

        mesh.SetVertices(verts.AsArray());
        mesh.SetUVs(0, uvs.AsArray());
        mesh.SetIndices(tris.AsArray(), MeshTopology.Triangles, 0);

        verts.Dispose();
        uvs.Dispose();
        tris.Dispose();

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    void AddSquare(List<Vector3> verts, List<int> tris)
    {

    }
}
