using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Components.ProceduralGeneration;
using VTools.RandomService;
using VTools.Grid;

[CreateAssetMenu(menuName = "Procedural Generation Method/3DTerrain")]

public class Terrain : ProceduralGenerationMethod
{
    [Header("NOISE PARAMETERS")]
    [Range(0f, 1f)] public float frequency = 1f;
    [Range(0f, 2f)] public float amplitude = 1f;
    [Range(0f, 10f)] public float lacunarity = 1f;
    [Range(0f, 10f)] public float Persistance = 1f;
    [Range(0, 10)] public int octave = 3;
    [Header("LAYER")]
    [Range(-1f, 1f)] public float WaterHeight = 1f;
    [Range(-1f, 1f)] public float SandHeight = 1f;
    [Range(-1f, 1f)] public float GrassHeight = 1f;
    [Range(-1f, 1f)] public float RockHeight = 1f;

    private Terrain TERRAIN;
    public FastNoiseLite.NoiseType _NOISETYPE;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetSeed(RandomService.Seed);


        noise.SetNoiseType(_NOISETYPE);
        float[,] noiseData = new float[Grid.Width, Grid.Lenght];

        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octave);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(Persistance);

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                noiseData[x, y] = noise.GetNoise(x, y);
                float Height = noise.GetNoise(x, y);
                bool hasplace = false;
                Height = Mathf.Clamp(Height, -1f, 1f) * amplitude;
                if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                {
                    if (Height < WaterHeight)
                    {
                        AddTileToCell(cell, WATER_TILE_NAME, false);
                        hasplace = true;
                    }
                    if (Height < SandHeight && hasplace == false)
                    {
                        AddTileToCell(cell, SAND_TILE_NAME, false);
                        hasplace = true;
                    }
                    if (Height < GrassHeight && hasplace == false)
                    {
                        AddTileToCell(cell, GRASS_TILE_NAME, false);
                        hasplace = true;
                    }
                    if (Height > RockHeight || Height > GrassHeight && hasplace == false)
                    {
                        AddTileToCell(cell, ROCK_TILE_NAME, false);
                        hasplace = true;
                    }
                }
            }
        }


    }
}