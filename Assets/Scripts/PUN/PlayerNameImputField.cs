using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
namespace MyGame
{

    //[RequireComponent(typeof(InputField))]
    public class PlayerNameImputField : MonoBehaviour
    {
        #region Constants
        //store the player name in the player Prefs
        const string playerNamePrefKey = "PlayerName";
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            string defaultName = string.Empty;
            InputField inputField = this.GetComponent<InputField>();
            if (inputField is null) return;
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                //if we have played before and save our name, make it defaut
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
            }
            PhotonNetwork.NickName = defaultName;
        }
        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.Log("Player name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
    }
}