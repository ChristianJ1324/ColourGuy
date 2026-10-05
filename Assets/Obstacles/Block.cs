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
    public Colour m_startingColour;
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
        Debug.Log($"Colour changed to {m_startingColour}");
        ChangeColour(m_startingColour);

        // When a Block is added to a module, the colours are randomized and remapped
        // all blue blocks may become red
    }

    public void ChangeColour(Colour _newColour)
    {
        GetComponent<SpriteRenderer>().color = _newColour switch
        {
            Colour.Red => Color.red,
            Colour.Green => Color.green,
            Colour.Blue => Color.blue,
            Colour.Yellow => Color.yellow,
        };
        // Adjust Collision Mask to match Colour
        GetComponent<Rigidbody2D>().includeLayers = _newColour switch
        {
            Colour.Red => LayerMask.GetMask("Red"),
            Colour.Green => LayerMask.GetMask("Green"),
            Colour.Blue => LayerMask.GetMask("Blue"),
            Colour.Yellow => LayerMask.GetMask("Yellow"),

        };
    }
}
