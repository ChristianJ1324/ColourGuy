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
            if (!TrySpawnModule())
            {
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn in modules after they are destroyed 
        if (transform.childCount < m_NumModules)
        {
            TrySpawnModule();
        }


    }

    public void SpawnModule()
    {
        TrySpawnModule();
    }

    private bool TrySpawnModule()
    {
        // Will not spawn in modules if no modules to be spawned in
        if (m_ModulePrefabs == null || m_ModulePrefabs.Count == 0)
        {
            return false;
        }

        int validModuleCount = 0;
        foreach (Module module in m_ModulePrefabs)
        {
            if (module != null)
            {
                validModuleCount++;
            }
        }

        if (validModuleCount == 0)
        {
            return false;
        }

        // Select a random module from the list
        int moduleIndex = Random.Range(0, validModuleCount);
        Module prefab = null;
        foreach (Module module in m_ModulePrefabs)
        {
            if (module == null)
            {
                continue;
            }

            if (moduleIndex == 0)
            {
                prefab = module;
                break;
            }

            moduleIndex--;
        }

        prefab.m_Movespeed = m_ModuleMoveSpeed;

        float x = transform.position.x;

        if (transform.childCount > 0)
        {
            Transform lastModule = transform.GetChild(transform.childCount - 1);
            x = lastModule.position.x + m_ModuleWidth;
        }

        Vector3 pos = new Vector3(x, transform.position.y, 0f);
        Instantiate(prefab, pos, Quaternion.identity, transform);
        return true;
    }
}
