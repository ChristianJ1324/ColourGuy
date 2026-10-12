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
        // Store Position
        Vector3 position = transform.position;

        // Move Module to the Left
        position.x += -m_Movespeed * Time.deltaTime;
        transform.position = position;

        // Destroy Module if too far off Screen
        if (position.x < 0)// TODO ADD MODULE SIZE and REPLACE WITH ACTUAL EDGE OF SCREEN
        {
            Destroy(gameObject);
        }

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
