using UnityEngine;
using UnityEngine.UI;
using RuntimeCurveEditor;

public enum MenuItems {New, Save, SaveAs, Load, Delete, Exit};

/// <summary>
/// Demmo script of using RTAnimationCurve for editing an animation curve at run time.
/// In this example: 2 curves and one path of another 2 curves can be edited in the same editor.
/// </summary>
public class Demo : MonoBehaviour, IFileOperations {

	public RTAnimationCurve rtAnimationCurve;//class through which we'll access the Runtime Curve Editor core

    public DemoAnimationCurves demoAnimationCurves;

    public Button curveEditorButton;
    public Button firstCurveButton;
    public Button secondCurveButton;
    public Button curvePairButton;
    public Button fileButton;
    public RectTransform menuList;
    public RectTransform fileSelectionControl;

    public Text filenameText;

    public Texture closeTexture;   
	
	const string addStr = "Add";
	const string removeStr = "Remove";
	bool curve1Added;//true when the first curve is added(shown) to the curve editor 
	bool curve2Added;//similar to above, but for curve2
	bool pairAdded;//similar to above, this for the pair of two curves
	
	const string DEFAULT_NAME = "Unnamed";
	
	bool showFileMenu;//true when the file's vertical menu is visible
	    			
	Vector2 scrollPosition;//keeps the scrolling position in the scrolling view
			
    Color fileButtonColor;
    Color fileButtonColorSelected;
	
	void Start () {
        //PlayerPrefs.DeleteAll();
        //we can set the curves in the inspector(as they are public fields)
        //if null, they will be automatically instantiated and a 0 key will be added at 0 position.
        demoAnimationCurves.FillInitCurves();          
        filenameText.text = DEFAULT_NAME;
        string lastFile = rtAnimationCurve.GetLastFile();
		if(lastFile != null) {
            LoadFile(lastFile);
		} 		
        ShowUI();
        UpdateButtonTexts();
        fileButtonColor = fileButton.GetComponent<Image>().color;
        fileButtonColorSelected = fileButtonColor - new Color32(50, 50, 50, 0);

        rtAnimationCurve.ListenOnDataAlter(OnDataAlter);
    }

    void Update() {
        if (!curveEditorButton.gameObject.activeSelf && rtAnimationCurve.IsCurveEditorClosed()) {
            curveEditorButton.gameObject.SetActive(true);
        }
    }
           
    public void ShowCurveEditor() {
        if (rtAnimationCurve.IsCurveEditorClosed()) {
            rtAnimationCurve.ShowCurveEditor();
            curveEditorButton.gameObject.SetActive(false);
        }
    }

    public void FirstCurve() {
        if (!curve1Added) {
            if (rtAnimationCurve.Add(ref demoAnimationCurves.animCurve)) {
                //we may select a different gradations ranges for each curve
                rtAnimationCurve.SetGradYRange(0f, 2.6f);
                curve1Added = true;
            }
        } else {
            rtAnimationCurve.Remove(demoAnimationCurves.animCurve);
            curve1Added = false;
        }
        UpdateFirstCurveText();
    }

    void UpdateFirstCurveText() {
        firstCurveButton.GetComponentInChildren<Text>().text = (curve1Added ? removeStr : addStr) + " 1st Curve";
    }

    public void SecondCurve() {
        if (!curve2Added) {
            if (rtAnimationCurve.Add(ref demoAnimationCurves.animCurve2)) {
                curve2Added = true;
            }
        } else {
            rtAnimationCurve.Remove(demoAnimationCurves.animCurve2);
            curve2Added = false;
        }
        UpdateSecondCurveText();
    }

    void UpdateSecondCurveText() {
        secondCurveButton.GetComponentInChildren<Text>().text = (curve2Added ? removeStr : addStr) + " 2nd Curve";
    }

    public void CurvePair() {
        if (!pairAdded) {
            if (rtAnimationCurve.Add(ref demoAnimationCurves.pairAnimCurve1, ref demoAnimationCurves.pairAnimCurve2)) {
                rtAnimationCurve.SetGradYRange(-11.2f, 11.2f);
                pairAdded = true;
            }
        } else {
            rtAnimationCurve.Remove(demoAnimationCurves.pairAnimCurve1);
            pairAdded = false;
        }
        UpdateCurvePairText();
    }

