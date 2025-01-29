using UnityEngine;


public class mParent : MonoBehaviour
{
    public GameObject mParentCon;

    private enum Mode
    {
        Idle,
        Ground,
        Hand,
        Back
    }

    private Mode m_Mode;

    public void Update()
    {

    }

    public void Start()
    {
        m_Mode = Mode.Ground;
        Debug.Log ("ground");
    }
    public void hand()
    {
        m_Mode = Mode.Hand;
        Debug.Log ("hand");
    }

    public void back()
    {
        m_Mode = Mode.Back;
        Debug.Log ("back");
    }
}
