using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParralaxEffect : MonoBehaviour
{
    private Material m_material;
    public float m_distance;
    public float m_parralaxSpeed;

    [Serializable]
    private class ParallaxLayer
    {
        public Sprite[] m_sprites;
        public float m_speedFactor;
    }

    private class LayerTiles
    {
        public Transform[] m_tiles;
        public float m_width;
        public float m_speedFactor;
    }

    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_groundLevel = 1f;
    [SerializeField] private ParallaxLayer[] m_layers;

    private readonly List<LayerTiles> m_layerTiles = new List<LayerTiles>();
    private ModuleSpawner m_moduleSpawner;

    private void Awake()
    {
        if (m_layers == null || m_layers.Length == 0)
        {
            return;
        }

        if (m_camera == null)
        {
            m_camera = Camera.main;
        }

        m_moduleSpawner = FindFirstObjectByType<ModuleSpawner>();
        CreateLayers();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            m_material = renderer.material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_layers != null && m_layers.Length > 0)
        {
            UpdateLayers();
            return;
        }

        if (m_material == null)
        {
            return;
        }

        m_distance += Time.deltaTime * m_parralaxSpeed;
        m_material.SetTextureOffset("_MainTex", Vector2.right * m_distance);
    }

    private void UpdateLayers()
    {
        if (m_moduleSpawner == null)
        {
            m_moduleSpawner = FindFirstObjectByType<ModuleSpawner>();
        }

        if (m_camera == null || m_moduleSpawner == null)
        {
            return;
        }

        float speed = m_moduleSpawner.CurrentMoveSpeed;
        if (speed <= 0f)
        {
            return;
        }

        float cameraLeft = m_camera.transform.position.x - m_camera.orthographicSize * m_camera.aspect;
        foreach (LayerTiles layer in m_layerTiles)
        {
            float movement = speed * layer.m_speedFactor * Time.deltaTime;
            if (movement <= 0f)
            {
                continue;
            }

            foreach (Transform tile in layer.m_tiles)
            {
                tile.position += Vector3.left * movement;
            }

            Transform leftTile = layer.m_tiles[0].position.x < layer.m_tiles[1].position.x
                ? layer.m_tiles[0]
                : layer.m_tiles[1];
            Transform rightTile = leftTile == layer.m_tiles[0] ? layer.m_tiles[1] : layer.m_tiles[0];

            if (leftTile.position.x + layer.m_width <= cameraLeft)
            {
                leftTile.position = new Vector3(rightTile.position.x + layer.m_width, leftTile.position.y, leftTile.position.z);
            }
        }
    }

    private void CreateLayers()
    {
        if (m_camera == null || m_layers == null || m_layers.Length == 0)
        {
            return;
        }

        float cameraHeight = m_camera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * m_camera.aspect;
        ParallaxLayer groundLayer = m_layers[m_layers.Length - 1];
        if (groundLayer.m_sprites == null || groundLayer.m_sprites.Length == 0 || groundLayer.m_sprites[0] == null)
        {
            return;
        }

        float groundScale = GetLayerScale(groundLayer, cameraWidth, cameraHeight);
        Sprite ground = groundLayer.m_sprites[0];
        float backgroundBottom = m_groundLevel - ground.rect.yMax / ground.pixelsPerUnit * groundScale;

        for (int i = 0; i < m_layers.Length; i++)
        {
            ParallaxLayer layer = m_layers[i];
            if (layer.m_sprites == null || layer.m_sprites.Length == 0 || layer.m_sprites[0] == null)
            {
                continue;
            }

            Sprite firstSprite = layer.m_sprites[0];
            float scale = GetLayerScale(layer, cameraWidth, cameraHeight);
            float width = firstSprite.texture.width / firstSprite.pixelsPerUnit * scale;
            float left = m_camera.transform.position.x - width * 0.5f;
            LayerTiles tiles = new LayerTiles
            {
                m_tiles = new Transform[2],
                m_width = width,
                m_speedFactor = layer.m_speedFactor
            };

            for (int tileIndex = 0; tileIndex < tiles.m_tiles.Length; tileIndex++)
            {
                GameObject tile = new GameObject("Parallax Layer " + i + " Tile " + tileIndex);
                tile.transform.SetParent(transform, false);
                tile.transform.position = new Vector3(left + width * tileIndex, backgroundBottom, 0f);
                tiles.m_tiles[tileIndex] = tile.transform;

                foreach (Sprite sprite in layer.m_sprites)
                {
                    if (sprite == null)
                    {
                        continue;
                    }

                    GameObject image = new GameObject(sprite.name);
                    image.transform.SetParent(tile.transform, false);
                    image.transform.localPosition = new Vector3(
                        (sprite.rect.x + sprite.pivot.x) / sprite.pixelsPerUnit * scale,
                        (sprite.rect.y + sprite.pivot.y) / sprite.pixelsPerUnit * scale,
                        0f);
                    image.transform.localScale = Vector3.one * scale;

                    SpriteRenderer renderer = image.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = -20 + i;
                }
            }

            m_layerTiles.Add(tiles);
        }
    }

    private static float GetLayerScale(ParallaxLayer layer, float cameraWidth, float cameraHeight)
    {
        Sprite sprite = layer.m_sprites[0];
        float textureWidth = sprite.texture.width / sprite.pixelsPerUnit;
        float textureHeight = sprite.texture.height / sprite.pixelsPerUnit;
        return Mathf.Max(cameraWidth / textureWidth, cameraHeight / textureHeight) * 1.01f;
    }
}
