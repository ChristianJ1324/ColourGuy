using UnityEngine;

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
        ChangeColour(m_startingColour, true);
        // When a Block is added to a module, the colours are randomized and remapped
        // all blue blocks may become red


    }

    public void ChangeColour(Colour _newColour, bool _editorChange = false)
    {
        // DO NOT REMAP WHITE COLOURS unless changed from editor
        if (m_startingColour == Colour.White && !_editorChange) return;

        GetComponent<Rigidbody2D>().includeLayers = LayerMask.GetMask("Red", "Green", "Blue", "Yellow");


        GetComponent<SpriteRenderer>().color = _newColour switch
        {
            Colour.Red => Color.red,
            Colour.Green => Color.green,
            Colour.Blue => Color.blue,
            Colour.Yellow => Color.yellow,
            Colour.White => Color.white,
        };
        // Adjust Collision Mask to match Colour
        GetComponent<Rigidbody2D>().excludeLayers = _newColour switch
        {
            Colour.Red => LayerMask.GetMask("Red"),
            Colour.Green => LayerMask.GetMask("Green"),
            Colour.Blue => LayerMask.GetMask("Blue"),
            Colour.Yellow => LayerMask.GetMask("Yellow"),
            Colour.White => LayerMask.GetMask("Nothing"),
        };
    }
}
