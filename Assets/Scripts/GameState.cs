using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameState : MonoBehaviour
{
    public List<GameObject> levels;
    public GameObject FadeScreen;
    private bool fadeIn;
    private bool fadeOut;
    private Color fadeColor;
    private float fadeDuration = 0.7f;
    private float lerpFade = 0.0f;
    public int currentLevel = 0;
    public GameObject player;
    public StageState currentRoomStageState;

	void Start ()
    {
        levels.ForEach(l => l.SetActive(false));
        levels[currentLevel].SetActive(true);
        fadeColor = FadeScreen.GetComponent<SpriteRenderer>().color;
        currentRoomStageState = new RoomStageState(this);
    }
	
	void Update ()
    {    
        if(fadeIn)
        {
            lerpFade += Time.deltaTime / fadeDuration;
            fadeColor = FadeScreen.GetComponent<SpriteRenderer>().color;
            fadeColor.a = Mathf.Lerp(0, 1, lerpFade);
            FadeScreen.GetComponent<SpriteRenderer>().color = fadeColor;
            if (fadeColor.a >= 1.0f)
            {
                lerpFade = 0;
                fadeIn = false;
            }
        }

        if (fadeOut)
        {
            lerpFade += Time.deltaTime / fadeDuration;
            fadeColor = FadeScreen.GetComponent<SpriteRenderer>().color;
            fadeColor.a = Mathf.Lerp(1, 0, lerpFade);
            FadeScreen.GetComponent<SpriteRenderer>().color = fadeColor;
            if (fadeColor.a <= 0.0f)
            {
                lerpFade = 0;
                fadeOut = false;
            }
        }
    }

    public string InteractWithTag(string interactingTag)
    {
        var states = currentRoomStageState.States;
        if (interactingTag == null || !states.ContainsKey(interactingTag))
            return null;

        if(states[interactingTag].Action != null)
            currentRoomStageState.States[interactingTag].Action.Invoke();
        return currentRoomStageState.States[interactingTag].Description;
    }

    public void FadeIn()
    {
        fadeIn = true;
        fadeOut = false;
    }

    public void FadeOut()
    {
        fadeOut = true;
        fadeIn = false;
    }

    public void LoadLevel(int i)
    {
        levels[currentLevel].SetActive(false);
        levels[i].SetActive(true);
        currentLevel = i;
    }

    public void SetPlayerPosition(float x, float y)
    {
        player.transform.position = new Vector3(x, y, 0);
    }

    public void PlayerDamaged()
    {
        var youngDodgeRoomStageState = currentRoomStageState as YoungDodgeRoomStageState;
        if(youngDodgeRoomStageState != null)
        {
            youngDodgeRoomStageState.LivesLeft -= 1;
            youngDodgeRoomStageState.SetHearts();
            if(youngDodgeRoomStageState.LivesLeft == 0)
            {
                currentRoomStageState = new YoungRoomStageState(this);

            }
        }

        var babyDodgeRoomStageState = currentRoomStageState as BabyDodgeRoomStageState;
        if (babyDodgeRoomStageState != null)
        {
            currentRoomStageState = new BabyDodgeRoomStageState(this);
        }

    }

    public void PlayerSurvived()
    {
        if (currentRoomStageState is YoungDodgeRoomStageState)
        {
            currentRoomStageState = new YoungRestRoomStageState(this);
        }
        if (currentRoomStageState is BabyDodgeRoomStageState)
        {
            var babyDodgeRoomStageState = currentRoomStageState as BabyDodgeRoomStageState;
            babyDodgeRoomStageState.StageComplete = true;
        }
        //TODO : Add babydodge
        //else if (currentRoomStageState is Baby)
    }
}

public class ObjectState
{
    public string Name;

    public string Description;

    public Action Action;

    public ObjectState (string name, string description, Action action = null)
    {
        Name = name;
        Description = description;
        Action = action;
    }
}

public class RoomStageState : StageState
{
    private bool _bedChecked;
    public bool BedChecked
    {
        get { return _bedChecked; }
        set { _bedChecked = value; ValidateState(); }
    }

    private bool _doorChecked;
    public bool DoorChecked
    {
        get { return _doorChecked; }
        set { _doorChecked = value; ValidateState(); }
    }

    private bool _deskChecked;
    public bool DeskChecked
    {
        get { return _deskChecked; }
        set { _deskChecked = value; ValidateState(); }
    }

