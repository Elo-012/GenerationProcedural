using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Components.ProceduralGeneration;
using VTools.RandomService;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/CellularAutomata")]
public class CellularAutomata : ProceduralGenerationMethod
{
    [Range(0, 1)] public float DirtoverWater = 0.5f;
    public int MinTileToBecomeGround = 4;

    private Dictionary<Vector2Int, string> tileMap;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        tileMap = new Dictionary<Vector2Int, string>();
        InitializeWorld();

        for (int i = 0; i < _maxSteps; i++)
        {
            SimulateStep();
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

        PlaceSandContours();
        ApplyTiles();
    }

    private void InitializeWorld()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var cell))
                {
                    Debug.LogError($"Unable to get cell at ({x}, {z})");
                    continue;
                }

                string tile = RandomService.Chance(DirtoverWater) ? GRASS_TILE_NAME : WATER_TILE_NAME;
                tileMap[new Vector2Int(x, z)] = tile;
            }
        }
    }

    private void SimulateStep()
    {
        var newTileMap = new Dictionary<Vector2Int, string>();

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                string currentTile = tileMap.TryGetValue(pos, out var tile) ? tile : WATER_TILE_NAME;

                int groundNeighbors = CountNeighborsOfType(x, z, GRASS_TILE_NAME);
                string newTile = currentTile;

                // Lissage : supprime les pixels isolés
                if (currentTile == GRASS_TILE_NAME && groundNeighbors <= 2)
                    newTile = WATER_TILE_NAME;

                // Automate classique
                else if (groundNeighbors > MinTileToBecomeGround)
                    newTile = GRASS_TILE_NAME;

                newTileMap[pos] = newTile;
            }
        }

        tileMap = newTileMap;
    }

    private void PlaceSandContours()
    {
        var sandPositions = new List<Vector2Int>();

        foreach (var kvp in tileMap)
        {
            if (kvp.Value != WATER_TILE_NAME) continue;

            Vector2Int pos = kvp.Key;
            int grassCount = CountNeighborsOfType(pos.x, pos.y, GRASS_TILE_NAME);
            int waterCount = CountNeighborsOfType(pos.x, pos.y, WATER_TILE_NAME);

            // Sable uniquement si la cellule d’eau touche la terre et est majoritairement entourée d’eau
            if (grassCount > 0 && waterCount >= grassCount)
                sandPositions.Add(pos);
        }

        foreach (var pos in sandPositions)
        {
            tileMap[pos] = SAND_TILE_NAME;
        }
    }

    private int CountNeighborsOfType(int x, int z, string targetTile)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                Vector2Int neighborPos = new Vector2Int(x + dx, z + dz);
                if (tileMap.TryGetValue(neighborPos, out var tile) && tile == targetTile)
                    count++;
            }
        }

        return count;
    }

    private void ApplyTiles()
    {
        foreach (var kvp in tileMap)
        {
            if (Grid.TryGetCellByCoordinates(kvp.Key.x, kvp.Key.y, out var cell))
            {
                AddTileToCell(cell, kvp.Value, true);
            }
        }
    }
}