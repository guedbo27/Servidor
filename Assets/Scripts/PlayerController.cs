using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendInputToServer();
    }

    //Cada fixed update vamos a enviar un array de bools indicando si se ha presionado las teclas wsad
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
        };
        //Para enviar dicho array debemos de crear un nuevo tipo de paquete, Player Movement
        ClientSend.PlayerMovement(_inputs);
    }
}