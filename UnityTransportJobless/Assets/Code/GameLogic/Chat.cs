using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public List<string> lines = new List<string>();
    [SerializeField]
    private Text GUI;

    [SerializeField]
    private Dropdown DirectionDropDown;

    // Update is called once per frame
    public void UpdateText()
    {
        string GUILines = string.Empty;

        for(int i = 0; i<lines.Count;i++)
            GUILines += lines[i] + "\n";

        GUI.text = GUILines;
    }

}
