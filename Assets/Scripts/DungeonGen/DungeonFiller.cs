using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DungeonGeneration.Graph;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

/// <summary>
/// Debug mode options for visualizing wall generation.
/// </summary>


/// <summary>
/// Handles the population of a dungeon graph into a tile-based representation,
/// including room filling, wall spawning, and debug visualization.
/// </summary>
public class DungeonFiller : Singleton<DungeonFiller>, IProgressProvider
{
    [Popup] [Tooltip("Settings config of the dungeon filler. To edit use ctrl + left click")]
    [SerializeField] private DungeonFillingSettingsSo _settings;

    public UnityEvent OnDungeonFilled;

    private Graph _dungeonGraph;
    private List<Transform> _dungeonRoomsTransforms = new List<Transform>();
    private int[,] _dungeonTileMap;
    private int[,] _dungeonRoomsMap;

    private int _localTilesYield;
    
    [HideInInspector]
    public Vector3 PlayerSpawnPoint;

    #region Debug

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void DebugTileMap() => DebugTileMap(true, false);

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void DebugTileMapWithAssets() => DebugTileMap(true, true);

    #endregion



    /// <summary>
    /// Initiates the dungeon filling process:
    /// tile mapping, room transforms, floor and wall spawning.
    /// </summary>
    [Button]
    public async void FillTheDungeon()
    {
        Initialize();
        CreteTileMap();
        //PathFinder.Initialize(_dungeonTileMap, _dungeonRoomsMap, _dungeonGraph, _settings.TileSize);
        CreateRoomsTransforms();

        _progressText = "Creating floors";
        await Task.Yield();
        await StartFloodFillFloor();
        _progressText = "Creating walls";
        await Task.Yield();
        await SpawnWalls();
        
        Debug.Log($"Full filled dungeon generation: {Time.realtimeSinceStartup - DungeonGenerator.Instance.StarTime}");
        //OnDungeonFilled?.Invoke();
    }
    private void Initialize()
    {
        _dungeonGraph = DungeonGenerator.Instance.Graph;
        _dungeonTileMap = new int[
            DungeonGenerator.Instance.Settings.DungeonParameters.height,
            DungeonGenerator.Instance.Settings.DungeonParameters.width];

        _dungeonRoomsMap = new int[
            DungeonGenerator.Instance.Settings.DungeonParameters.height,
            DungeonGenerator.Instance.Settings.DungeonParameters.width];
        
        _totalTiles = _dungeonTileMap.GetLength(0) * _dungeonTileMap.GetLength(1);
        _tilesFiled = 0;
    }

    /// <summary>
    /// Fills the tile map based on the dungeon graph structure.
    /// Marks floors, walls, and doors.
    /// </summary>
    public void CreteTileMap()
    {
        var dungeonRooms = _dungeonGraph.GetRooms();

        // Initialize map to default value
        for (int y = 0; y < _dungeonTileMap.GetLength(0); y++)
            for (int x = 0; x < _dungeonTileMap.GetLength(1); x++)
                _dungeonTileMap[y, x] = -1;

        // Fill the map
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            AlgorithmsUtils.FillRectangle(_dungeonTileMap, dungeonRooms[i].Dimensions, 0);
            AlgorithmsUtils.FillRectangle(_dungeonRoomsMap, dungeonRooms[i].Dimensions, i);
            AlgorithmsUtils.FillRectangleOutline(_dungeonTileMap, dungeonRooms[i].Dimensions, 1,
                DungeonGenerator.Instance.Settings.WallWidth);

            //Add columns
            if (dungeonRooms[i].Dimensions.height > _settings.MinimalHeightForColumn &&
                dungeonRooms[i].Dimensions.width > _settings.MinimalWidthForColumn)
            {
                var columns = CreateColumnsFromRoom(dungeonRooms[i].Dimensions);
                foreach (var column in columns)
                    AlgorithmsUtils.FillRectangle(_dungeonTileMap, column, 1);
            }
        }

