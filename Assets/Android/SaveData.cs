using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    private string gemsFilepath = Application.persistentDataPath + "/gemcount.txt";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveGems(int _amount)
    {
        string saveData = System.IO.File.ReadAllText(gemsFilepath); // Read File
        int currentGemCount = int.Parse(saveData); // Convert to Int
        int newGemCount = currentGemCount += _amount; // Add with new amount
        string newGemAmount = newGemCount.ToString(); // Convert back to String

        System.IO.File.WriteAllText(gemsFilepath, newGemAmount);
    }
}
