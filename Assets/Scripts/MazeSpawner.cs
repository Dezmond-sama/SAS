using System.Collections.Generic;
using UnityEngine;

public class MazeSpawner : MonoBehaviour
{
    public float Scale = 0.5f;
    [SerializeField] private List<Cell> _cellElems;
    [SerializeField] private List<Cell> _roomElems;
    [SerializeField] private List<GameObject> _corners;
    [SerializeField] private float _cellSize = 1;
    [SerializeField] private bool _is3D = false;
    [SerializeField] private Vector2Int _start = new Vector2Int(5, 5);
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private int _seed = 1;
    [SerializeField] private int _straightCoef = 0;
    [SerializeField] private int _scaleCoef = 6;
    private Player _player;
    private Maze _maze;
    private MazeGrid _grid;
    private void Awake()
    {
        Generate();
        InstantiateMaze(_maze);
        _player = Instantiate(_playerPrefab, GetPosition(_start.x + .5f, _start.y + .5f, _maze),Quaternion.identity);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Generate();
            foreach (Transform child in transform) Destroy(child.gameObject);
            InstantiateMaze(_maze);
            _player.transform.position = GetPosition(_start.x + .5f, _start.y + .5f, _maze);
            _player.CameraInit();
        }
    }

    private void OnDrawGizmos()
    {
        if (_grid == null) return;
        for(int x = 0; x < _grid.Width; ++x)
        {
            for (int y = 0; y < _grid.Height; ++y)
            {
                Vector3 start = new Vector3((x - _grid.Width / 2) * _cellSize / _scaleCoef, (y - _grid.Height/ 2) * _cellSize / _scaleCoef, 0) + transform.position;
                Vector3 size = new Vector3(_cellSize / _scaleCoef, _cellSize / _scaleCoef, _cellSize / _scaleCoef);
                Color color;
                if (_grid[x, y].CameraSlot) color = new Color(1f, 1f, 0.5f, 0.5f);
                else if (!_grid[x, y].Walkable) color = new Color(1, 0.5f, 0.5f, 0.5f);
                else if (_grid[x, y].RoomNumber == 0) color = new Color(0.5f, 1, 0.5f, 0.5f);
                else if (_grid[x, y].RoomNumber > 0) color = new Color(0.5f, 0.5f, 1f, 0.5f);
                else color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                Gizmos.color = color;
                Gizmos.DrawCube(start, size);
            }
        }
    }
    private void Generate()
    {
        _maze = Maze.CreateMaze(21,21);
        _maze.SetSeed(_seed);
        for (int i = 0; i < 7; ++i)
        {
            _maze.PlaceRoom(new Vector2Int(3 * i, 18), new Vector2Int(3, 3), i + 1, false, new Vector2Int(1, 0));
        }
        _maze.RemoveWall(new Vector2Int(2, 19), new Vector2Int(3, 19));
        _maze.GenerateMaze(5, 0, 
            new Vector2Int(2, 2), new Vector2Int(6, 6), 
            _start, true, 10, true, false, _straightCoef);
        _maze.InitKeys();
        _grid = MazeGrid.CreateGrid(_maze, _scaleCoef);

    }

    Vector3 GetPosition(float x, float y, Maze maze)
    {
        if (_is3D)
        {
            return new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y - maze.Height / 2f) * _cellSize + transform.position.z);
        }
        else
        {
            return new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, (y - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
        }
        
    }
    private void InstantiateMaze(Maze maze)
    {
        Vector3 pos;

        List<Vector3> corners = new List<Vector3>();
        for (int x = 0; x < maze.Width; ++x)
        {
            for (int y = 0; y < maze.Height; ++y)
            {
                //if (maze.GetExitCount(x, y) == 0) continue;

                if (_corners.Count > 0)
                {
                    if (maze[x, y].HasLeftWall ||
                       maze[x, y].HasBottomWall ||
                       (x > 0 && (maze[x - 1, y].HasRightWall || maze[x - 1, y].HasBottomWall)) ||
                       (y > 0 && (maze[x, y - 1].HasTopWall || maze[x, y - 1].HasLeftWall)))
                    {
                        if (_is3D)
                        {
                            pos = new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y - maze.Height / 2f) * _cellSize + transform.position.z);
                        }
                        else
                        {
                            pos = new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, (y - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
                        }
                        //Instantiate(_corners[Random.Range(0, _corners.Count)], pos, Quaternion.identity,transform);
                        if(!corners.Contains(pos))corners.Add(pos);
                    }

                    if ((y == maze.Height - 1 || maze.GetExitCount(x, y + 1) == 0) &&
                       (maze[x, y].HasLeftWall ||
                        maze[x, y].HasTopWall))
                    {
                        if (_is3D)
                        {
                            pos = new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y + 1 - maze.Height / 2f) * _cellSize + transform.position.z);
                        }
                        else
                        {
                            pos = new Vector3((x - maze.Width / 2f) * _cellSize + transform.position.x, (y + 1 - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
                        }
                        //Instantiate(_corners[Random.Range(0, _corners.Count)], pos, Quaternion.identity, transform);
                        if (!corners.Contains(pos)) corners.Add(pos);
                    }

                    if ((x == maze.Width - 1 || maze.GetExitCount(x + 1, y) == 0) &&
                       (maze[x, y].HasRightWall ||
                        maze[x, y].HasBottomWall))
                    {
                        if (_is3D)
                        {
                            pos = new Vector3((x + 1 - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y - maze.Height / 2f) * _cellSize + transform.position.z);
                        }
                        else
                        {
                            pos = new Vector3((x + 1 - maze.Width / 2f) * _cellSize + transform.position.x, (y - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
                        }
                        //Instantiate(_corners[Random.Range(0, _corners.Count)], pos, Quaternion.identity, transform);
                        if (!corners.Contains(pos)) corners.Add(pos);
                    }

                    if ((y == maze.Height - 1 || maze.GetExitCount(x, y + 1) == 0) &&
                        (x == maze.Width - 1 || maze.GetExitCount(x + 1, y) == 0) &&
                        (maze[x, y].HasRightWall ||
                       maze[x, y].HasTopWall))
                    {
                        if (_is3D)
                        {
                            pos = new Vector3((x + 1 - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y + 1 - maze.Height / 2f) * _cellSize + transform.position.z);
                        }
                        else
                        {
                            pos = new Vector3((x + 1 - maze.Width / 2f) * _cellSize + transform.position.x, (y + 1 - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
                        }
                        //Instantiate(_corners[Random.Range(0, _corners.Count)], pos, Quaternion.identity, transform);
                        if (!corners.Contains(pos)) corners.Add(pos);
                    }
                }

                if (_is3D)
                {
                    pos = new Vector3((x + .5f - maze.Width / 2f) * _cellSize + transform.position.x, transform.position.y, (y + .5f - maze.Height / 2f) * _cellSize + transform.position.z);
                }
                else
                {
                    pos = new Vector3((x + .5f - maze.Width / 2f) * _cellSize + transform.position.x, (y + .5f - maze.Height / 2f) * _cellSize + transform.position.y, transform.position.z);
                }
                Cell cell = maze[x, y].Room ?
                    Instantiate(_roomElems[Random.Range(0, _roomElems.Count)], pos, Quaternion.identity, transform) :
                    Instantiate(_cellElems[Random.Range(0, _cellElems.Count)], pos, Quaternion.identity, transform);
                if (cell != null)
                {
                    bool b = maze.GetExitCount(x, y) > 0;
                    cell.SetCeilEnabledState(!b);
                    cell.SetWallEnabledState(Cell.WallSide.wsLeft, b && maze[x, y].HasLeftWall);
                    cell.SetWallEnabledState(Cell.WallSide.wsRight, b && maze[x, y].HasRightWall);
                    cell.SetWallEnabledState(Cell.WallSide.wsTop, b && maze[x, y].HasTopWall);
                    cell.SetWallEnabledState(Cell.WallSide.wsBottom, b && maze[x, y].HasBottomWall);
                    cell.RoomIndex = maze[x, y].RoomNumber;
                }
            }
        }
        if (_corners.Count > 0) {
            foreach (Vector3 coords in corners) Instantiate(_corners[Random.Range(0, _corners.Count)], coords, Quaternion.identity, transform);
        }
        for(int doorIndex = 0; doorIndex < maze.DoorsCount(); ++doorIndex)
        {
            MazeDoor door = maze.Door(doorIndex);
            InstantiateDoor(door);
            //if (door.doorIndex > 0) PlaceKey(door.doorIndex, false);
        }
        //InstantiateEnemies();
        //InstantiateTreasures();
    }

    private void InstantiateDoor(MazeDoor door)
    {

    }
}