        //Add doors
        foreach (var door in _dungeonGraph.GetDoors())
            AlgorithmsUtils.FillRectangle(_dungeonTileMap, door.Dimensions, 0);
    }

    /// <summary>
    /// Creates support columns inside a room based on its dimensions.
    /// </summary>
    private List<RectInt> CreateColumnsFromRoom(RectInt d)
    {
        List<RectInt> columns = new List<RectInt>();
        int columnH = Mathf.CeilToInt((float)(d.height - DungeonGenerator.Instance.Settings.WallWidth * 2) / _settings.MinimalHeightForColumn);
        int columnW = Mathf.CeilToInt((float)(d.width - DungeonGenerator.Instance.Settings.WallWidth * 2) / _settings.MinimalWidthForColumn);
        if (columnH <= 0 || columnW <= 0) return columns;

        float distanceH = (float)(d.height - DungeonGenerator.Instance.Settings.WallWidth * 2) / (columnH + 1);
        float distanceW = (float)(d.width - DungeonGenerator.Instance.Settings.WallWidth * 2) / (columnW + 1);

        for (int i = 1; i <= columnH; i++)
        {
            for (int j = 1; j <= columnW; j++)
            {
                RectInt newColumn = new RectInt
                {
                    height = _settings.ColumnsSize,
                    width = _settings.ColumnsSize,
                    x = Mathf.RoundToInt(d.x + distanceW * j - (_settings.ColumnsSize - 1) + DungeonGenerator.Instance.Settings.WallWidth),
                    y = Mathf.RoundToInt(d.y + distanceH * i - (_settings.ColumnsSize - 1) + DungeonGenerator.Instance.Settings.WallWidth)
                };
                columns.Add(newColumn);
            }
        }

        return columns;
    }

    /// <summary>
    /// Instantiates room GameObjects to hold generated tiles.
    /// </summary>
    private void CreateRoomsTransforms()
    {
        foreach (var room in _dungeonGraph.GetRooms())
        {
            var tr = new GameObject($"Room_{room.Dimensions.x},{room.Dimensions.y}").transform;
            tr.parent = transform;
            tr.position = room.GetCenter() * _settings.TileSize;
            _dungeonRoomsTransforms.Add(tr);
        }
    }

 /// <summary>
/// Starts the flood-fill algorithm from the first room to instantiate floor tiles.
/// </summary>
private async Task StartFloodFillFloor()
{
    var startPoint = _dungeonGraph.GetRooms()[0].GetCenter();
    var tile = _dungeonTileMap[(int)startPoint.z, (int)startPoint.x];

    // Find a valid starting floor tile
    if (tile != 0)
    {
        var roomDim = _dungeonGraph.GetRooms()[0].Dimensions;
        startPoint = new Vector3(roomDim.x, startPoint.y, roomDim.y);

        while (true)
        {
            tile = _dungeonTileMap[(int)startPoint.z, (int)startPoint.x];
            if (tile == 0) break;

            if (startPoint.x < roomDim.x + roomDim.width - 1)
            {
                startPoint.x += 1;
                continue;
            }

            if (startPoint.z < roomDim.y + roomDim.height - 1)
            {
                startPoint.z += 1;
            }
        }
    }

    startPoint = new Vector3((int)startPoint.x, startPoint.y, (int)startPoint.z);

    PlayerSpawnPoint = new Vector3(
        (startPoint.x + 0.5f) * _settings.TileSize,
        startPoint.y,
        (startPoint.z + 0.5f) * _settings.TileSize
    );

    await FillIterative(startPoint);
}

