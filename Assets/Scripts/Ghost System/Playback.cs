using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using static Playback;

public class Playback : MonoBehaviour
{
    private List<PlaybackKeyFrame> playbackKeyFrames = new List<PlaybackKeyFrame>();
    private float playerTimer;

    [SerializeField] private GameObject target;
    private Animator targetAnimator;
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int JumpState = Animator.StringToHash("JumpState");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int WallGrabbing = Animator.StringToHash("WallGrabbing");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");

    [SerializeField] private GameObject ghostPrefab;
    private GameObject ghost;
    private Animator ghostAnimator;

    [Range(0f, 1f)]
    [SerializeField] private float timeBetweenKeyFrames;

    private bool isRecording = false;
    private bool isPlaybacking = false;
    private bool isPlaybackDone = false;

    public static event Action playbackDoneEvent;

    [SerializeField] private CinemachineCamera virtualCamera;
    public struct PlaybackKeyFrame
    {
        public float time;
        public Vector3 pos;
        public Quaternion rot;
        public float moveSpeedAnimator;
        public float jumpSpeedAnimator;
        public bool isJumpingAnimator;
        public bool isGrabbingAnimator;
        public bool isDashingAnimator;

    }

    public void SetIsRecording(bool value)
    {
        isRecording = value;
    }

    public void SetIsPlaybacking(bool value)
    {
        isPlaybacking = value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetAnimator = target.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            Record();
        }
        if (isPlaybacking && !isPlaybackDone)
        {
            PlaybackUpdate(playerTimer);
        }
        if (isPlaybackDone)
        {
            Destroy(ghost);
        }

