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
        GetComponent<SpriteRenderer>().color = StartingColour switch
        {
            Colour.Red    => Color.red,
            Colour.Green  => Color.green,
            Colour.Blue   => Color.blue,
            Colour.Yellow => Color.yellow,
        };
        GetComponent<Rigidbody2D>().includeLayers = StartingColour switch
        {
            Colour.Red => LayerMask.GetMask("Red"),
            Colour.Green => LayerMask.GetMask("Green"),
            Colour.Blue => LayerMask.GetMask("Blue"),
            Colour.Yellow => LayerMask.GetMask("Yellow"),

        };


    }
}