/// <summary>
/// Iterative flood-fill using a queue to place floor prefabs.
/// </summary>
private async Task FillIterative(Vector3 startPoint)
{
    var discovered = new HashSet<Vector3>();
    var queue = new Queue<Vector3>();
    queue.Enqueue(startPoint);

    while (queue.Count > 0)
    {
        var point = queue.Dequeue();
        
        // Skip if out of bounds or already visited
        if (point.z >= _dungeonTileMap.GetLength(0) ||
            point.x >= _dungeonTileMap.GetLength(1) ||
            !discovered.Add(point))
            continue;

        var tile = _dungeonTileMap[(int)point.z, (int)point.x];
        if (tile != 0) continue;

        _tilesFiled++;
        // Instantiate floor
        var floor = Instantiate(
            _settings.TilesPrefabs[0],
            new Vector3(point.x * _settings.TileSize, point.y, point.z * _settings.TileSize),
            Quaternion.identity
        );
        floor.transform.parent = _dungeonRoomsTransforms[_dungeonRoomsMap[(int)point.z, (int)point.x]];
        floor.name = $"Floor_{point.x},{point.z}";

        if (_settings.ResizeAssetsUsingTileSize)
            floor.transform.localScale *= _settings.TileSize;

        // Add neighbors to queue
        queue.Enqueue(new Vector3(point.x + 1, point.y, point.z));
        queue.Enqueue(new Vector3(point.x - 1, point.y, point.z));
        queue.Enqueue(new Vector3(point.x, point.y, point.z + 1));
        queue.Enqueue(new Vector3(point.x, point.y, point.z - 1));
        
        if (_tilesFiled % _settings.TilesSpawnYield == 0)
            await Task.Yield();
        // Optional pacing for debug visualization
        await Task.Delay(_settings.FloodFillAwaitTime, Application.exitCancellationToken);
    }
}

    /// <summary>
    /// Spawns wall tiles based on tile map transitions using bitmask index.
    /// </summary>
    private async Task SpawnWalls()
    {
        for (int y = 1; y < _dungeonTileMap.GetLength(0); y++)
        {
            for (int x = 1; x < _dungeonTileMap.GetLength(1); x++)
            {
                if (_dungeonTileMap[y, x] < 0 || _dungeonTileMap[y - 1, x] < 0 ||
                    _dungeonTileMap[y, x - 1] < 0 || _dungeonTileMap[y - 1, x - 1] < 0) continue;


                // Calculate wall index
                int index = _dungeonTileMap[y, x] * 2 +
                            _dungeonTileMap[y - 1, x] * 1 +
                            _dungeonTileMap[y, x - 1] * 4 +
                            _dungeonTileMap[y - 1, x - 1] * 8;
                if (index == 0) continue;

                _tilesFiled++;
                // Instantiate prefab
                Vector3 position = new Vector3((x - 0.5f) * _settings.TileSize, _dungeonRoomsTransforms[_dungeonRoomsMap[y, x]].position.y, (y - 0.5f) * _settings.TileSize);
                var wall = Instantiate(_settings.TilesPrefabs[index], position, Quaternion.identity).transform;
                wall.parent = _dungeonRoomsTransforms[_dungeonRoomsMap[y, x]];
                wall.name = $"{_settings.TilesPrefabs[index].name}_{x},{y}";
                if (_settings.ResizeAssetsUsingTileSize)
                    wall.localScale *= _settings.TileSize;

                if (_tilesFiled % _settings.TilesSpawnYield == 0)
                    await Task.Yield();
                
                if (_settings.WallDebugMode == WallDebugMode.EachTile)
                    await Task.Delay(_settings.WallCreationAwaitTime, Application.exitCancellationToken);
            }

            if (_settings.WallDebugMode == WallDebugMode.Line)
                await Task.Delay(_settings.WallCreationAwaitTime, Application.exitCancellationToken);
        }
    }

    /// <summary>
    /// Outputs the current dungeon tile map to the console.
    /// Optionally spawns debug prefabs at tile locations.
    /// </summary>
    public void DebugTileMap(bool flip, bool spawnAssets)
    {
        int rows = _dungeonTileMap.GetLength(0);
        int cols = _dungeonTileMap.GetLength(1);
        var sb = new StringBuilder();

        int start = flip ? rows - 1 : 0;
        int end = flip ? -1 : rows;
        int step = flip ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            for (int j = 0; j < cols; j++)
            {
                string o = _dungeonTileMap[i, j] switch
                {
                    -1 => "@",
                    1 => "#",
                    _ => _dungeonTileMap[i, j].ToString()
                };
                sb.Append(o);
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());

        if (!spawnAssets) return;

        for (int y = 0; y < _dungeonTileMap.GetLength(0); y++)
        {
            for (int x = 0; x < _dungeonTileMap.GetLength(1); x++)
            {
                int index = _dungeonTileMap[y, x] < 0 ? 0 : _dungeonTileMap[y, x];
                Vector3 position = new Vector3(x, 0, y);
                Instantiate(_settings.DebugPrefabs[index], position, Quaternion.identity);
            }
        }
    }

    #region Progress
    
    private string _progressText;
    private int _totalTiles = 1;
    private int _tilesFiled;

    public float GetProgress()
    {
        return (float)_tilesFiled / _totalTiles;
    }

    public string GetProgressTitle()
    {
        return _progressText;
    }

    #endregion
}
