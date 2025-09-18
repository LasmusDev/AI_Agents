using PlayerPoseEngine.Scripts;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PosemapPlayer : MonoBehaviour
{
    public int BPM;
    public Vector3 from;
    public Vector3 to;
    public int currBeat;
    public int visibleBeats;

    [SerializeField, ReadOnly]
    public int score = 0;

    [SerializeField, ReadOnly]
    public int combo = 1;

    [SerializeField, ReadOnly]
    private float poseSpeed;
    [SerializeField, ReadOnly]
    private float timePerBeat;

    public Posemap poseMap;
    public PosePool pool;


    List<PlayerPoseResolver> activePoseResolvers = new List<PlayerPoseResolver>();


    public void Start()
    {        
        timePerBeat = 60f / BPM;
        poseSpeed = (1f/(float)visibleBeats)/timePerBeat;
        StartPosemapPlayback();
    }

    

    public void StartPosemapPlayback()
    {
        StartCoroutine(PlayPosemap(poseMap));
    }

    public void StartPosemapPlayback(Posemap map)
    {
        poseMap = map;
        StartCoroutine(PlayPosemap(map));
    }

    public void StopPosemapPlayback()
    {
        StopAllCoroutines();
    }

    public void ScorePose(PlayerPoseResolver resolver, PlayerPose p)
    {
        activePoseResolvers.Remove(resolver);
        pool.Release(resolver);
        score += 100 * combo;
        combo++;
    }

    public IEnumerator PlayPosemap(Posemap map)
    {
        activePoseResolvers = new List<PlayerPoseResolver>();
        float timeSinceLastBeat = 0;
        while (true)
        {
            timeSinceLastBeat += Time.deltaTime;
            if (timeSinceLastBeat > timePerBeat)
            {
                timeSinceLastBeat -= timePerBeat;
                currBeat += 1;
                PlayerPose next = map.GetPose(currBeat - visibleBeats);
                if (next != null)
                {
                    PlayerPoseResolver res = pool.Get();                   
                    res.RequestPose(next);
                    res.transform.position = from;
                    res.transform.LookAt(res.transform.position + this.transform.forward);
                    activePoseResolvers.Add(res);
                    res.onPlayerPoseFulfilled += ScorePose;
                }
            }
            Vector3 movement = (to - from) * poseSpeed * Time.deltaTime;
            PlayerPoseResolver toRemove = null;
            foreach(PlayerPoseResolver resolver in activePoseResolvers)
            {
               
                resolver.transform.position += movement;
                if(Vector3.Distance(from, to) < Vector3.Distance(from, resolver.transform.position))
                {
                    pool.Release(resolver);     
                    toRemove = resolver;
                    combo = 1;
                }
            }
            if(toRemove != null)
            {
                activePoseResolvers.Remove(toRemove);
            }
            yield return null;
        }

    }
}
