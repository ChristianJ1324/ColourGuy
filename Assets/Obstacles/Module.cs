using UnityEngine;
using UnityEngine.UIElements;

public class Module : MonoBehaviour
{
    public bool m_RandomColour = true;

    public float m_Movespeed = 0.1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Gradually move Right
        Vector3 position = transform.position;
        position.x += -m_Movespeed * Time.deltaTime;
        transform.position = position;
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
