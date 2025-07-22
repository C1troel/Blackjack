using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class ProjectileManager : MonoBehaviour
    {
        public List<IProjectile> avaitingProjectiles {  get; private set; } = new List<IProjectile>();

        public static ProjectileManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {

        }

        void Update()
        {}

        public void OnTimeStopEnd()
        {
            TryToStartProjectileActivity();
        }

        private void TryToStartProjectileActivity()
        {
            if (avaitingProjectiles.Count == 0)
                return;

            StartProjectileActivity(avaitingProjectiles[0]);
        }

        private void StartProjectileActivity(IProjectile projectile)
        {
            projectile.StartProjectileActivity(OnProjectileActivityEnd);
        }

        private void OnProjectileActivityEnd(IProjectile projectile)
        {
            avaitingProjectiles.Remove(projectile);
        }

        public void AddAvaitingProjectile(IProjectile projectile)
        {
            avaitingProjectiles.Add(projectile);
        }
    }
}