    void UpdateCurvePairText() {
        curvePairButton.GetComponentInChildren<Text>().text = (pairAdded ? removeStr : addStr) + " Curve Pair";
    }

    public void OnFileButton() {
        showFileMenu = !showFileMenu;
        menuList.gameObject.SetActive(showFileMenu);
        fileButton.GetComponent<Image>().color = showFileMenu ? fileButtonColorSelected : fileButtonColor;
    }

    public void OnFileNewButton() {
        NewWindow();
    }

    public void OnFileSaveButton() {
        if (DEFAULT_NAME != filenameText.text) {
            if (rtAnimationCurve.DataAltered()) {
                SaveData(false);
            }
        } else {
            ShowFileSelection(MenuItems.Save);
        }
    }

    public void OnFileSaveAsButton() {
        ShowFileSelection(MenuItems.Save);
    }

    public void OnFileLoadButton() {
        ShowFileSelection(MenuItems.Load);
    }

    public void OnFileDeleteButton() {
        ShowFileSelection(MenuItems.Delete);
    }

    public void OnFileExitButton() {
        Application.Quit();
    }

    public void DeleteFile(string fileName) {
        string temp = filenameText.text.Replace("*", "");
        if (temp == fileName) {//this is the case, we're deleting the current file
            NewWindow();
        }
        rtAnimationCurve.DeleteFile(fileName);
    }

    public void SaveData(bool saveAs) {
        if (!saveAs && filenameText.text.Contains("*")) {
            filenameText.text = filenameText.text.Substring(0, filenameText.text.Length - 1);
        } else {
            filenameText.text = fileSelectionControl.GetComponent<FileSelectionControlBehaviour>().GetInputFileName();
        }
        rtAnimationCurve.SaveData(filenameText.text, demoAnimationCurves);
    }

    public void LoadFile(string fileName) {
        rtAnimationCurve.LoadData(fileName, demoAnimationCurves);

        curve1Added = rtAnimationCurve.CurveVisible(demoAnimationCurves.animCurve);
        curve2Added = rtAnimationCurve.CurveVisible(demoAnimationCurves.animCurve2);
        pairAdded = rtAnimationCurve.CurvesVisible(demoAnimationCurves.pairAnimCurve1, demoAnimationCurves.pairAnimCurve2);
        UpdateButtonTexts();

        filenameText.text = fileName;
        curveEditorButton.gameObject.SetActive(rtAnimationCurve.IsCurveEditorClosed());
    }

    void ShowUI() {
        curveEditorButton.gameObject.SetActive(rtAnimationCurve.IsCurveEditorClosed());
        firstCurveButton.gameObject.SetActive(true);
        secondCurveButton.gameObject.SetActive(true);
        curvePairButton.gameObject.SetActive(true);
        fileButton.gameObject.SetActive(true);
        filenameText.gameObject.SetActive(true);
    }

    void UpdateButtonTexts() {
        UpdateFirstCurveText();
        UpdateSecondCurveText();
        UpdateCurvePairText();
    }

    void ShowFileSelection(MenuItems menuItem) {
        fileSelectionControl.gameObject.SetActive(true);
        FileSelectionControlBehaviour fileSelectionControlBehaviour = fileSelectionControl.GetComponent<FileSelectionControlBehaviour>();
        fileSelectionControlBehaviour.MenuItem = menuItem;
        fileSelectionControlBehaviour.NamesList = rtAnimationCurve.GetNamesList();
        fileSelectionControlBehaviour.FileOperations = this;
        fileSelectionControlBehaviour.Init();
    }
       	
	void NewWindow() {
		curve1Added = curve2Added = pairAdded = false;
        UpdateButtonTexts();
        rtAnimationCurve.NewWindow();
        filenameText.text = DEFAULT_NAME;
		demoAnimationCurves.ReInitCurves();    
    }

    void OnDataAlter() {
        if ((filenameText.text != DEFAULT_NAME) && (filenameText.text[filenameText.text.Length - 1] != '*') && rtAnimationCurve.DataAltered()) {
            filenameText.text += "*";
        }
    }
				
}
