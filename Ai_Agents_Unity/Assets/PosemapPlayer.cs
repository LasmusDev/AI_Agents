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
    private float poseSpeed;
    [SerializeField, ReadOnly]
    private float timePerBeat;

    public Posemap poseMap;
    public PosePool pool;



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

    public IEnumerator PlayPosemap(Posemap map)
    {
        List<PlayerPoseResolver> activePoseResolvers = new List<PlayerPoseResolver>();
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
                    activePoseResolvers.Add(res);
                }
            }
            Vector3 movement = (from - to) * poseSpeed * Time.deltaTime;
            PlayerPoseResolver toRemove = null;
            foreach(PlayerPoseResolver resolver in activePoseResolvers)
            {
                resolver.transform.Translate(movement);
                if(Vector3.Distance(from, to) < Vector3.Distance(from, resolver.transform.position))
                {
                    pool.Release(resolver);     
                    toRemove = resolver;
                    //TODO: Failure Event
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
