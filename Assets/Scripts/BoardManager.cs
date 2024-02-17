using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance
    {
        get;
        set;
    }

    private bool[,] allowedMoves
    {
        get;
        set;
    }

    private int selectionX = -1;
    private int selectionY = -1;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    private List<GameObject> activePlayer;
    
    // 칸의 개수
    private static readonly int xLen = 5;
    private static readonly int yLen = 5;
    // 한칸의 크기
    private static readonly float tileSize = 1.0f;
    
    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Player[,] PlayerAxis
    {
        get;
        set;
    }
    private Player selectedPlayer = null;
    private Player selectedEnemy;

    public bool isPlayerTurn = true;

    private Material previousMat;
    public Material selectedMat;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnPlayer(0, 2, 0, true);
        SpawnPlayer(0, 2, 4, false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedPlayer == null)
                {
                    SelectPlayer(selectionX, selectionY);
                }
                else
                {
                    MovePlayer(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    // TODO: 유저턴으로 넘어오면 자동으로 SelectPlayer 상태로 바꾸기
    private void SelectPlayer(int x, int y)
    {
        Debug.Log("X: " + x + " Y: " + y);
        if (PlayerAxis[x, y] == null) return;

        if (PlayerAxis[x, y].isPlayer != isPlayerTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = PlayerAxis[x, y].PossibleMoves();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 5;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedPlayer = PlayerAxis[x, y];
        previousMat = selectedPlayer.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedPlayer.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MovePlayer(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Player p = PlayerAxis[x, y];

            PlayerAxis[selectedPlayer.CurrentX, selectedPlayer.CurrentY] = null;
            selectedPlayer.transform.position = GetTileCenter(x, y);
            selectedPlayer.SetPosition(x, y);
            PlayerAxis[x, y] = selectedPlayer;
            isPlayerTurn = !isPlayerTurn;
        }

        selectedPlayer.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedPlayer = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("BoardPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    // Spawn function for player and enemy
    private void SpawnPlayer(int index, int x, int y, bool isPlayer)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isPlayer)
        {
            go = Instantiate(playerPrefab, position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(enemyPrefab, position, blackOrientation) as GameObject;
        }

        Vector3 temp;
        
        temp = go.transform.localScale;
        temp.x *= 0.5f;
        temp.y *= 0.5f;
        temp.z *= 0.5f;
        go.transform.localScale = temp;
        
        go.transform.SetParent(transform);
        PlayerAxis[x, y] = go.GetComponent<Player>();
        PlayerAxis[x, y].SetPosition(x, y);
        activePlayer.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (tileSize * x);
        origin.z += (tileSize * y);

        return origin;
    }
}
