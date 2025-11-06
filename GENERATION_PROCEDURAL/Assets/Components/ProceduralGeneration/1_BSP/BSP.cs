using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Components.ProceduralGeneration;
using VTools.RandomService;
using VTools.Grid;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
public class BSP : ProceduralGenerationMethod
{
    [Header ("Split Parameters")]
    [Range (0f, 1f)] public float BSPScale = 1f;

    protected override UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}