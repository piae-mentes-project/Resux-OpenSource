using UnityEngine;

public class DemoAnimationCurves : MonoBehaviour {

    //below we have the curves(or pairs of curves) we want to edit at runtime (keep in mind that the public fields of type AnimationCurve are saved by the PersistenceManager)
    public AnimationCurve animCurve;//animation curve that we can edit at run time
    public AnimationCurve animCurve2;//animation curve that we can to edit at run time

    //pair of animation curves,that can be edited simultanuesly ,if we want to see the path between them
    public AnimationCurve pairAnimCurve1;
    public AnimationCurve pairAnimCurve2;

    //initial curve values(when File->New pressed, the curve will get back to these initial values)
    AnimationCurve animCurveInitial;
    AnimationCurve animCurve2Initial;
    AnimationCurve pairAnimCurve1Initial;
    AnimationCurve pairAnimCurve2Initial;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FillInitCurves() {
        CopyCurve1ToCurve2(animCurve, ref animCurveInitial);
        CopyCurve1ToCurve2(animCurve2, ref animCurve2Initial);
        CopyCurve1ToCurve2(pairAnimCurve1, ref pairAnimCurve1Initial);
        CopyCurve1ToCurve2(pairAnimCurve2, ref pairAnimCurve2Initial);
    }

    public void ReInitCurves() {
        CopyCurve1ToCurve2(animCurveInitial, ref animCurve);
        CopyCurve1ToCurve2(animCurve2Initial, ref animCurve2);
        CopyCurve1ToCurve2(pairAnimCurve1Initial, ref pairAnimCurve1);
        CopyCurve1ToCurve2(pairAnimCurve2Initial, ref pairAnimCurve2);
    }

    void CopyCurve1ToCurve2(AnimationCurve curve1, ref AnimationCurve curve2) {
        if (curve1 != null) {
            curve2 = new AnimationCurve();
            foreach (Keyframe keyframe in curve1.keys) {
                curve2.AddKey(keyframe);
            }
        } else {
            curve2 = null;
        }
    }
}