    public RoomStageState(GameState gameState)
    {
        relatedLevel = 0;
        this.gameState = gameState;
        States = new Dictionary<string, ObjectState>();

        States.Add("Bed", new ObjectState("Bed", "My cold bed", () => { BedChecked = true; }));
        States.Add("Door", new ObjectState("Door", "I need my phone!", () => { DoorChecked = true; }));
        States.Add("Window", new ObjectState("Window", "I'm late for work."));
        States.Add("Desk", new ObjectState("Desk", "\"Where is my phone?\"", () => { DeskChecked = true; }));
        States.Add("Poster", new ObjectState("Poster", "\"All work and no play...\""));
    }
    
    public override void ValidateState()
    {
        if(BedChecked && DoorChecked && DeskChecked)
        {
            StageComplete = true;
        }
    }

    public override IEnumerator OnStageComplete ()
    {
        yield return new WaitForSeconds(1);
        gameState.currentRoomStageState = new PosterRoomStageState(gameState);
        yield return null;
    }
}

public class PosterRoomStageState : RoomStageState
{
    private bool _posterChecked;
    public bool PosterChecked
    {
        get { return _posterChecked; }
        set { _posterChecked = value; ValidateState(); }
    }

    public PosterRoomStageState(GameState gameState) : base(gameState)
    {
        States["Poster"].Description = "What's this?";
        States["Poster"].Action = () => { PosterChecked = true; };

        gameState.levels[relatedLevel].GetComponent<RoomScript>().ChangeBackground(1);
    }

    public override void ValidateState()
    {
        if (PosterChecked)
        {
            StageComplete = true;
        }
    }

    public override IEnumerator OnStageComplete()
    {
        gameState.FadeIn();
        yield return new WaitForSeconds(2);
        gameState.currentRoomStageState = new YoungRoomStageState(gameState);
        gameState.SetPlayerPosition(62, -100);
        gameState.LoadLevel(1);
        gameState.FadeOut();
        yield return null;
    }
}

public class YoungRoomStageState : StageState
{
    private bool _consoleChecked;
    public bool ConsoleChecked
    {
        get { return _consoleChecked; }
        set { _consoleChecked = value; ValidateState(); }
    }

    public YoungRoomStageState(GameState gameState)
    {
        relatedLevel = 1;
        this.gameState = gameState;
        States = new Dictionary<string, ObjectState>();

        States.Add("Bed", new ObjectState("Bed", "My old bed?"));
        States.Add("Door", new ObjectState("Door", "I can hear my parents fighting..."));
        States.Add("Window", new ObjectState("Window", "It's nighttime"));
        States.Add("Console", new ObjectState("Console", "I miss this :)", () => { ConsoleChecked = true; }));
        States.Add("OldPoster", new ObjectState("OldPoster", "Best game ever!"));

        gameState.levels[relatedLevel].transform.FindChild("Hearths").gameObject.SetActive(false);
        gameState.levels[relatedLevel].transform.FindChild("Spawners").gameObject.SetActive(false);

    }

    public override void ValidateState()
    {
        if (ConsoleChecked)
        {
            StageComplete = true;
        }
    }

    public override IEnumerator OnStageComplete()
    {
        gameState.currentRoomStageState = new YoungDodgeRoomStageState(gameState);
        yield return null;
    }
}

public class YoungDodgeRoomStageState : YoungRoomStageState
{
    public int LivesLeft = 3;

    public YoungDodgeRoomStageState(GameState gameState) : base(gameState)
    {
        States = new Dictionary<string, ObjectState>();

        States.Add("Console", new ObjectState("Console", "I miss this :)"));

        gameState.levels[relatedLevel].transform.FindChild("Hearths").gameObject.SetActive(true);
        gameState.levels[relatedLevel].transform.FindChild("Spawners").gameObject.SetActive(true);
        SetHearts();
    }

    public void SetHearts()
    {
        gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(0).gameObject.SetActive(false);
        gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(1).gameObject.SetActive(false);
        gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(2).gameObject.SetActive(false);

        if (LivesLeft > 2)
            gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(2).gameObject.SetActive(true);
        if (LivesLeft > 1)
            gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(1).gameObject.SetActive(true);
        if (LivesLeft > 0)
            gameState.levels[relatedLevel].transform.FindChild("Hearths").GetChild(0).gameObject.SetActive(true);
    }
}

