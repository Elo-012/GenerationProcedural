using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2Int _roomMinSize = new(3, 3);
        [SerializeField] private Vector2Int _roomMaxSize = new(10, 10);
        private List<RectInt> Totalroom;
        int roomplacecount = 0;
        public int MaxattemptsPerRoom = 20;
        int attempts = 0;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            Totalroom = new List<RectInt>();
            roomplacecount = 0;
            attempts = 0;

            for (int i = 0; i < _maxRooms; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int x = RandomService.Range(0, Grid.Width);
                int y = RandomService.Range(0, Grid.Lenght);
                int sizeX = RandomService.Range(_roomMinSize.x , _roomMaxSize.x);
                int sizeY = RandomService.Range(_roomMinSize.y , _roomMaxSize.y);

                RectInt room = new RectInt(x, y, sizeX, sizeY);


                PlaceRoom(room);
                
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }

            // Final ground building.
            BuildPath();
            BuildGround();
        }
        
        private void PlaceRoom(RectInt room)
        {
            
            if (CanPlaceRoom(room, 2))
            {
                for (int x = room.xMin; x < room.xMax; x++)
                {
                    for (int y = room.yMin; y < room.yMax; y++)
                    {
                        if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        {
                            AddTileToCell(cell, ROOM_TILE_NAME, true);
                        }
                    }
                }
                Totalroom.Add(room);
                roomplacecount++;
            }
            else
            {
                room.x = RandomService.Range(0, Grid.Width);
                room.y = RandomService.Range(0, Grid.Lenght);
                attempts++;
                if (attempts < MaxattemptsPerRoom)
                {
                    PlaceRoom(room);
                }
            }
        }

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }

        private void BuildPath()
        {
            SortRoomsByProximity();

            for (int i = 0; i < Totalroom.Count - 1; i++)
            {
                Vector2 coo1 = Totalroom[i].center;
                Vector2 coo2 = Totalroom[i + 1].center;

                for (int x = (int)Mathf.Min(coo1.x, coo2.x); x <= Mathf.Max(coo1.x, coo2.x); x++)
                {
                    if (Grid.TryGetCellByCoordinates(x, (int)coo1.y, out Cell cell))
                    {
                        AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
                    }
                }
                for (int y = (int)Mathf.Min(coo1.y, coo2.y); y <= Mathf.Max(coo1.y, coo2.y); y++)
                {
                    if (Grid.TryGetCellByCoordinates((int)coo2.x, y, out Cell cell))
                    {
                        AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
                    }
                }
            }
        }
        private void SortRoomsByProximity()
        {
            if (Totalroom == null || Totalroom.Count == 0) return;

            // Étape 1 : trouver la pièce la plus en haut à droite
            RectInt startRoom = Totalroom[0];
            foreach (var room in Totalroom)
            {
                if (room.yMax > startRoom.yMax || (room.yMax == startRoom.yMax && room.xMax > startRoom.xMax))
                {
                    startRoom = room;
                }
            }

            List<RectInt> sortedRooms = new List<RectInt> { startRoom };
            List<RectInt> remainingRooms = new List<RectInt>(Totalroom);
            remainingRooms.Remove(startRoom);

            while (remainingRooms.Count > 0)
            {
                RectInt last = sortedRooms[sortedRooms.Count - 1];
                Vector2 lastCenter = last.center;

                RectInt closest = remainingRooms[0];
                float minDist = Vector2.Distance(lastCenter, closest.center);

                foreach (var room in remainingRooms)
                {
                    float dist = Vector2.Distance(lastCenter, room.center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = room;
                    }
                }

                sortedRooms.Add(closest);
                remainingRooms.Remove(closest);
            }

            Totalroom = sortedRooms;
        }
    }
}