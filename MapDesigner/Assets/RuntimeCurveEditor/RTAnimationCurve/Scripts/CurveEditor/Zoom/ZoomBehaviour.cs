using UnityEngine;

namespace RuntimeCurveEditor
{
    public class ZoomBehaviour : MonoBehaviour
    {

        public CurveWindow curveWindow;

        const float MAX_LEVEL = 3;

        const float MIN_LEVEL = 1;

        const float INV_MAX_LEVEL = 1 / MAX_LEVEL;

        const float INV_MIN_LEVEL = 1 / MIN_LEVEL;

        const float SCROLL_SCALE = 0.01f;

        float level = MIN_LEVEL;

        bool middleDown;

        bool altRightMouseDown;

        bool altLeftMouseDown;

        Vector2 prevPos;

        float prevTouchsDist;

        void Start() {
            //curveWindow.Zoom(zoomedRect);
        }

        void Update() {

            //check for zoom
            Vector2 pointerPos = Input.mousePosition;

            //1st check zoom by scrolling mouse wheel
            float zoomDelta = Input.mouseScrollDelta.y;
            if (zoomDelta == 0) {
                //2nd check zoom by 'alt' + right mouse button
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                    if (Input.GetMouseButtonDown(1)) {
                        prevPos = pointerPos;
                        altRightMouseDown = true;
                    } else if (altRightMouseDown) {
                        if (Input.GetMouseButton(1)) {
                            Vector2 diff = pointerPos - prevPos;
                            prevPos = pointerPos;
                            if ((diff.x >= 0) && (diff.y <= 0)) {
                                zoomDelta = Mathf.Max(diff.x, -diff.y);
                            } else if ((diff.x <= 0) && (diff.y >= 0)) {
                                zoomDelta = Mathf.Min(diff.x, -diff.y);
                            } else {
                                zoomDelta = diff.x - diff.y;
                            }
                        }
                    }
                }
                if (zoomDelta == 0) {
                    //3rd check zoom by touch pinch
                    if (Input.touchCount == 2) {
                        if ((Input.touches[0].phase == TouchPhase.Moved) && (Input.touches[1].phase == TouchPhase.Moved)) {
                            if (IsPinch(Input.touches[1].deltaPosition, Input.touches[0].deltaPosition)) {//there's a pinch
                                float dist = (Input.touches[1].position - Input.touches[0].position).magnitude;
                                if (prevTouchsDist == 0) {//for touchscreen(e.g mobiles) pinch(zoom) and pan, both are done with 2 fingers, so need to know if the fingers are too close to each other to make the distinction
                                    prevTouchsDist = dist;
                                } else {
                                    zoomDelta = dist / prevTouchsDist - 1;
                                    pointerPos = (Input.touches[1].position + Input.touches[0].position) * 0.5f;
                                    prevTouchsDist = dist;
                                }
                            } else {
                                prevTouchsDist = 0;
                            }
                        } else if ((Input.touches[0].phase == TouchPhase.Ended) || (Input.touches[1].phase == TouchPhase.Ended)) {
                            prevTouchsDist = 0;
                        }
                    }
                } else {
                    zoomDelta *= SCROLL_SCALE;
                }
            } else {
                zoomDelta *= SCROLL_SCALE;
            }

            if (altRightMouseDown && (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))) {
                altRightMouseDown = false;
            }

            if (zoomDelta != 0) {
                if (curveWindow.GetGridRect().Contains(pointerPos)) {
                    Zoom(1 - zoomDelta, pointerPos);
                }
            } else {
                //check for pan
                Vector2 panDiff = Vector2.zero;
                //1st check if middle mouse button is pressed
                if (Input.GetMouseButtonDown(2)) {
                    middleDown = true;
                    prevPos = pointerPos;
                } else if (middleDown) {
                    if (Input.GetMouseButtonUp(2)) {
                        middleDown = false;
                    } else if (Input.GetMouseButton(2)) {
                        panDiff = pointerPos - prevPos;
                        prevPos = pointerPos;
                    }
                } else {
                    //2nd check pan by 'alt' + left mouse button
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                        if (Input.GetMouseButtonDown(0)) {
                            prevPos = pointerPos;
                            altLeftMouseDown = true;
                        } else if (altLeftMouseDown) {
                            if (Input.GetMouseButton(0)) {
                                panDiff = pointerPos - prevPos;
                                prevPos = pointerPos;
                            }
                        }
                    }

                    if (panDiff == Vector2.zero) {
                        //3rd check pan by two fingers movement
                        if (Input.touchCount == 2) {
                            if ((Input.touches[0].phase == TouchPhase.Moved) && (Input.touches[1].phase == TouchPhase.Moved)) {
                                pointerPos = (Input.touches[1].position + Input.touches[0].position) * 0.5f;
                                panDiff = (Input.touches[1].deltaPosition + Input.touches[0].deltaPosition) * 0.5f;
                            }
                        }
                    }
                }

                if (panDiff != Vector2.zero) {
                    if (curveWindow.GetGridRect().Contains(pointerPos)) {
                        Pan(panDiff);
                    }
                }
            }

            if (altLeftMouseDown && (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))) {
                altLeftMouseDown = false;
            }
        }

        public void Reset() {
            level = MIN_LEVEL;
            middleDown = false;
            altRightMouseDown = false;
            altLeftMouseDown = false;
            prevPos = Vector2.zero;
            prevTouchsDist = 0;
        }

        void Zoom(float factor, Vector2 mousePos) {//factor should be a little smaller(for zoom in) or bigger(for zoom out) than 1
            level /= factor;
            if (level < MIN_LEVEL) {
                factor *= level * INV_MIN_LEVEL;
                level = MIN_LEVEL;
            } else if (level > MAX_LEVEL) {
                factor *= level * INV_MAX_LEVEL;
                level = MAX_LEVEL;
            }
            curveWindow.Zoom(factor, mousePos);
        }

        void Pan(Vector2 diff) {
            curveWindow.Pan(diff);
        }

        bool IsPinch(Vector2 v1, Vector2 v2) {//do these 2 vectors opposite directions?
            return ((v1.x >= 0) && (v2.x <= 0) && (v1.y >= 0) && (v2.y <= 0)) ||
                    ((v2.x >= 0) && (v1.x <= 0) && (v1.y >= 0) && (v2.y <= 0)) ||
                    ((v1.x >= 0) && (v2.x <= 0) && (v2.y >= 0) && (v1.y <= 0)) ||
                    ((v2.x >= 0) && (v1.x <= 0) && (v2.y >= 0) && (v1.y <= 0));
        }

    }
}