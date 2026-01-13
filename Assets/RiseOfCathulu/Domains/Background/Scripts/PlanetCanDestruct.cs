using System;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

public class PlanetCanDestruct : MonoBehaviour
{
    [SerializeField] private Transform Player;
    private bool smallerThanPlayer;
    private bool guradianIsAlive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        GameEvents.PlanetEnemyDestroyed += CheckIfGuardianDeadInChild;
    }
    
    private void OnDisable()
    {
        GameEvents.PlanetEnemyDestroyed -= CheckIfGuardianDeadInChild;
    }
    
    


    private void CheckIfGuardianDeadInChild(Transform planet)
    {
        foreach (Transform child in planet)
        {
            if (child.CompareTag("Planet Enemy"))
            {
                guradianIsAlive = true;
                break;
            }
        }

        if (!guradianIsAlive)
        {
            CheckDestructable();
        }
    }

    private void CheckSmallerThanPlayer()
    {
        
    }

    private void CheckDestructable()
    {
        if (!guradianIsAlive && smallerThanPlayer)
        {
            return;
        }
    }
}