public class YoungRestRoomStageState : YoungRoomStageState
{
    private bool _bedChecked;
    public bool BedChecked
    {
        get { return _bedChecked; }
        set { _bedChecked = value; ValidateState(); }
    }

    public YoungRestRoomStageState(GameState gameState) : base(gameState)
    {
        States = new Dictionary<string, ObjectState>();

        States.Add("Console", new ObjectState("Console", "I need to sleep."));
        States.Add("Bed", new ObjectState("Bed", null, () => { BedChecked = true;}));
    }

    public override void ValidateState()
    {
        if (BedChecked)
        {
            StageComplete = true;
        }
    }

    public override IEnumerator OnStageComplete()
    {
        gameState.FadeIn();
        yield return new WaitForSeconds(2);
        gameState.LoadLevel(2);
        gameState.currentRoomStageState = new BabyRoomStageState(gameState);
        gameState.FadeOut();
        yield return null;
    }
}

public class BabyRoomStageState : StageState
{
    private bool _toysChecked;
    public bool ToysChecked
    {
        get { return _toysChecked; }
        set { _toysChecked = value; ValidateState(); }
    }

    private bool _doorChecked;
    public bool DoorChecked
    {
        get { return _doorChecked; }
        set { _doorChecked = value; ValidateState(); }
    }

    private bool _windowChecked;
    public bool WindowChecked
    {
        get { return _windowChecked; }
        set { _windowChecked = value; ValidateState(); }
    }

    private bool _bedChecked;
    public bool BedChecked
    {
        get { return _bedChecked; }
        set { _bedChecked = value; ValidateState(); }
    }

    public BabyRoomStageState(GameState gameState)
    {
        relatedLevel = 2;
        this.gameState = gameState;
        States = new Dictionary<string, ObjectState>();

        States.Add("Bed", new ObjectState("Bed", "Why is there blood here?", () => { BedChecked = true; }));
        States.Add("Door", new ObjectState("Door", "A lot of blood behide the door.", () => { DoorChecked = true; }));
        States.Add("Window", new ObjectState("Window", "I just want to leave!", () => { WindowChecked = true; }));
        States.Add("Toys", new ObjectState("Toys", "Blood on toys... Oh no...", () => { ToysChecked = true; }));

        gameState.levels[relatedLevel].transform.FindChild("Spawners").gameObject.SetActive(false);
    }

    public override void ValidateState()
    {
        if (BedChecked && DoorChecked && WindowChecked && ToysChecked)
        {
            StageComplete = true;
        }
    }

    public override IEnumerator OnStageComplete()
    {
        gameState.currentRoomStageState = new BabyDodgeRoomStageState(gameState);
        yield return null;
    }
}

public class BabyDodgeRoomStageState : BabyRoomStageState
{
    public BabyDodgeRoomStageState(GameState gameState) : base(gameState)
    {
        HideSpawners();
        ShowSpawners();
    }

    public void HideSpawners()
    {
        gameState.levels[relatedLevel].transform.FindChild("Spawners").gameObject.SetActive(false);
    }

    public void ShowSpawners()
    {
        gameState.FadeIn();
        gameState.Invoke("FadeOut", 2);
        gameState.levels[relatedLevel].transform.FindChild("Spawners").gameObject.SetActive(true);
    }

    public override IEnumerator OnStageComplete()
    {
        gameState.FadeIn();
        yield return new WaitForSeconds(2);
        gameState.LoadLevel(3);
        gameState.currentRoomStageState = new MentalStageState(gameState);
        gameState.FadeOut();
        yield return null;
    }
}

public class MentalStageState : StageState
{
    public MentalStageState(GameState gameState)
    {
        relatedLevel = 3;
        this.gameState = gameState;
        States = new Dictionary<string, ObjectState>();

        States.Add("Mind", new ObjectState("Mind", "I... lost it..."));
        States.Add("Door", new ObjectState("Door", "I LOST IT!"));
    }

    public override void ValidateState()
    {

    }

    public override IEnumerator OnStageComplete()
    {
        yield return null;
    }
}


public abstract class StageState
{
    protected int relatedLevel = -1;

    protected GameState gameState;

    public Dictionary<string, ObjectState> States { get; set; }

    private bool _stageComplete;
    public bool StageComplete
    {
        get { return _stageComplete; }
        set { _stageComplete = value; if (_stageComplete) gameState.StartCoroutine(OnStageComplete()); }
    }

    public abstract void ValidateState();

    public abstract IEnumerator OnStageComplete();
}