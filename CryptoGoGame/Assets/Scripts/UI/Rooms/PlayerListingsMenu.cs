using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject Background;

   [SerializeField]
    private PlayerListing _playerListing;
    [SerializeField]
    private Text _roomName;

    [SerializeField]
    private MainPlayer _player;

    [SerializeField]
    private MainPlayer _enemy;

    [SerializeField]
    private GameObject _CardTohandPrefab;

    [SerializeField]
    private Transform _content;
    [SerializeField]
    private Transform _mainPlayer;
    [SerializeField]
    private MainPlayer _enemyPlayer1;
    [SerializeField]
    private MainPlayer _enemyPlayer2;
    [SerializeField]
    private MainPlayer _enemyPlayer3;

    [SerializeField]
    private Text _readyUpText;

    [SerializeField]
    private Button _readyButton;

    [SerializeField]
    private Button _startButton;

    [SerializeField]
    private PlayerDeck playerDeck;




    private CardDatabase CardDatabase;
    private bool _ready = false;
    private List<PlayerListing> _listings = new List<PlayerListing>(); 
    public List<PlayerListing> PlayerListings{get {return _listings;}}
    private List<MainPlayer> _mainPlayersListTemp = new List<MainPlayer>();
    private List<string> _transformListings = new List<string>{"MainPlayer", "EnemyPlayer (1)", "EnemyPlayer (2)","EnemyPlayer (3)"};
    private Dictionary<int, MainPlayer> _enemyPlayerObjectList = new Dictionary<int, MainPlayer>();


    private  Dictionary<Player, List<Card>> playerDecks;
    public Dictionary<Player,List<Card>> PlayerDecks{
        get {return playerDecks;}
        set{
            playerDecks = value;
        }
    }
    public Dictionary<Player, List<Card>> playerCardsPlay;
        public Dictionary<Player,List<Card>> PlayerCardsPlay{
        get {return playerCardsPlay;}
        set{
            playerCardsPlay = value;
        }
    }

 //   private ExitGames.Client.Photon.Hashtable m_playerCustomProperties = ExitGames.Client.Photon.Hashtable();
    private int cardNumber;
    private void Start(){
        PhotonNetwork.EnableCloseConnection = true;
    }
    private void Awake(){
        Debug.Log("PlayerListingsMenu() Call");
        playerDeck = Background.GetComponent<PlayerDeck>();
        CardDatabase = Background.GetComponent<CardDatabase>();

        playerDecks = new Dictionary<Player, List<Card>>();
        playerCardsPlay = new Dictionary<Player, List<Card>>();
        _roomName.text += PhotonNetwork.CurrentRoom.Name;

        GetCurrentRoomPlayers();
        ButtonsHide();
        UnHideKickButton();
      //  AddPlayerObject();
        AddingEnemyPlayerObjectList();
        CreatePlayerObjectOfCurrentRoomPlayers();
       ExitGames.Client.Photon.Hashtable setPlayerValues = new ExitGames.Client.Photon.Hashtable();
        
        setPlayerValues["PlayerCardsPlay"] = null ;
        setPlayerValues["PlayerCards"] = null;
        Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setPlayerValues));
        ExitGames.Client.Photon.Hashtable setRoomValues = new ExitGames.Client.Photon.Hashtable();
        // setRoomValues["turn"] = 0;
        // setRoomValues["StartTime"] = 0;
        setRoomValues["Deck"] = null;
        Debug.Log(PhotonNetwork.CurrentRoom.SetCustomProperties(setRoomValues));
         setRoomValues["StartTime"] = 0;
         setRoomValues["isShuffle"] = false;
        Debug.Log(PhotonNetwork.CurrentRoom.SetCustomProperties(setRoomValues));
       
        
    }
    private void ButtonsHide(){
        Debug.Log("ButtonsHide() call");
        Debug.Log(PhotonNetwork.IsMasterClient);
        if(!PhotonNetwork.IsMasterClient){
            _startButton.gameObject.SetActive(false);
        }
        else{
            _readyButton.gameObject.SetActive(false);
            int index = _listings.FindIndex(x => x.Player == PhotonNetwork.MasterClient);
            if(index != -1){
                _listings[index].SetReady(false);

            }
            _readyUpText.text = "Waiting others ready";
        }
    }
    private void AddingEnemyPlayerObjectList(){
        _enemyPlayerObjectList.Add(1, _enemyPlayer1);
        _enemyPlayerObjectList.Add(2, _enemyPlayer2);
        _enemyPlayerObjectList.Add(3, _enemyPlayer3);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        SetReadyUp(false);
    }
    private void SetReadyUp(bool state){
        _ready = state;
        if(!PhotonNetwork.IsMasterClient){
            if(_ready){
                _readyUpText.text = "Ready";
            }
            else{
                _readyUpText.text = "Unready";
            }
      
        }
    }
    public void OnClick_ReadyUp(){
        Debug.Log("Ready button clicked");
        if(!PhotonNetwork.IsMasterClient){
            SetReadyUp(!_ready);
        //    base.photonView.RPC("RPC_ChangeReadyState", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, !_ready);
            this.photonView.RPC("RPC_ChangeReadyState", RpcTarget.All, PhotonNetwork.LocalPlayer, _ready);
        }
    }
    public void OnClick_StartGame(){
        if(PhotonNetwork.IsMasterClient){
            foreach(PlayerListing player in _listings){
                if(!player.Ready && player.Player != PhotonNetwork.MasterClient){
                    return;
                }
            }
          //  playerDeck.StartGame();
            this.photonView.RPC("RPC_StartingGame", RpcTarget.AllViaServer);
//            _readyUpText.gameObject.SetActive(false);
            _readyButton.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(false);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }
    private void GetCurrentRoomPlayers(){
        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players){
            Debug.Log("Player : " + playerInfo.Value.NickName + " , Key: " + playerInfo.Key);
            AddPlayerListing(playerInfo.Value);
        }

    }
    private void CreatePlayerObjectOfCurrentRoomPlayers(){
   //     _playersListTemp.Add(PhotonNetwork.LocalPlayer);
        AddPlayerObject(PhotonNetwork.LocalPlayer, 0);
        // foreach(Player playerInfo in PhotonNetwork.PlayerListOthers){
        //     _playersListTemp.Add(playerInfo);
        // }

        for(int i = 0; i < PhotonNetwork.PlayerListOthers.Length;i++){
            AddPlayerObject(PhotonNetwork.PlayerListOthers[i], i+1);
        }
    
    }
    private void AddPlayerObject(Player newPlayer, int position){
        Debug.Log("Instantiate " + newPlayer.NickName);
        // int index = _playersListTemp.FindIndex(x => x == newPlayer);
        // if(index == -1){
        //     _playersListTemp.Add(newPlayer);
        // }  
        MainPlayer playerObject;
        GameObject playerPosition = GameObject.Find(_transformListings[position]);
        if(newPlayer == PhotonNetwork.LocalPlayer){
            playerObject = Instantiate(_player, playerPosition.transform);
        }
        else{
            playerObject = Instantiate(_enemyPlayerObjectList[position], playerPosition.transform);
            Enemy enemy = playerObject.GetComponent<Enemy>();
            enemy.SetInfoEnemyPlayer(newPlayer);
        }
        if(playerObject != null){
            playerObject.SetPlayerInfo(newPlayer);
            _mainPlayersListTemp.Add(playerObject);
        }
        // GameObject playerPosition = GameObject.Find(_transformListings[0]);
        // MainPlayer playerObject = Instantiate(_player, playerPosition.transform);
            //         GameObject playerPosition = GameObject.Find(_transformListings[playerInfo.Key-1]);
            // MainPlayer playerObject;
            // if(playerInfo.Value == PhotonNetwork.LocalPlayer){
            //     Debug.Log("Local Player: " + playerInfo.Value.NickName);
            //     playerObject = Instantiate(_player, playerPosition.transform);
            // }
            // else{
            //     playerObject = Instantiate(_enemy, playerPosition.transform);
            // }
            
            // playerObject.SetPlayerInfo(playerInfo.Value);
    }
    public void AddPlayerListing(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " just joined the room");
        PlayerListing listing = Instantiate(_playerListing,_content);
        if(listing != null){
            listing.SetPlayerInfo(newPlayer);
            listing.SetReady(false);
            _listings.Add(listing);
        }
        if(newPlayer != PhotonNetwork.MasterClient){
            
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);
        AddPlayerObject(newPlayer, PhotonNetwork.PlayerListOthers.Length);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer){
        Debug.Log(otherPlayer.NickName + " just left the room");
        int index = _listings.FindIndex(x => x.Player == otherPlayer);
        int indexMainPlayer = _mainPlayersListTemp.FindIndex(x => x.Player == otherPlayer);
        if(index != -1){
            Destroy(_listings[index].gameObject);
            _listings.RemoveAt(index);
        }
        if(indexMainPlayer != -1){
            Destroy(_mainPlayersListTemp[indexMainPlayer].gameObject);
            _mainPlayersListTemp.RemoveAt(indexMainPlayer);
        }
        RemovePlayerInTheList(otherPlayer);
        
    }
    private void RemovePlayerInTheList(Player otherPlayer){
        ExitGames.Client.Photon.Hashtable setPlayerValues = new ExitGames.Client.Photon.Hashtable();
        PhotonNetwork.CurrentRoom.Players.Remove(otherPlayer.ActorNumber);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
       // base.OnMasterClientSwitched(newMasterClient);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        playerDeck.ResetDeck();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PhotonNetwork.LeaveRoom();
        
    }

    [PunRPC]
    private void RPC_ChangeReadyState(Player player, bool ready){
        Debug.Log("RPC_ChangeReadyState() called");
        Debug.Log(player.NickName + " , Ready: " + ready);
        int index = _listings.FindIndex(x => x.Player == player);
        if(index != -1){
            _listings[index].SetReady(ready);
        }
    }

    [PunRPC]
    private void RPC_StartingGame(){
        _readyUpText.gameObject.SetActive(false);
        _readyButton.gameObject.SetActive(false);
        HideKickButtons();
        if(PhotonNetwork.IsMasterClient){
            UpdatePlayerList();
        }

        GameObject Hand = GameObject.Find("Hand");
        Debug.Log("Game start, the players in the room: ");
      //  MasterManager.NetworkInstantiate(_CardTohandPrefab, Hand.transform.position, Quaternion.identity, Hand);
        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players){
            Debug.Log(playerInfo.Key + "  " + playerInfo.Value);
        }
        Debug.Log("Players size = " + PhotonNetwork.CurrentRoom.Players.Count);
        GameManager.Instance.DeckSize = CardDatabase.CardList.Count*PhotonNetwork.CurrentRoom.PlayerCount;
        playerDeck.InstantiateCardsDeck();
        Background.GetComponent<PhotonView>().RPC("RPC_playerDeckStart", RpcTarget.All);
  //      playerDeck.ShuffleCard();
     //   GameObject.Find("Background Image").GetComponent<PhotonView>().RPC("RPC_ShuffleCard", RpcTarget.All);
        // foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players){
          //  GameObject.Find("Background Image").GetComponent<PhotonView>().RPC("RPC_GiveCardsToPlayer", RpcTarget.All, playerInfo.Value, 6);
       // }
    //    GameObject.Find("Background Image").GetComponent<PhotonView>().RPC("RPC_GiveCardsToPlayer", RpcTarget.All, th, 6);
    //    GameObject.Find("Background Image").GetComponent<PhotonView>().RPC("RPC_GiveCardsToPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer,6);
        if(PhotonNetwork.IsMasterClient){
            // playerDeck.AddCardsToDeck(PhotonNetwork.CurrentRoom.PlayerCount);
            playerDeck.ShuffleCard();
        }
        // playerDeck.ShuffleCard();
    //     List<Card> currentPlayerCards = playerDeck.GiveCardsToPlayer(PhotonNetwork.LocalPlayer, 6, playerDeck.TotalCardNumber);
         
    //     playerDecks.Add(PhotonNetwork.LocalPlayer, currentPlayerCards);
        
    //     ExitGames.Client.Photon.Hashtable setCardsValue = new ExitGames.Client.Photon.Hashtable();
    //     Debug.Log("playerCardsplay size = " + playerCardsPlay.Count);
    //     if(playerCardsPlay.Count > 0){
    //         playerCardsPlay = new Dictionary<Player, List<Card>>();
    //         int[] myPlayCards = new int[0];
    //         setCardsValue["PlayerCardsPlay"] = myPlayCards;
    //         Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setCardsValue));
            
    //     }
    //     playerCardsPlay.Add(PhotonNetwork.LocalPlayer, new List<Card>());
         

    //      Debug.Log("playerDecks size = " + playerDecks.Count);
    //      int[] myCards = new int[currentPlayerCards.Count];
    //      int i = 0;
    //      foreach(Card card in currentPlayerCards){
    //         myCards[i++] = card.Id;
            
    //      }
         
    // //     Debug.Log(myCards.Length);
    // //     // playerDecks.Add(PhotonNetwork.LocalPlayer, currentPlayerCards);
    // //   //  setCardsValue.Add("playerCards",myCards);
    //      setCardsValue["PlayerCards"] = myCards;
    //      Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setCardsValue));

    //     playerDeck.InstantiateCards(6,2 ,PhotonNetwork.LocalPlayer);
    //    Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("playerCards"));
        
    //    Debug.Log("playerDecks size = " + playerDecks.Count);
    

    
        // playerDeck.StartGame();
    }
    private void UpdatePlayerList(){
        int[] playerKeyLists = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        int i = 0;
        ExitGames.Client.Photon.Hashtable setPlayerKeysValue = new ExitGames.Client.Photon.Hashtable();
        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players){
            playerKeyLists[i++] = playerInfo.Key;
        }
        setPlayerKeysValue["PlayerKeys"] = playerKeyLists;
        Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setPlayerKeysValue));
    }
    private void HideKickButtons(){
        if(PhotonNetwork.IsMasterClient){

            GameObject[] kickButtons = GameObject.FindGameObjectsWithTag("KickButton");
            foreach(GameObject kickButton in kickButtons){
                kickButton.SetActive(false);
            }
                
        }
    }
    private void UnHideKickButton(){
        if(PhotonNetwork.IsMasterClient){

            GameObject[] kickButtons = GameObject.FindGameObjectsWithTag("KickButton");
            foreach(GameObject kickButton in kickButtons){
                kickButton.SetActive(true);
            }
        }
    }
    public void GiveCards(Player player,int numberOfCardsGivenToPlayer, int cardNumber ){
        List<Card> currentPlayerCards = playerDeck.GiveCardsToPlayer(player, numberOfCardsGivenToPlayer, cardNumber); 
         
        playerDecks.Add(player, currentPlayerCards);
        
        ExitGames.Client.Photon.Hashtable setCardsValue = new ExitGames.Client.Photon.Hashtable();
        Debug.Log("playerCardsplay size = " + playerCardsPlay.Count);
        if(playerCardsPlay.Count > 0){
            playerCardsPlay = new Dictionary<Player, List<Card>>();
            int[] myPlayCards = new int[0];
            setCardsValue["PlayerCardsPlay"] = myPlayCards;
            Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setCardsValue));
            
        }
        playerCardsPlay.Add(player, new List<Card>());
         

         Debug.Log("playerDecks size = " + playerDecks.Count);
         int[] myCards = new int[currentPlayerCards.Count];
         int i = 0;
         foreach(Card card in currentPlayerCards){
            myCards[i++] = card.Id;
            
         }
         
    //     Debug.Log(myCards.Length);
    //     // playerDecks.Add(PhotonNetwork.LocalPlayer, currentPlayerCards);
    //   //  setCardsValue.Add("playerCards",myCards);
         setCardsValue["PlayerCards"] = myCards;
         Debug.Log(PhotonNetwork.LocalPlayer.SetCustomProperties(setCardsValue));
        
        object auxiliaryNumberObject;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AuxiliaryNumber", out auxiliaryNumberObject);
        int auxiliaryNumber = (int) auxiliaryNumberObject;
        Debug.Log("Auxiliary Number = " + auxiliaryNumber);
        playerDeck.InstantiateCards(6,auxiliaryNumber ,PhotonNetwork.LocalPlayer);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == null || changedProps == null) {
            return;
        }
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        Debug.Log(targetPlayer.NickName + " properties changed");

        object arrayPlayerKeysObject;
        if(targetPlayer.CustomProperties.TryGetValue("PlayerKeys", out arrayPlayerKeysObject)){
            int[] playerKeysArray = (int[]) arrayPlayerKeysObject;
            List<Player> players = new List<Player>();
            for(int i = 0; i < playerKeysArray.Length;i++){
                players.Add(PhotonNetwork.CurrentRoom.Players[playerKeysArray[i]]);
            }
            GameManager.Instance.playerList = players;
            Debug.Log("Updated GameManager.Instance.playerList , size = " + GameManager.Instance.playerList.Count);
        }
        
        object arrayObject;
        if (targetPlayer.CustomProperties.TryGetValue("PlayerCards", out arrayObject)) {
            int[] cardIdArray = (int[])arrayObject;
            Debug.Log(targetPlayer.NickName + " , The length of the array is: " + cardIdArray.Length);
            List<Card> playerCards = new List<Card>();
            for(int i = 0; i < cardIdArray.Length; i++){
                playerCards.Add(CardDatabase.CardList[cardIdArray[i]]);
               
            }
            bool existedPlayer = playerDecks.ContainsKey(targetPlayer);
            if(!existedPlayer){
                playerDecks.Add(targetPlayer, playerCards);
                playerCardsPlay.Add(targetPlayer, new List<Card>());

            }
            else{
             //   int indexMainPlayer = playerDecks.Keys.ToList().IndexOf(targetPlayer);
                playerDecks[targetPlayer] = playerCards;
                
         
                
          //      Debug.Log("playerDecks updated");
            }
            int index = playerDecks.Keys.ToList().IndexOf(targetPlayer);
            List<Card> targetPlayerCards = playerDecks.Values.ElementAt(index);
            foreach(Card card in targetPlayerCards){
                Debug.Log(targetPlayer.NickName + " updated " + card.Name);
            }
           // Debug.Log("playerDecks size = " + playerDecks.Count);   
        }



        object arrayObject1;
        if (targetPlayer.CustomProperties.TryGetValue("PlayerCardsPlay", out arrayObject1)) {
            int[] playCardIdArray = (int[])arrayObject1;
            Debug.Log("The length of the array cards played is: " + playCardIdArray.Length);
            List<Card> playerCardsPlayList = new List<Card>();
            for(int i = 0; i < playCardIdArray.Length; i++){
                playerCardsPlayList.Add(CardDatabase.CardList[playCardIdArray[i]]);
               
            }
            bool existedPlayer = PlayerCardsPlay.ContainsKey(targetPlayer);
            if(!existedPlayer){
                playerCardsPlay.Add(targetPlayer, playerCardsPlayList);

            }
            else{
             //   int indexMainPlayer = playerDecks.Keys.ToList().IndexOf(targetPlayer);
                playerCardsPlay[targetPlayer] = playerCardsPlayList;
                if(targetPlayer == PhotonNetwork.LocalPlayer){

                    playerDeck.updateCardObjects(PhotonNetwork.LocalPlayer);
                }
                
         
                
               // Debug.Log("playerCardsPlay updated, size = " + playerCardsPlayList.Count);
            }
            int index = playerDecks.Keys.ToList().IndexOf(targetPlayer);
            List<Card> targetPlayerCardsPlay = playerCardsPlay.Values.ElementAt(index);
            // foreach(Card card in targetPlayerCardsPlay){
            //     Debug.Log(targetPlayer.NickName + " played " + card.Name);
            // }
            // Debug.Log("playerCardsPlay size = " + playerCardsPlayList.Count);   
        }

    }

}