        playerTimer += Time.deltaTime;
    }


    public void Record()
    {
        if (playbackKeyFrames.Count == 0 || playerTimer - playbackKeyFrames[^1].time > timeBetweenKeyFrames)
        {
            PlaybackKeyFrame frame = new PlaybackKeyFrame()
            {
                time = playerTimer,
                pos = target.transform.position,
                rot = target.transform.rotation,
                moveSpeedAnimator = targetAnimator.GetFloat(Move),
                jumpSpeedAnimator = targetAnimator.GetFloat(JumpState),
                isJumpingAnimator = targetAnimator.GetBool(IsJumping),
                isGrabbingAnimator = targetAnimator.GetBool(WallGrabbing),
                isDashingAnimator = targetAnimator.GetBool(IsDashing)
            };

            playbackKeyFrames.Add(frame);
        }
    }

    public void PlaybackUpdate(float playTime)
    {
        if (playbackKeyFrames.Count <= 1) return;

        int prevIndex = 0;
        int nextIndex = playbackKeyFrames.Count - 1;
        int i = (nextIndex) / 2;
        int safety = 10000;

        while (true)
        {
            // t lies to left
            if (playTime <= playbackKeyFrames[i].time)
            {
                nextIndex = i;
            }
            // t lies to right
            else
            {
                prevIndex = i;
            }

            i = (nextIndex + prevIndex) / 2;

            if (nextIndex - prevIndex <= 1)
            {
                break;
            }

            safety--;
            if (safety <= 0)
            {
                Debug.Log("Fix me!");
                return;
            }
        }

        if(nextIndex ==  playbackKeyFrames.Count - 1)
        {
            isPlaybackDone = true;
            virtualCamera.Follow = target.transform;
            playbackDoneEvent?.Invoke();
        }

        PlaybackKeyFrame frameA = playbackKeyFrames[prevIndex];
        PlaybackKeyFrame frameB = playbackKeyFrames[nextIndex];
        float abPercent = Mathf.InverseLerp(frameA.time, frameB.time, playTime);

        ghost.transform.position = Vector3.Lerp(frameA.pos, frameB.pos, abPercent);
        ghost.transform.rotation = Quaternion.Slerp(frameA.rot, frameB.rot, abPercent);
        ghostAnimator.SetFloat(Move, Mathf.Lerp(frameA.moveSpeedAnimator, frameB.moveSpeedAnimator, abPercent));
        ghostAnimator.SetFloat(JumpState, Mathf.Lerp(frameA.jumpSpeedAnimator, frameB.jumpSpeedAnimator, abPercent));
        ghostAnimator.SetBool(IsJumping, frameA.isJumpingAnimator);
        ghostAnimator.SetBool(WallGrabbing, frameA.isGrabbingAnimator);
        ghostAnimator.SetBool(IsDashing, frameA.isDashingAnimator);
    }

    public string SaveDatas()
    {
        string savedDatas = "";

        for (int i = 0; i < playbackKeyFrames.Count; i++)
        {
            savedDatas += playbackKeyFrames[i].time.ToString() + "|" + SerializeVector3(playbackKeyFrames[i].pos) + "|" + SerializeQuarternion(playbackKeyFrames[i].rot) + "|" +
                          playbackKeyFrames[i].moveSpeedAnimator.ToString() + "|" + playbackKeyFrames[i].jumpSpeedAnimator.ToString() + "|" + playbackKeyFrames[i].isJumpingAnimator + "|" +
                           playbackKeyFrames[i].isGrabbingAnimator + "|" + playbackKeyFrames[i].isDashingAnimator;
            if(i < playbackKeyFrames.Count - 1)
            {
                savedDatas += "/";
            }
        }

        return savedDatas;
    }

    private string SerializeVector3(Vector3 v)
    {
        return v.x.ToString() + "_" + v.y.ToString() + "_" + v.z.ToString();
    }

    private Vector3 DeserializeVector3(string s)
    {
        return new Vector3(float.Parse(s.Split('_')[0]), float.Parse(s.Split('_')[1]), float.Parse(s.Split('_')[2]));
    }

    private string SerializeQuarternion(Quaternion q)
    {
        return q.x.ToString() + "_" + q.y.ToString() + "_" + q.z.ToString() + "_" + q.w.ToString(); 
    }

    private Quaternion DeserializeQuaternion(string s)
    {
        return new Quaternion(float.Parse(s.Split('_')[0]), float.Parse(s.Split('_')[1]), float.Parse(s.Split('_')[2]), float.Parse(s.Split('_')[3]));
    }

    public void LoadDatas(string savedDatas)
    {
        playbackKeyFrames.Clear();
        playerTimer = 0;
        isRecording = false;

        playbackKeyFrames = GetPlaybackKeyFramesFromString(savedDatas);

        ghost = Instantiate(ghostPrefab, playbackKeyFrames[0].pos, playbackKeyFrames[0].rot);
        ghostAnimator = ghost.GetComponentInChildren<Animator>();
        playerTimer = playbackKeyFrames[0].time;
        virtualCamera.Follow = ghost.transform;
    }


    private List<PlaybackKeyFrame> GetPlaybackKeyFramesFromString(string datas)
    {
        List<PlaybackKeyFrame> localKeyFrames = new List<PlaybackKeyFrame>();

        string[] splitStrings = datas.Split('/');
        for (int i = 0; i < splitStrings.Length; i++)
        {
            string splitString = splitStrings[i];

            string timeString = splitString.Split('|')[0];
            string posString = splitString.Split('|')[1];
            string rotString = splitString.Split('|')[2];
            string moveSpeedAnimatorString = splitString.Split('|')[3];
            string jumpSpeedAnimatorString = splitString.Split('|')[4];
            string isJumpingAnimatorString = splitString.Split('|')[5];
            string isGrabbingAnimatorString = splitString.Split('|')[6];
            string isDashingAnimatorString = splitString.Split('|')[7];

            float _time = float.Parse(timeString);
            Vector3 _pos = DeserializeVector3(posString);
            Quaternion _rot = DeserializeQuaternion(rotString);
            float _moveSpeedAnimator = float.Parse(moveSpeedAnimatorString);
            float _jumpSpeedAnimator = float.Parse(jumpSpeedAnimatorString);
            bool isJumpingAnimator = bool.Parse(isJumpingAnimatorString);
            bool isGrabbingAnimator = bool.Parse(isGrabbingAnimatorString);
            bool isDashingAnimator = bool.Parse(isDashingAnimatorString);

            PlaybackKeyFrame frame = new PlaybackKeyFrame()
            {
                time = _time,
                pos = _pos,
                rot = _rot,
                moveSpeedAnimator = _moveSpeedAnimator,
                jumpSpeedAnimator = _jumpSpeedAnimator,
                isJumpingAnimator = isJumpingAnimator,
                isGrabbingAnimator = isGrabbingAnimator,
                isDashingAnimator = isDashingAnimator
            };

            localKeyFrames.Add(frame);
        }

        return localKeyFrames;
    }

    public float CompareRuns(string currentRun, string savedRun, float accuracyThreshold, int frameThreshold)
    {
        List<PlaybackKeyFrame> currentRunKeyFrames = new List<PlaybackKeyFrame>();
        List<PlaybackKeyFrame> savedRunKeyFrames = new List<PlaybackKeyFrame>();

        currentRunKeyFrames = GetPlaybackKeyFramesFromString(currentRun);
        savedRunKeyFrames = GetPlaybackKeyFramesFromString(savedRun);

        int minFrameCount = Mathf.Min(currentRunKeyFrames.Count, savedRunKeyFrames.Count);
        int maxFrameCount = Mathf.Max(currentRunKeyFrames.Count, savedRunKeyFrames.Count);

        float sameValues = 0;

        for (int i = 0; i < minFrameCount; i++)
        {
            for (int j = 0; j < frameThreshold; j++)
            {
                if (i + j < currentRunKeyFrames.Count)
                {
                    if (Vector3.Distance(currentRunKeyFrames[i + j].pos, savedRunKeyFrames[i].pos) <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                }
                if (i - j >= 0)
                {
                    if (Vector3.Distance(currentRunKeyFrames[i - j].pos, savedRunKeyFrames[i].pos) <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                }
            }
        }

        Debug.Log(sameValues + "/" + maxFrameCount);

        return sameValues / maxFrameCount;
    }

    private float CompareCurve(AnimationCurve curve1, AnimationCurve curve2, float accuracyThreshold, int frameThreshold)
    {
        float sameValues = 0f;

        int minLength = Mathf.Min(curve1.length, curve2.length);

        for (int i = 0; i < minLength; i++)
        {
            for (int j = 0; j < frameThreshold; j++)
            {
                if (i + j < curve1.length)
                {
                    if (Mathf.Abs(curve1.keys[i + j].value - curve2.keys[i].value) <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                }
                if (i - j >= 0)
                {
                    if (Mathf.Abs(curve1.keys[i - j].value - curve2.keys[i].value) <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                }
            }
        }

        return sameValues;
    }
}
