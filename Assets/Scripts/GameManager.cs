using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //1 Crear para no duplicar la instancia
    public static GameManager instance;

    //2 Crear un diccionario de jugadores, que contendrá a todos los jugadores de la partida
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    //2 Prefabs para el jugador local y el remoto, por si son diferentes
    public GameObject localPlayerPrefab;

    public GameObject playerPrefab;

    //1 Nos permite que no tengamos 2 instancias aunque cambiemos de escena y estuviera en ambas
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //3 método que spawnea 1 jugador, lo llamaremos cuantas veces sea necesaria para crear los jugadores
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        //Si el id es el del cliente, spwneamos una instancia del prefab local, si no del remoto, aquí podemos rellenar la información de cada instancia que hagamos.
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }
        //Rellenamos la info del player Manager
        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        //Por último añadimos al Diccionario el jugador que acabamos de crear
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
