using Unity.VisualScripting;
using UnityEngine;

public class ParralaxEffect : MonoBehaviour
{
    private Material m_material;
    public float m_distance;
    public float m_parralaxSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        m_distance += Time.deltaTime * m_parralaxSpeed;
        m_material.SetTextureOffset("_MainTex", Vector2.right * m_distance);
    }
}
