using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    //Ip del servidor
    public string ip = "127.0.0.1";

    public int port = 26950;
    public int myId = 0;

    //Creamos la clase TCP que manejará la conexión de la misma forma que la hace el servidor
    public TCP tcp;

    //Creamos la clase UDP que manejará la conexión
    public UDP udp;
    //1 Añadimos un bool para conocer si el cliente se encuentra conectado o no
    private bool isConnected = false;

    //Un objeto delegate se utiliza para hacer ferencia a cualquier metodo que tenga como parámetro de entrada lo mismo
    private delegate void PacketHandler(Packet _packet);

    private static Dictionary<int, PacketHandler> packetHandlers;

    //Asegurarnos de que no hay más de una instancia de Client en el juego
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

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    //Creamos un método que nos permite llamar al connect de nuestra instancia de TCP
    public void ConnectToServer()
    {
        //Inicializamos el método
        InitializeClientData();

        isConnected = true;

        tcp.Connect();
    }

    // De forma similar al servidor
    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;

        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            // Esperamos a que se realice la conexión y hacemos algo cuando nos conseguimos conectar
            socket.BeginConnect(IPAddress.Parse(instance.ip), instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            // Cuando nos acabamos de conectar
            socket.EndConnect(_result);

            // El socket tiene diferentes estados, si no se ha conectado error
            if (!socket.Connected)
            {
                return;
            }

            // Si se ha conectado error obtenemos el stream
            stream = socket.GetStream();

            //Inicializamos el paquete una vez hemos realizado la conexión correcta
            receivedData = new Packet();

            //Una vez conectado debemos de crear un nuevo Callback cuando recibamos nueva información por el NetworkStream
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        //Método SendData, igual que en el servidor

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        //igual que el servidor
        private void ReceiveCallback(IAsyncResult _result)
        {
            //Si hay algun error no hace que el servidor crashee

            try
            {
                // Controlamos el fin de la lectura asíncrona y obtenemos los bytes recibidos, hay que controlar si hemmmos recibido algo o el cliente se ha desconectado)

                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                //Copiamos el buffer de recepción a un buffer temporal

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                //Tratamos los datos recibidos, lo haremos en el futuro
                receivedData.Reset(HandleData(_data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        //Analicemos los datos que recibimos
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;
            //Inicializamos el paquete que hemos creado con los bytes que hemos recibido
            receivedData.SetBytes(_data);

            //Si el paquete tiene al menos 4 bytes (que represetan un int) significa que lo primero que nos han enviado es un int, el cual es el tamaño del paquete
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                //Si el paquete tiene un tamaño de 0 pues no tiene informacións
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            //receivedData es todo el buffer de recepción, el cual puede ser mayor que nuestro tamaño de paquete
            //pueden llegarnos varios paquetes a la vez
            //por ello vamos a recoger varios paquetes hasta que el packet length sea 0 o no quede buffer no leido
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    //Creamos un paquete nuevo con los bytes que hemos leído, esto lo podemos
                    //hacer en el main thread para ya poder guardarnos los resultados
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        //La información que aparece en el paquete tras el tamaño es el tipo de paquete
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                //Volvemos a inicializar el packetLength y miramos el tamaño del próximo paquete
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        //3 Desconexión y reinicio de los buffer
        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    // Vamos a crear la nueva Clase UDP que se encargará de gestionar las conexiones por UDP
    //Los metodos son similares a TCP
    public class UDP
    {

        public UdpClient socket;

        public IPEndPoint endPoint;

        //Necesitamos inicializar un endPoint para luego poder recibir los datos correctamente
        //Esto no es más que la ip y el puerto de destino en un objeto específico
        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        //Creamos el método Connect
        //public void Connect(int _localPort)
        public void Connect()
        {
            //socket = new UdpClient(_localPort);
            //Inicializamos el socket
            socket = new UdpClient();

            //Conectamos con el servidor, no hace falta esperar respuesta a la conexión como en TCP porque nunca la habrá
            socket.Connect(endPoint);
            //Creamos un método asíncrono para recibir cualquier mensaje de vuelta del server
            socket.BeginReceive(ReceiveCallback, null);

            //
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                //recibe el stream de bytes y
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                //Recargará el BeginReceive por si llegan más datos.
                socket.BeginReceive(ReceiveCallback, null);

                //WARNING al ser UDP puede que algun paquete no llegue o llegue con algún fallo, habría que controlar este punto mejor ya que no podemos desconectar por solo 1 paquete defectuoso
                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            // Crea un paquete con los datos recibidos
            using (Packet _packet = new Packet(_data))
            {
                //El primer número es el tamaño del paquete
                int _packetLength = _packet.ReadInt();
                //Obtiene los bytes del paquete obviando el tamaño del paquete
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    //Una vez descartado el tamaño del paquete lo siente en leerse es el tipo de paquete
                    int _packetId = _packet.ReadInt();
                    //Utilizamos el método del ClientHandler para tratar con el tipo de paquete recibido
                    packetHandlers[_packetId](_packet);
                }
            });
        }
        //3 Desconexión y reinicio de los buffer
        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }

    }

    //Relación entre el tipo de paquete (hay un enum con los diferentes tipos en la clase Packet) y el método de la clase ClientHandle que lo debe de tratar
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
                { (int)ServerPackets.welcome, ClientHandle.Welcome },
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
                { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
                { (int)ServerPackets.playerEnemy, ClientHandle.PlayerToEnemy },
                { (int)ServerPackets.enemyPlayer, ClientHandle.EnemyToPlayer },
        };
        Debug.Log("Initialized packets.");
    }
    //2 Creamos un método que, si está conectado cierra los sockets
    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
    //2 Llamamos al método cuando se apague el cliente
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}