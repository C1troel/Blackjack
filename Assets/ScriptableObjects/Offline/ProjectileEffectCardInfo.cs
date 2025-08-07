using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [CreateAssetMenu(fileName = "ProjectileEffectCard", menuName = "Infos/New ProjectileEffectCardInfo")]
    public class ProjectileEffectCardInfo : EffectCardInfo
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _spawnCordsOffset;

        public GameObject ProjectilePrefab => _projectilePrefab;
        public float SpawnCordsOffset => _spawnCordsOffset;
    }
}