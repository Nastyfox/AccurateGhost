using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class Playback : MonoBehaviour
{
    public enum TriggerType
    {
        Jump,
        Wall,
        Land
    }

    private List<PlaybackKeyFrame> playbackKeyFrames = new List<PlaybackKeyFrame>();
    private float playerTimer;

    [SerializeField] private GameObject target;
    private Animator targetAnimator;
    private SpriteRenderer targetSpriteRenderer;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Wall = Animator.StringToHash("Wall");
    private static readonly int Land = Animator.StringToHash("Land");

    private bool pendingJump = false;
    private bool pendingWall = false;
    private bool pendingLand = false;

    [SerializeField] private GameObject ghostPrefab;
    private GameObject ghost;
    private Animator ghostAnimator;
    private SpriteRenderer ghostSpriteRenderer;

    [Range(0f, 1f)]
    [SerializeField] private float timeBetweenKeyFrames;

    private bool isRecording = false;
    private bool isPlaybacking = false;
    private bool isPlaybackDone = false;

    public static event Action playbackDoneEvent;
    private bool startRunAfterPlayback = false;

    private int frameOffset = 0;

    [SerializeField] private CinemachineCamera virtualCamera;
    public struct PlaybackKeyFrame
    {
        public float time;
        public Vector3 pos;
        public bool flipXSprite;
        public bool isMovingAnimator;
        public bool jumpAnimator;
        public bool wallAnimator;
        public bool landAnimator;

    }

    public void SetIsRecording(bool value)
    {
        isRecording = value;
        if(isRecording)
        {
            playerTimer = 0;
        }
    }

    public void SetIsPlaybacking(bool value)
    {
        isPlaybacking = value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target != null)
        {
            targetAnimator = target.GetComponentInChildren<Animator>();
            targetSpriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaybacking && !isPlaybackDone)
        {
            PlaybackUpdate(playerTimer);
        }
        if (isPlaybackDone)
        {
            Destroy(ghost);
        }
    }

    private void FixedUpdate()
    {
        playerTimer += Time.fixedDeltaTime;

        if (isRecording)
        {
            Record();
        }
    }


    public void Record()
    {
        if (playbackKeyFrames.Count == 0 || playerTimer - playbackKeyFrames[^1].time > timeBetweenKeyFrames)
        {
            PlaybackKeyFrame frame = new PlaybackKeyFrame()
            {
                time = playerTimer,
                pos = target.transform.position,
                flipXSprite = targetSpriteRenderer.flipX,
                isMovingAnimator = targetAnimator.GetBool(IsMoving),
                jumpAnimator = pendingJump,
                wallAnimator = pendingWall,
                landAnimator = pendingLand
            };

            playbackKeyFrames.Add(frame);

            pendingJump = false;
            pendingWall = false;
            pendingLand = false;
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

        if ((nextIndex + frameOffset) == playbackKeyFrames.Count - 1)
        {
            isPlaybackDone = true;
            isPlaybacking = false;
            if (target != null)
            {
                virtualCamera.Follow = target.transform;
            }
            if(startRunAfterPlayback)
            {
                playbackDoneEvent?.Invoke();
            }
        }

        PlaybackKeyFrame frameA = playbackKeyFrames[prevIndex + frameOffset];
        PlaybackKeyFrame frameB = playbackKeyFrames[nextIndex + frameOffset];
        float abPercent = Mathf.InverseLerp(frameA.time, frameB.time, playTime);

        ghost.transform.position = Vector3.Lerp(frameA.pos, frameB.pos, abPercent);
        ghostSpriteRenderer.flipX = frameA.flipXSprite;
        ghostAnimator.SetBool(IsMoving, frameA.isMovingAnimator);
        if (frameA.jumpAnimator)
        {
            ghostAnimator.SetTrigger(Jump);
        }
        if (frameA.wallAnimator)
        {
            ghostAnimator.SetTrigger(Wall);
        }
        if (frameA.landAnimator)
        {
            ghostAnimator.SetTrigger(Land);
        }
    }

    public string GetSavedDatas()
    {
        string savedDatas = "";

        for (int i = 0; i < playbackKeyFrames.Count; i++)
        {
            savedDatas += playbackKeyFrames[i].time.ToString() + "|" + SerializeVector3(playbackKeyFrames[i].pos) + "|" + playbackKeyFrames[i].flipXSprite + "|" +
                          playbackKeyFrames[i].isMovingAnimator + "|" + playbackKeyFrames[i].jumpAnimator + "|" + playbackKeyFrames[i].wallAnimator + "|" + playbackKeyFrames[i].landAnimator;
            if (i < playbackKeyFrames.Count - 1)
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

    public void SetGhostPlayback(bool follow, bool startRun, int frameOffset, string savedDatas)
    {
        ResetPlayback();

        playbackKeyFrames = GetPlaybackKeyFramesFromString(savedDatas);

        ghost = Instantiate(ghostPrefab, playbackKeyFrames[0].pos, Quaternion.identity);
        ghostAnimator = ghost.GetComponentInChildren<Animator>();
        ghostSpriteRenderer = ghost.GetComponentInChildren<SpriteRenderer>();
        playerTimer = playbackKeyFrames[0].time;
        if (follow)
        {
            virtualCamera.Follow = ghost.transform;
        }
        startRunAfterPlayback = startRun;
        this.frameOffset = frameOffset;
    }

    public void ResetPlayback()
    {
        playbackKeyFrames.Clear();
        playerTimer = 0;
        isRecording = false;
        isPlaybackDone = false;
    }


    private List<PlaybackKeyFrame> GetPlaybackKeyFramesFromString(string datas)
    {
        List<PlaybackKeyFrame> localKeyFrames = new List<PlaybackKeyFrame>();

        string[] splitStrings = datas.Split('/');
        for (int i = 0; i < splitStrings.Length; i++)
        {
            string splitString = splitStrings[i];

            if (splitString != "")
            {
                string timeString = splitString.Split('|')[0];
                string posString = splitString.Split('|')[1];
                string flipXSpriteString = splitString.Split('|')[2];
                string moveAnimatorString = splitString.Split('|')[3];
                string jumpAnimatorString = splitString.Split('|')[4];
                string wallAnimatorString = splitString.Split('|')[5];
                string landAnimatorString = splitString.Split('|')[6];

                float _time = float.Parse(timeString);
                Vector3 _pos = DeserializeVector3(posString);
                bool _flipXSprite = bool.Parse(flipXSpriteString);
                bool _isMovingAnimator = bool.Parse(moveAnimatorString);
                bool _jumpAnimator = bool.Parse(jumpAnimatorString);
                bool _wallAnimator = bool.Parse(wallAnimatorString);
                bool _landAnimator = bool.Parse(landAnimatorString);

                PlaybackKeyFrame frame = new PlaybackKeyFrame()
                {
                    time = _time,
                    pos = _pos,
                    flipXSprite = _flipXSprite,
                    isMovingAnimator = _isMovingAnimator,
                    jumpAnimator = _jumpAnimator,
                    wallAnimator = _wallAnimator,
                    landAnimator = _landAnimator
                };

                localKeyFrames.Add(frame);
            }
        }

        return localKeyFrames;
    }

    public float CompareRuns(string currentRun, string savedRun, float accuracyThreshold, int frameThreshold)
    {
        List<PlaybackKeyFrame> currentRunKeyFrames = new List<PlaybackKeyFrame>();
        List<PlaybackKeyFrame> savedRunKeyFrames = new List<PlaybackKeyFrame>();

        Debug.Log(currentRun);
        Debug.Log(savedRun);

        currentRunKeyFrames = GetPlaybackKeyFramesFromString(currentRun);
        savedRunKeyFrames = GetPlaybackKeyFramesFromString(savedRun);

        int minFrameCount = Mathf.Min(currentRunKeyFrames.Count, savedRunKeyFrames.Count);
        int maxFrameCount = Mathf.Max(currentRunKeyFrames.Count, savedRunKeyFrames.Count);

        float sameValues = 0;

        float distanceToGhostPos = 0;

        for (int i = 0; i < minFrameCount; i++)
        {
            Debug.Log("Comparing frame " + i);
            for (int j = 0; j < frameThreshold; j++)
            {
                if (i + j < currentRunKeyFrames.Count)
                {
                    distanceToGhostPos = Vector3.Distance(currentRunKeyFrames[i + j].pos, savedRunKeyFrames[i].pos);
                    if (distanceToGhostPos <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                    Debug.Log("Distance at frame " + (i + j) + " is " + distanceToGhostPos);
                    Debug.Log("Pos are " + currentRunKeyFrames[i + j].pos + " compared to " + savedRunKeyFrames[i].pos);
                }
                if (i - j >= 0)
                {
                    distanceToGhostPos = Vector3.Distance(currentRunKeyFrames[i - j].pos, savedRunKeyFrames[i].pos);
                    if (distanceToGhostPos <= accuracyThreshold)
                    {
                        sameValues++;
                        break;
                    }
                    Debug.Log("Distance at frame " + (i - j) + " is " + distanceToGhostPos);
                    Debug.Log("Pos are " + currentRunKeyFrames[i - j].pos + " compared to " + savedRunKeyFrames[i].pos);
                }
            }
        }

        Debug.Log(sameValues + "/" + savedRunKeyFrames.Count);

        return sameValues / savedRunKeyFrames.Count;
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

    public void NotifyTrigger(TriggerType type)
    {
        switch (type)
        {
            case TriggerType.Jump: pendingJump = true; break;
            case TriggerType.Wall: pendingWall = true; break;
            case TriggerType.Land: pendingLand = true; break;
        }
    }
}
