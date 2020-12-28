using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{
    #region Sender TCP and UDP

    //Igual que en el servidor, solo que en este caso no hace falta decir a que cliente se le envía, porque solo hay un server
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    //Creamos La clase SendUDPData con el tamaño del paquete
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #endregion Sender TCP and UDP

    #region Packets

    //2 de igual forma creamos el método WelcomeReceived que se encarga de enviar un mensaje de tipo Welcome
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

            SendUDPData(_packet);
        }
    }

    public static void CollisionEnemy(int _myId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.changeEnemy))
        {
            _packet.Write(_myId);

            SendTCPData(_packet);
        }
    }

    public static void CollisionPlayer(int _myId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.changePlayer))
        {
            _packet.Write(_myId);

            SendTCPData(_packet);
        }
    }
    #endregion Packets
}
