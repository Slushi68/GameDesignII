using UnityEngine;
using Photon;
using Photon.Realtime;
using Photon.Pun;
public class TankMovement : MonoBehaviour
{
    [SerializeField] private int _PlayerNumber = 1;
     public int m_PlayerNumber
    {
        get
        {
            return _PlayerNumber;
        }
        set
        {
            _PlayerNumber = value;
        }
    }
    [SerializeField] private float m_Speed = 12f;
    [SerializeField] private float m_TurnSpeed = 180f;
    [SerializeField] private AudioSource m_MovementAudio;
    [SerializeField] private AudioClip m_EngineIdling;
    [SerializeField] private AudioClip m_EngineDriving;
    [SerializeField] private float m_PitchRange = 0.2f;

    
    private string m_MovementAxisName;     
    private string m_TurnAxisName;         
    private Rigidbody m_Rigidbody;         
    private float m_MovementInputValue;    
    private float m_TurnInputValue;        
    private float m_OriginalPitch;         


    private void Awake()
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber != _PlayerNumber)
        {
            this.enabled = false;
            return;
        }
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";

        m_OriginalPitch = m_MovementAudio.pitch;
    }
    

    private void Update()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != m_PlayerNumber)
        {
            this.enabled = false;
            return;
        }
        // Store the player's input and make sure the audio for the engine is playing.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
    }


    private void FixedUpdate()
    {
        // Move and turn the tank.
        Move();
        Turn();
        
    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        //Create a vector in the direction the tank is facing with a magnitude
        //Based on the input, speed and the time between frames
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        // Determine the number of degrees to be turned based on the input, speed and time between frames
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        // Make this into a rotation in the y axis
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        // Apply this rotation to the rigidbody's rotation
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}