using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ModuleSpawner : MonoBehaviour
{
    public List<Module> m_ModulePrefabs; 
    public float m_ModuleMoveSpeed = 0.1f;
    public int m_NumModules = 3; // Amount of modules that are loaded into the game at once, may need to be increased with screen size
    public float m_ModuleWidth = 2f; // TEMPLATE NUMBER CHANGE ME

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Spawn in starting modules
        while (transform.childCount < m_NumModules)
        {
            SpawnModule();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn in modules after they are destroyed 
        if (transform.childCount < m_NumModules)
        {
            SpawnModule();
        }


    }

    public void SpawnModule()
    {
        Debug.Log($"Spawned Module #{transform.childCount}");
        // Will not spawn in modules if no modules to be spawned in
        if (m_ModulePrefabs.Count == 0) return;
        
        // Select a random module from the list
        Module prefab = m_ModulePrefabs[Random.Range(0, m_ModulePrefabs.Count)];

        prefab.m_Movespeed = m_ModuleMoveSpeed;

        float x = transform.position.x;

        if (transform.childCount > 0)
        {
            Transform lastModule = transform.GetChild(transform.childCount - 1);
            x = lastModule.position.x + m_ModuleWidth;
        }

        Vector3 pos = new Vector3(x, transform.position.y, 0f);
        Instantiate(prefab, pos, Quaternion.identity, transform);
    }
}
