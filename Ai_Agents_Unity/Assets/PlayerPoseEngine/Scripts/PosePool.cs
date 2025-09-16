using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace PlayerPoseEngine.Scripts {

    public class PosePool : MonoBehaviour
    {
        public List<PlayerPoseResolver> pool;
        public List<bool> inUse;
        public int currIndex;
        public GameObject poolPrefab;

        public void Start()
        {           
            if(inUse.Count < pool.Count)
            {
                bool[] bools = new bool[pool.Count - inUse.Count];
                inUse.AddRange(bools);
            }         
        }

        public PlayerPoseResolver Get()
        {
            if (inUse.All(x => x))
            {
                ExpandPool();
            }
            while (inUse[currIndex])
            {
                currIndex = (currIndex + 1) % inUse.Count;
            }
            inUse[currIndex] = true;
            pool[currIndex].gameObject.SetActive(true);
            return pool[currIndex];

        }

        public void Release(int index)
        {
            inUse[index] = false;
            pool[index].gameObject.SetActive(false);
        }

        public void Release(PlayerPoseResolver resolver)
        {
            resolver.gameObject.SetActive(false);
            inUse[pool.IndexOf(resolver)] = false;            
        }

        public void ExpandPool()
        {
            inUse.Add(false);
            pool.Add(Instantiate(poolPrefab, this.transform).GetComponent<PlayerPoseResolver>());
        }
    }
}
