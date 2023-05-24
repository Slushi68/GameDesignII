using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private enum GameState { Completed, Starting, Playing, Ending }
    private GameState currentGameState;
    private GameState lastState;
    public int m_NumRoundsToWin = 5;        
    public float m_StartDelay = 3f;         
    public float m_EndDelay = 3f;           
    public CameraControl m_CameraControl;   
    public TextMeshProUGUI m_MessageText;              
    public GameObject m_TankPrefab;         
    public TankManager[] m_Tanks;
    public Button StartButton;

    private int m_RoundNumber;              
    private WaitForSeconds m_StartWait;     
    private WaitForSeconds m_EndWait;       
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    private void Start()
    {
        DontDestroyOnLoad(this);
        StartButton.onClick.AddListener(StartGame);
        StartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void StartGame()
    {
        StartButton.gameObject.SetActive(false);
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RunLogic();
        }
    }
    private void RunLogic()
    {
        if (lastState == currentGameState) return;
        
        switch (currentGameState)
        {
            case GameState.Completed:
                break;
            case GameState.Starting:
                RoundStartLogic();
                break;
            case GameState.Playing:
                RoundPlayLogic();
                break;
            case GameState.Ending:
                RoundEndLogic();
                break;
            default:
                break;
        }
        lastState = currentGameState;
    }


    private void SpawnAllTanks()
    {
        //only let the master spawn the tanks
        if (!PhotonNetwork.IsMasterClient) return;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                PhotonNetwork.Instantiate(m_TankPrefab.name, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_Instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }
    public void AddTank(GameObject tank)
    {
        //what is the closest spawn point to this tank
        int closest = -1;
        float distance = float.MaxValue;
        for(var i = 0; i <m_Tanks.Length; i++)
        {
            var dist = Vector3.Distance(tank.transform.position, m_Tanks[i].m_SpawnPoint.position);
            if (dist < distance)
            {
                distance = dist;
                closest = i;
            }
        }
        m_Tanks[closest].m_Instance = tank;
        m_Tanks[closest].m_PlayerNumber = closest + 1;
        m_Tanks[closest].Setup();
        SetCameraTargets();
    }

    private IEnumerator GameLoop()
    {
        currentGameState = GameState.Starting;
        yield return StartCoroutine(RoundStarting());
        currentGameState = GameState.Playing;
        yield return StartCoroutine(RoundPlaying());
        currentGameState = GameState.Ending;
        yield return StartCoroutine(RoundEnding());
        currentGameState = GameState.Completed;
        if (m_GameWinner != null)
        {
            LeaveRoom();
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        RoundStartLogic(); 
        yield return m_StartWait;
    }

    private void RoundStartLogic()
    {
        //reset all tanks
        ResetAllTanks();
        //disable all tank controls
        DisableTankControl();
        //set camera position and size
        m_CameraControl.SetStartPositionAndSize();
        //increment round number
        m_RoundNumber++;
        //set the message UI text
        m_MessageText.text = "ROUND " + m_RoundNumber;
    }

    private IEnumerator RoundPlaying()
    {
        RoundPlayLogic(); 
        while (!OneTankLeft())
        {
            yield return null; //come back after every frame and check
        }
    }

    private void RoundPlayLogic()
    {
        //enable all tank controls
        EnableTankControl();
        //empty message UI texts
        m_MessageText.text = string.Empty;
        //wait for one tank left
    }

    private IEnumerator RoundEnding()
    {
        RoundEndLogic();
        yield return m_EndWait;
    }

    private void RoundEndLogic()
    {
        //disable all tank controls
        DisableTankControl();
        //clear existing winner and get the round winner
        m_RoundWinner = null;
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
        }
        //check for a game winner
        m_GameWinner = GetGameWinner();
        //calculate message UI text and show text
        string message = EndMessage();
        m_MessageText.text = message;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
    private void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Trying to load but we are not the Master");
            return;
        }
        PhotonNetwork.LoadLevel(1);//Level 1 is the scenes/main
    }

    #region Photon Callbacks
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("Player has entered the arena" + other.NickName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player has left the arena: " + otherPlayer.NickName);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(currentGameState);
        }
        else if(stream.IsReading && !PhotonNetwork.IsMasterClient)
        {
            currentGameState = (GameState)stream.ReceiveNext();
        }
    }
    #endregion

}