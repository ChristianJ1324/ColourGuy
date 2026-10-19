using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Module : MonoBehaviour
{
    public bool m_RandomColour = true;

    public float m_Movespeed = 0.1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Randomize Colours 
        RemapColours();
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
        if (position.x < -12)// TODO ADD MODULE SIZE and REPLACE WITH ACTUAL EDGE OF SCREEN
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
                block.ChangeColour(block.m_startingColour);
            }
        }
    }

    void RemapColours()
    {
        // Pull Colours Enum
        var colours = (Colour[])System.Enum.GetValues(typeof(Colour));

        // Create a shuffled list
        var shuffled = (Colour[])colours.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        // Create Map of Colour -> Shuffled
        var map = new Dictionary<Colour, Colour>();
        for (int i = 0; i < colours.Length; i++)
        {
            map[colours[i]] = shuffled[i];
        }

        // Apply new colour map to blocks
        foreach (var block in GetComponentsInChildren<Block>())
        {
            block.ChangeColour(map[block.m_startingColour]);
        }
    }
}
