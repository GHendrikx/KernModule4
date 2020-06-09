using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpawnConnection : MonoBehaviour
{
    public GameObject[] Panels;
    [SerializeField]
    private Text textComponent;
    [SerializeField]
    private UnityEvent connect;
    ServerBehaviour serverBehaviour;
    ClientBehaviour clientBehaviour;
    GameObject go;

    public void Start()
    {
        for (int i = 0; i < Panels.Length; i++)
            Panels[i].SetActive(false);
        Panels[0].SetActive(true);
    }
    public void SpawnObject(int i)
    {
        textComponent.color = Color.black;
        if (textComponent.text != string.Empty)
        {
            if (i == 0)
            {
                serverBehaviour = new GameObject().AddComponent<ServerBehaviour>();
            }
            else if (i == 1)
                clientBehaviour = new GameObject().AddComponent<ClientBehaviour>();
            if(clientBehaviour != null)
                clientBehaviour.ClientName = textComponent.text;
            connect.Invoke();
        }
        else
            textComponent.text = "Need a name";

    }
}
