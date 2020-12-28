using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle
{
    public static void Welcome(Packet _packet)
    {
        //Lo tenemos que recibir en el mismo orden que lo hemos enviado en el servidor
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;

        //Responder al servidor
        ClientSend.WelcomeReceived();

        //Inicializamos la conexión UDP al servidor una vez nos hemos conectado por TCP
        //Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        Client.instance.udp.Connect();
    }

    //1 Recepción del paquete de tipo SpawnPlayer
    public static void SpawnPlayer(Packet _packet)
    {
        //Fijarnos en que orden hemos ido introduciendo los datos en el servidor para extraerlos  de la misma forma
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        //Llamamos al método SpawnPlayer que se va a encargar de Instanciar el jugador, ya sea local o remoto
        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    //2
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.ContainsKey(_id))
            GameManager.players[_id].transform.position = _position;
    }

    //Este paquete lo recibirán todos los clientes remotos, no el local
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();
        if (GameManager.players.ContainsKey(_id))
            GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerToEnemy(Packet _packet)
    {
        Componets.componets.isEnemy = _packet.ReadInt();
    }

    public static void EnemyToPlayer(Packet _packet)
    {
        Componets.componets.isEnemy = _packet.ReadInt();
    }
}
