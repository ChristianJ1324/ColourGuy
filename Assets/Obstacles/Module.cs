using UnityEngine;

public class Module : MonoBehaviour
{
    public bool m_RandomColour = true;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Block>(out var block))
            {
                block.m_startingColour = m_RandomColour ? Colour.Blue : Colour.Red;
                block.ChangeColour(block.m_startingColour);
            }
        }
    }
}
