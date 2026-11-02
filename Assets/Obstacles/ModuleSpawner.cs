using UnityEngine;
using System.Collections.Generic;

public class ModuleSpawner : MonoBehaviour
{
    public List<Module> m_ModulePrefabs; 
    public float m_ModuleMoveSpeed = 0.1f;
    public int m_NumModules = 3; // Amount of modules that are loaded into the game at once, may need to be increased with screen size
    public float m_ModuleWidth = 2f; // TEMPLATE NUMBER CHANGE ME
    private Vector3 m_startPosition;

    public float CurrentMoveSpeed
    {
        get
        {
            if (Time.timeScale <= 0f)
            {
                return 0f;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Module module = transform.GetChild(i).GetComponent<Module>();
                if (module != null && module.isActiveAndEnabled)
                {
                    return module.m_Movespeed;
                }
            }

            return 0f;
        }
    }

    private void Awake()
    {
        m_startPosition = transform.position;
    }

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

    public void ResetModules()
    {
        while (transform.childCount > 0)
        {
            Transform module = transform.GetChild(0);
            module.gameObject.SetActive(false);
            module.SetParent(null);
            Destroy(module.gameObject);
        }

        for (int i = 0; i < m_NumModules; i++)
        {
            Vector3 position = m_startPosition + Vector3.right * (i * m_ModuleWidth);
            if (!TrySpawnModule(position))
            {
                break;
            }
        }
    }

    private bool TrySpawnModule()
    {
        float x = transform.position.x;

        if (transform.childCount > 0)
        {
            Transform lastModule = transform.GetChild(transform.childCount - 1);
            x = lastModule.position.x + m_ModuleWidth;
        }

        Vector3 pos = new Vector3(x, transform.position.y, 0f);
        return TrySpawnModule(pos);
    }

    private bool TrySpawnModule(Vector3 position)
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
        Instantiate(prefab, position, Quaternion.identity, transform);
        return true;
    }
}
