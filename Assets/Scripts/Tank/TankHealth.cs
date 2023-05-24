using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class TankHealth : MonoBehaviourPunCallbacks, IPunObservable
{
    public float m_StartingHealth = 100f;          
    public Slider m_Slider;                        
    public Image m_FillImage;                      
    public Color m_FullHealthColor = Color.green;  
    public Color m_ZeroHealthColor = Color.red;    
    public GameObject m_ExplosionPrefab;
    
    
    private AudioSource m_ExplosionAudio;          
    private ParticleSystem m_ExplosionParticles;   
    private float m_CurrentHealth;  
    private bool m_Dead;            


    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        m_ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        SetHealthUI();
    }
    private void Update()
    {
        CheckForDeath();
    }

    public void TakeDamage(float amount)
    {
        // if this is not the master, ignore damage 
        if (PhotonNetwork.IsMasterClient) return;
        // Adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
        //Reduce the health by amount
        m_CurrentHealth -= amount;
        //update the UI
        SetHealthUI();
        //If the current health is at or below zero and it has not yet been registered
        //Call OnDeath
        CheckForDeath();
    }
    private void CheckForDeath()

    {
        if (m_CurrentHealth <= 0 && !m_Dead)
        {
            OnDeath();
        }
    }
        
    


    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        m_Slider.value = m_CurrentHealth;
        //Adjust the color
        float healthStep = m_CurrentHealth / m_StartingHealth;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, healthStep);
    }


    private void OnDeath()
    {
        // Play the effects for the death of the tank and deactivate it.
        //Set the flag to only die once
        m_Dead = true;
        //Move the explosion prefab to the same position as the tank
        m_ExplosionPrefab.transform.position = transform.position;
        //activate the explosions
        m_ExplosionPrefab.SetActive(true);

        //play the particles
        m_ExplosionParticles.Play();
        //play the audio
        m_ExplosionAudio.Play();

        //turn off the tank
        gameObject.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            //we own this player: send the others our date
            stream.SendNext(m_CurrentHealth);
        }
        else if (stream.IsReading)
        {
            //Network player, receieve data
            m_CurrentHealth = (float)stream.ReceiveNext();
            SetHealthUI();
        }
    }
}