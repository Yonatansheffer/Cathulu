using System;
using System.Collections.Generic;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

public class PlanetCanDestruct : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject destructibleLight;
    
    private const string NORMAL_PLANET_TAG = "Planet";
    private const string DESTRUCTIBLE_PLANET_TAG = "Destructable Planet";


    private bool smallerThanPlayer;
    private bool guardianIsAlive = true;
    private bool canBeDestroyed;
    
    private List<Transform> guardians = new List<Transform>();

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        GameEvents.PlayerChangeSize += CheckSmallerThanPlayer;
    }
    
    private void OnDisable()
    {
        GameEvents.PlayerChangeSize -= CheckSmallerThanPlayer;
    }
    
    private void Start()
    {
        // Auto-assign Player
        if (Player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure the Player has the 'Player' tag.");
            }
        }

        // Auto-find destructible light sprite child
        if (destructibleLight == null)
        {
            Transform lightTransform = transform.Find("Light");

            if (lightTransform != null)
            {
                destructibleLight = lightTransform.gameObject;
                destructibleLight.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"{name}: Light sprite child not found.");
            }
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("Planet Enemy"))
                guardians.Add(child);
        }
    }

    private void Update()
    {
        guardians.RemoveAll(g => g == null);

        // If the list is empty, no guardians remain
        guardianIsAlive = guardians.Count > 0;

        // Update destructible status
        CheckDestructable();
    }


    private void CheckSmallerThanPlayer()
    {
        if (Player == null)
            return;

        // Find the visual child
        Transform planetVisuals = transform.Find("Planet Visuals");
        if (planetVisuals == null)
        {
            Debug.LogWarning($"{name}: Planet Visuals child not found, using root scale instead.");
            planetVisuals = transform; // fallback
        }

        float playerSize = Player.localScale.magnitude;
        float planetSize = planetVisuals.localScale.magnitude;

        smallerThanPlayer = planetSize < playerSize;

        CheckDestructable();
    }


    private void CheckDestructable()
    {
        bool shouldBeDestructible = !guardianIsAlive && smallerThanPlayer;
        SetDestructible(shouldBeDestructible);
    }

    private void SetDestructible(bool value)
    {
        if (canBeDestroyed == value)
            return;

        canBeDestroyed = value;

        // Change tag
        gameObject.tag = value ? DESTRUCTIBLE_PLANET_TAG : NORMAL_PLANET_TAG;

        // Visual feedback
        if (destructibleLight != null)
            destructibleLight.SetActive(value);

        Debug.Log(value
            ? $"{name} is now destructible!"
            : $"{name} is no longer destructible!");
    }

    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!canBeDestroyed)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            DestroyPlanet();
        }
    }
    
    private void DestroyPlanet()
    {
        Destroy(gameObject);
    }
}
