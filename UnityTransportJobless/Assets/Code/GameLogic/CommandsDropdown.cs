using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandsDropdown : MonoBehaviour
{
    public Dropdown dropDown;
    public Chat chat;


    // Invoke button
    public void InvokeButton()
    {
        chat.lines.Add(dropDown.options[dropDown.value].text);    
    }
}
