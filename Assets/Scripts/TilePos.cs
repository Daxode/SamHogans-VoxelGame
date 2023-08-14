using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TilePos
{
    int xPos, yPos;

    public Vector2 uv0 { get; private set; }
    public Vector2 uv1 { get; private set; }
    public Vector2 uv2 { get; private set; }
    public Vector2 uv3 { get; private set; }

    public TilePos(int xPos, int yPos)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        uv0 = new Vector2(xPos / 16f + .001f, yPos / 16f + .001f);
        uv1 = new Vector2(xPos / 16f + .001f, (yPos + 1) / 16f - .001f);
        uv2 = new Vector2((xPos + 1) / 16f - .001f, (yPos + 1) / 16f - .001f);
        uv3 = new Vector2((xPos + 1) / 16f - .001f, yPos / 16f + .001f);
    }

    public static Dictionary<Tile, TilePos> tiles = new Dictionary<Tile, TilePos>()
    {
        {Tile.Dirt, new TilePos(0,0)},
        {Tile.Grass, new TilePos(1,0)},
        {Tile.GrassSide, new TilePos(0,1)},
        {Tile.Stone, new TilePos(0,2)},
        {Tile.TreeSide, new TilePos(0,4)},
        {Tile.TreeCX, new TilePos(0,3)},
        {Tile.Leaves, new TilePos(0,5)},
    };
}

public enum Tile {Dirt, Grass, GrassSide, Stone, TreeSide, TreeCX, Leaves}
