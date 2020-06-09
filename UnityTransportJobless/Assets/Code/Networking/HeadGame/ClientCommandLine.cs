using UnityEngine;
using UnityEngine.UI;

public class ClientCommandLine : MonoBehaviour
{
    private ClientBehaviour clientBehaviour;
    [SerializeField]
    private Dropdown dropDown;
    public Dropdown DirectionDropDown;

    public void Start()
    {
        clientBehaviour = FindObjectOfType<ClientBehaviour>();

    }

    public void InvokeButton()
    {
        int i = dropDown.value;
        switch (i)
        {
            case 0:
                string direction = DirectionDropDown.options[DirectionDropDown.value].text;
                Debug.Log(direction);
                MoveMessage moveMessage = new MoveMessage();

                switch (direction)
                {
                    case "North":
                        moveMessage.direction = Direction.North;
                        break;
                    case "East":
                        moveMessage.direction = Direction.East;
                        break;
                    case "South":
                        moveMessage.direction = Direction.South;
                        break;
                    case "West":
                        moveMessage.direction = Direction.West;
                        break;
                }
                clientBehaviour.SendMessage(moveMessage);
                break;
            case 1:
                AttackRequestMessage attackRequestMessage = new AttackRequestMessage();
                Debug.Log("Attack");
                clientBehaviour.SendMessage(attackRequestMessage);
                break;
            case 2:
                DefendRequestMessage defendRequestMessage = new DefendRequestMessage();
                Debug.Log("Defend");
                clientBehaviour.SendMessage(defendRequestMessage);
                break;
            case 3:
                ClaimTreasureRequestMessage claimTreasureRequestMessage = new ClaimTreasureRequestMessage();
                Debug.Log("Claim Treasure");
                clientBehaviour.SendMessage(claimTreasureRequestMessage);
                break;
            case 4:
                LeaveDungeonRequest leaveDungeonRequest = new LeaveDungeonRequest();
                Debug.Log("Leave Dungeon");
                clientBehaviour.SendMessage(leaveDungeonRequest);
                break;

        }




    }

    public void DropDownIndex()
    {
        int s = dropDown.value;
        if (s != 0)
            DirectionDropDown.gameObject.SetActive(false);
        else
            DirectionDropDown.gameObject.SetActive(true);
    }
}

