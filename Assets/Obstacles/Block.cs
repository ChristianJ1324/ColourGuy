using UnityEngine;

public enum Colour
{ 
    Red,
    Green,
    Blue,
    Yellow,
}



public class Block : MonoBehaviour
{
    public Colour StartingColour;
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
        // Automatically change Colour of Block when Enum is updated in editor
        Debug.Log($"Colour changed to {StartingColour}");
        ChangeColour(StartingColour);

        // When a Block is added to a module, the colours are randomized and remapped
        // all blue blocks may become red
    }

    void ChangeColour(Colour NewColour)
    {
        GetComponent<SpriteRenderer>().color = StartingColour switch
        {
            Colour.Red => Color.red,
            Colour.Green => Color.green,
            Colour.Blue => Color.blue,
            Colour.Yellow => Color.yellow,
        };
        // Adjust Collision Mask to match Colour
        GetComponent<Rigidbody2D>().includeLayers = StartingColour switch
        {
            Colour.Red => LayerMask.GetMask("Red"),
            Colour.Green => LayerMask.GetMask("Green"),
            Colour.Blue => LayerMask.GetMask("Blue"),
            Colour.Yellow => LayerMask.GetMask("Yellow"),

        };
    }
}
