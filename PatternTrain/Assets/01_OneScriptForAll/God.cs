using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class God : MonoBehaviour
{
    [Serializable]
    public struct PrefabStartTransform
    {
        public Vector3 pos;
        public Vector3 eulerRot;
        public Vector3 scale;
    }

    [Serializable]
    public struct GameConfig
    {
        [Range(0, 10f)] public float Speed;
        [Range(1, 20)] public int DoorCount;// 布局在Road上自动平均分
        public Vector3 CameraOffset;
    }

    [Header("Prefabs:")]
    public GameObject Road;
    public GameObject Player;
    public GameObject Door;
    public GameObject MainUI;
    public GameObject GameOverPanel;

    [Space(5)]
    [Header("Prefab param:")]
    public PrefabStartTransform RoadPst;
    public PrefabStartTransform PlayerPst;
    public float HalfDoorHeight;

    [Space(5)]
    public GameConfig Config;

    private GameObject _player;
    private GameObject _mainUI;
    //private List<GameObject> _doors;

    private Camera _camera;

    private TextMeshProUGUI _scoreText;
    private GameObject _gameOverPanel;

    [Space(5)]
    public Vector2 holeSizeRange = new Vector2(0.5f, 2f);
    public Vector2 playerSizeLimit = new Vector2(0.3f, 3f);

    private List<DoorData> _doors;
    private int _currentScore;
    private bool _isGameOver;
    private bool _isUIDirty;

    private class DoorData
    {
        public GameObject doorObject;
        public float holeSize;
        public bool isPassed;
    }

    private void Awake()
    {
        InitPrefab(Road, RoadPst);
        _mainUI = Instantiate(MainUI);
        _scoreText = _mainUI.transform.Find("SocrePrefix/Socre").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _camera = Camera.main;
        InitializeGame();
    }

    private void InitPlayer()
    {
        if (_player == null)
        {
            _player = InitPrefab(Player, PlayerPst);
        }
        else
        {
            _player = SetGameObjTrans(_player, PlayerPst);
        }
    }

    private void InitializeGame()
    {
        _currentScore = 0;
        _isGameOver = false;
        _isUIDirty = true;

        InitPlayer();
        SetCameraPos();

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }

        GenerateDoors();
    }


    private void GenerateDoors()
    {
        if (_doors != null)
        {
            foreach (var door in _doors) Destroy(door.doorObject);
        }

        _doors = new List<DoorData>();
        float roadLength = RoadPst.scale.z * 10;// Road一scale有10单位
        float startZ = RoadPst.pos.z;
        float spacing = roadLength / (Config.DoorCount + 1);

        for (int i = 0; i < Config.DoorCount; i++)
        {
            float zPos = startZ + spacing * (i + 1);
            Vector3 doorPos = new Vector3(RoadPst.pos.x, RoadPst.pos.y + HalfDoorHeight, zPos);

            GameObject newDoor = Instantiate(Door, doorPos, Quaternion.identity);
            float holeSize = UnityEngine.Random.Range(holeSizeRange.x, holeSizeRange.y);

            Transform hole = newDoor.transform.Find("Hole");
            if (hole) hole.localScale = Vector3.one * holeSize;

            _doors.Add(new DoorData
            {
                doorObject = newDoor,
                holeSize = holeSize,
                isPassed = false
            });
        }
    }


    private void Update()
    {
        UpdateInput();
        UpdatePlayer();
        UpdateCheckLogic();
        UpdateCamera();
        UpdateUI();
    }

    private void UpdateInput()
    {
        if (_isGameOver) return;

        float scaleSpeed = 1.5f; // 每秒缩放的速度
        float targetScale = _player.transform.localScale.x; // 当前目标尺寸

        if (Input.GetKey(KeyCode.Q)) // 按住 Q 键变大
        {
            targetScale += scaleSpeed * Time.deltaTime;
            targetScale = Mathf.Min(targetScale, playerSizeLimit.y); // 限制最大尺寸
        }
        if (Input.GetKey(KeyCode.E)) // 按住 E 键变小
        {
            targetScale -= scaleSpeed * Time.deltaTime;
            targetScale = Mathf.Max(targetScale, playerSizeLimit.x); // 限制最小尺寸
        }

        _player.transform.localScale = Vector3.one * targetScale;
    }

    private void UpdatePlayer()
    {
        Vector3 newPos = _player.transform.position;
        newPos.z += Config.Speed * Time.deltaTime;
        _player.transform.position = newPos;
    }

    private void UpdateCheckLogic()
    {
        CheckDoorPassing();
    }

    private void CheckDoorPassing()
    {
        if (_isGameOver || _doors == null) return;

        float playerZ = _player.transform.position.z;

        for (int i = 0; i < _doors.Count; i++)
        {
            DoorData door = _doors[i];
            if (!door.isPassed && playerZ > door.doorObject.transform.position.z)
            {
                door.isPassed = true;
                float playerSize = _player.transform.localScale.x;
                float sizeDiff = Mathf.Abs(playerSize - door.holeSize);

                _currentScore += Mathf.Max(0, 100 - Mathf.RoundToInt(sizeDiff * 50));
                _isUIDirty = true;
                CheckGameCompletion();
            }
        }
    }

    private void UpdateScoreDisplay()
    {
        if (_scoreText) _scoreText.text = _currentScore.ToString();
    }

    private void CheckGameCompletion()
    {
        if (_doors.TrueForAll(d => d.isPassed))
        {
            _isGameOver = true;

            if (_gameOverPanel == null)
            {
                _gameOverPanel = Instantiate(GameOverPanel);
                var btn = _gameOverPanel.transform.Find("Btn_Restart").GetComponent<Button>();
                var btn2 = _gameOverPanel.transform.Find("Btn_Quit").GetComponent<Button>();
                btn.onClick.AddListener(OnRestart);
                btn2.onClick.AddListener(OnQuit);
            }
            else
            {
                _gameOverPanel.SetActive(true);
            }

            Time.timeScale = 0;
        }
    }

    private void UpdateCamera()
    {
        SetCameraPos();
    }

    private void UpdateUI()
    {
        if (_isUIDirty)
            UpdateScoreDisplay();
    }

    #region Camera
    private void SetCameraPos()
    {
        float shakingY = Mathf.Sin(Time.time * 2f) * 0.3f; // 上下浮动。幅度为0.3，频率为2
        float shakingZ = Mathf.Cos(Time.time * 1.5f) * 0.2f; // 前后浮动。幅度为0.2，频率为1.5
        _camera.transform.position = _player.transform.position + Config.CameraOffset + new Vector3(0, shakingY, shakingZ);
    }
    #endregion

    #region UI Event
    public void OnRestart()
    {
        Time.timeScale = 1;
        InitializeGame();
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Scene Helper
    private GameObject InitPrefab(GameObject obj, PrefabStartTransform pst)
    {
        return InitPrefab(obj, pst.pos, pst.eulerRot, pst.scale);
    }
    private GameObject InitPrefab(GameObject obj, Vector3 pos, Vector3 eulerRot, Vector3 scale)
    {
        Quaternion rot = Quaternion.Euler(eulerRot.x, eulerRot.y, eulerRot.z);
        var go = Instantiate(obj, pos, rot);
        go.transform.localScale = scale;
        return go;
    }

    private GameObject SetGameObjTrans(GameObject obj, PrefabStartTransform pst)
    {
        Quaternion rot = Quaternion.Euler(pst.eulerRot.x, pst.eulerRot.y, pst.eulerRot.z);
        obj.transform.position = pst.pos;
        obj.transform.rotation = rot;
        obj.transform.localScale = pst.scale;
        return obj;
    }
    #endregion
}
