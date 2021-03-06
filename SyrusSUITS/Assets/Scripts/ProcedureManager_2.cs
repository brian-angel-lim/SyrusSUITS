﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcedureManager_2 : MonoBehaviour {

    public GameObject placingPanel;

    public delegate void StepEvent(Step step);
    public static event StepEvent OnStepChanged;

    private static ProcedureManager_2 _Instance;
    public static ProcedureManager_2 Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<ProcedureManager_2>();
            }
            return _Instance;
        }
    }

    public GameObject optionsPrefab;
	public GameObject textPrefab;
	public GameObject buttonPrefab;
	
	int currentProcedureNum = 0; // The current procedure number
	int stepNumber = 0;
	string path;    // Directory of where the procedure JSON files are located
    List<string> proceduresPath;                // List of procedure system path (e.x. something.json)
    List<string> procedureName;             // List of procedure names (e.x. title from JSON)
    Procedure procedure = new Procedure();  // Procedure Class holds the information of the steps
    bool isProcedure = false;           // If the procedure panel has been loaded.

    OptionsMenu stepMenu;               // Step menu

    //This is for the Procedure Panel V2
    public GameObject procedurePanel;       // Procedure Panel
    public Text previousStep; // Procedure Panel Text
    public Text currentStep; // Procedure Panel Text
    public Text nextStep; // Procedure Panel Text

    float timer;

    void Start () {

        path = Application.streamingAssetsPath + "/Procedures/";
		LoadFileNames("/Procedures/");
		//printProcedurePaths();
		//ChooseProcedure();

        LeapManager.OnGestureSwipe += SwipeGesture;
        timer = 0;
	}
	
    void SwipeGesture(Vector3 pos, Vector3 dir) {
        if (Vector3.Angle(Vector3.left, dir) < 40.0f) {
            if (timer <= 0) {
                timer = 1.0f;
                NextStep();
            }
        } else if (Vector3.Angle(Vector3.right, dir) < 40.0f) {
            if (timer <= 0) {
                timer = 1.0f;
                PreviousStep();
            }
        }

    }

	// Update is called once per frame
	void Update () {
		if (timer > 0) timer -= Time.deltaTime;
	}
    public void stepSpeak()
    {
        string t = procedure.steps[Instance.stepNumber].text;
        if (t != null) HoloToolkit.Unity.TextToSpeech.AetherSpeech(t);
    }
    void printProcedurePaths() {
		for(int i = 0; i < proceduresPath.Count; i++) {
			Debug.Log(proceduresPath[i]);
		}
	}
	public void ChooseProcedure() {
        OptionsMenu opts = OptionsMenu.Instance("Choose A Procedure", true);
        opts.OnSelection += LoadProcedure;
        if (procedureName.Count > 0) {
            //procedureName.Count
            
            for (int i = 0; i < procedureName.Count; i++) {
                opts.AddItem(procedureName[i], i);
            }
            opts.ResizeOptions();
        } else {
            Debug.Log("Error: No procedures currently loaded in Procedure Manager Script");
        }
    }
    //Toggles the procedure panel active state
    public void ToggleProcedurePanel(bool val)
    {
        procedurePanel.SetActive(val);
    }
    //Change the current task information
    void changeBarTxt()
    {
        //procedurBarText.alignment = TextAnchor.UpperLeft;
        //procedurBarText.text = procedure.steps[stepNumber].number + ".) " + procedure.steps[stepNumber].text + "\n";
        //if (procedure.steps[stepNumber].subtext.Length > 0)
        //{
        //    procedurBarText.text += CreateSubText(procedure.steps[stepNumber]) + "\n";
        //}

        //current
        currentStep.alignment = TextAnchor.UpperLeft;
        currentStep.text = procedure.steps[stepNumber].number + ".) " + procedure.steps[stepNumber].text + "\n";
        if (procedure.steps[stepNumber].subtext.Length > 0)
        {
            currentStep.text += CreateSubText(procedure.steps[stepNumber]) + "\n";
        }

        //next
        if (Instance.stepNumber + 1 <= Instance.procedure.steps.Count - 1)
        {
            nextStep.text = procedure.steps[stepNumber + 1].number + ".) " + procedure.steps[stepNumber + 1].text + "\n";
            nextStep.alignment = TextAnchor.UpperLeft;
            nextStep.text = procedure.steps[stepNumber + 1].number + ".) " + procedure.steps[stepNumber + 1].text + "\n";
            if (procedure.steps[stepNumber + 1].subtext.Length > 0)
            {
                nextStep.text += CreateSubText(procedure.steps[stepNumber + 1]) + "\n";
            }
        }
        else
        {
            nextStep.text = "";
        }
        //previous
        if (Instance.stepNumber - 1 >= 0)
        {
            //previousStep.text = procedure.steps[stepNumber - 1].number + ".) " + procedure.steps[stepNumber - 1].text + "\n";
            previousStep.alignment = TextAnchor.UpperLeft;
            previousStep.text = procedure.steps[stepNumber - 1].number + ".) " + procedure.steps[stepNumber - 1].text + "\n";
            if (procedure.steps[stepNumber - 1].subtext.Length > 0)
            {
                previousStep.text += CreateSubText(procedure.steps[stepNumber - 1]) + "\n";
            }
        }
        else
        {
            previousStep.text = "";
        }



    }
    // Goes forward to the next step
    public void NextStep()
    {
        if (isProcedure)
        if (isProcedure)
        {
            //Debug.Log("next Step");
            Instance.stepNumber++;
            if (Instance.stepNumber > Instance.procedure.steps.Count - 1)
            {
                //Do not let them go into the negatives
                Instance.stepNumber = Instance.procedure.steps.Count - 1;
            }
            else OnStepChanged(Instance.procedure.steps[Instance.stepNumber]);
            Instance.changeBarTxt();
        }
    }

    //Goes back to the previous step
    public void PreviousStep()
    {
        //Debug.Log("Previous Step");
        if (isProcedure)
        {
            Instance.stepNumber--;
            if (Instance.stepNumber < 0)
            {
                //Do not let them go into the negatives
                Instance.stepNumber = 0;
            }
            else OnStepChanged(Instance.procedure.steps[Instance.stepNumber]);
            Instance.changeBarTxt();
        }
    }
    //Returns a string with the color warning/caution and sub text of current step
    string CreateSubText(Step s)
    {
        string temp = "";
        string firstWord = s.subtext.Split(' ')[0];
        if (firstWord == "CAUTION:")
        {
            temp += "<color=red>CAUTION: </color>";
        }
        else if (firstWord == "WARNING:")
        {
            temp += "<color=red>WARNING: </color>";
        }
        int i = s.subtext.IndexOf(":");
        if (i + 2 < s.subtext.Length)
        {
            i += 2;
        }
        temp += s.subtext.Substring(i, s.subtext.Length - i);
        return temp;
    }
    // Manually override the step and jump to a new one
    public static void SetStep(int newStepNumber)
    {
        if (newStepNumber >= 0 && newStepNumber <= Instance.procedure.steps.Count - 1)
        {
            Instance.stepNumber = newStepNumber;
            OnStepChanged(Instance.procedure.steps[Instance.stepNumber]);
            Instance.changeBarTxt();
        }
    }
    //Loads in the procedure that should be completed

    public void LoadProcedure(string procName) {
        try
        {
            //Destroy(options);
            string temp = path + procName;
            if (System.IO.File.Exists(temp))
            {
                string contents = System.IO.File.ReadAllText(temp);
                //Debug.Log(contents);
                procedure = JsonUtility.FromJson<Procedure>(contents);
                OnStepChanged(procedure.steps[stepNumber]);
                //Debug.Log(procedure.steps[1].text);
                //LoadStepWindow(procedure.steps);
                ToggleProcedurePanel(true);
                Transform tr = OverlayManager.Instance.transform;
                Layout ly = OverlayManager.Instance.getLayout();
                procedurePanel.transform.rotation = Quaternion.LookRotation(tr.forward, Vector3.up) * Quaternion.Euler(ly.panel_rot.x, ly.panel_rot.y, ly.panel_rot.z);
                procedurePanel.transform.position = tr.position + tr.rotation * new Vector3(ly.panel_pos.x, ly.panel_pos.y, ly.panel_pos.z);
                changeBarTxt(); // Needs to be here to update to the first Item
                isProcedure = true;
                //placingPanel.loadPlacingProcedure();
            }
            //if the procedures are loaded. Then render the procedure bar
            else
            {
                Debug.Log("Error: Unable to read " + procName + " file, at " + temp);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error: " + procName + " JSON input. " + ex.Message);
        }
    }

    public void LoadProcedure(int procedureNumber) {
        currentProcedureNum = procedureNumber;
        try
        {
            //Destroy(options);
            string temp = path + proceduresPath[procedureNumber];
            if (System.IO.File.Exists(temp))
            {
                string contents = System.IO.File.ReadAllText(temp);
                //Debug.Log(contents);
                procedure = JsonUtility.FromJson<Procedure>(contents);
                OnStepChanged(procedure.steps[stepNumber]);
                //Debug.Log(procedure.steps[1].text);
                //LoadStepWindow(procedure.steps);
                ToggleProcedurePanel(true);
                changeBarTxt(); // Needs to be here to update to the first Item
                isProcedure = true;
                //placingPanel.loadPlacingProcedure();
            }
            //if the procedures are loaded. Then render the procedure bar
            else
            {
                Debug.Log("Error: Unable to read " + proceduresPath[procedureNumber] + " file, at " + temp);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error: " + proceduresPath[procedureNumber] + " JSON input. " + ex.Message);
        }
    }
    //Loads the all the JSON files from the Streaming Assets
    void LoadFileNames(string dir) {
        procedureName = new List<string>();
        proceduresPath = new List<string>();
        string location = Application.streamingAssetsPath;
        try
        {
            string temp = location + dir;
            foreach (string file in System.IO.Directory.GetFiles(temp))
            {
                string label = file.Replace(temp, ""); // file (e.x. = procedure1.JSON)
				if( label.EndsWith(".json"))
                {
                    string contents = System.IO.File.ReadAllText(temp + label);
                    ProcedureNames procedureNames = JsonUtility.FromJson<ProcedureNames>(contents);
                    // Debug.Log(procedureNames.title);
                    procedureName.Add(procedureNames.title);
                    proceduresPath.Add(label);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error: JSON input. " + ex.Message);
        }
    }
}


//PROCEDURES
[System.Serializable]
public class Procedure
{
    public string title; //Name of procedure
    public List<Step> steps = new List<Step>();
}

[System.Serializable]
public class Step
{
    public string text;     // Text to display
    public string subtext;  // Sub Text with CAUTION || DANGER
    public string number;   // Number of which step this is
    public List<Prompt> prompts = new List<Prompt>();
}

[System.Serializable]
public class Prompt {
    public string type;
    public string moduleID;
    public string misc;
    public float rotation;
}

// Query the Procedure names out from the JSON
[System.Serializable]
public class ProcedureNames
{
    public string title;
}

