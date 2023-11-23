using UnityEngine;
using System.Collections.Generic;

namespace RuntimeCurveEditor
{
    public enum TangentPart { Left, Right, Both };

    public class ContextMenuUI : MonoBehaviour
    {

        public RectTransform panel;

        public RectTransform overlay;

        public RectTransform deleteKey;
        public RectTransform editKey;

        public RectTransform clampedAuto;
        public RectTransform auto;
        public RectTransform freeSmooth;
        public RectTransform flat;
        public RectTransform broken;

        public RectTransform leftTangent;
        public RectTransform rightTangent;
        public RectTransform bothTangents;

        public RectTransform panelTangent;

        public RectTransform freeTangent;
        public RectTransform linearTangent;
        public RectTransform constantTangent;

        public RectTransform tangentOverlay;

        public RectTransform checkMark;

        public RectTransform panelAdd;

        public RectTransform addKey;

        public RectTransform addKeyOverlay;

        RectTransform canvas;

        RectTransform checkMark2;

        List<RectTransform> menuItems;
        List<RectTransform> tangentsMenuItems;
        List<RectTransform> tangentTypesMenuItems;

        RectTransform tangentMenuItemHovered;

        ContextMenu contextMenu;

        InterfaceContextMenuListener interfaceContextMenuListener;

        Vector2 ratioScreenCanvas;
        Vector2 invRatioScreenCanvas;

        // Use this for initialization
        void Start() {
            tangentsMenuItems = new List<RectTransform>() { leftTangent, rightTangent, bothTangents };
            menuItems = new List<RectTransform>() { deleteKey, editKey, clampedAuto, auto, freeSmooth, flat, broken };
            tangentTypesMenuItems = new List<RectTransform>() { freeTangent, linearTangent, constantTangent };
            menuItems.AddRange(tangentsMenuItems);
            panelTangent.gameObject.SetActive(false);
            canvas = panel.parent.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                CheckSelectedItem();
            } else {
                CheckHovering();
            }

            //for debug
            //if (Input.GetKeyDown(KeyCode.D)) {
            //    //Vector2 menuItemPos = new Vector2(panel.position.x, menuItem.position.y);
            //    Vector2 mousePos = Input.mousePosition;
            //    RectTransform menuItem = editKey;
            //    Debug.LogError("menuItem:" + panel.position.x + "  " + menuItem.position.y);
            //    Debug.LogError("mousePos:" + mousePos.x + "  " + mousePos.y);
            //    Debug.LogError("menuItem rect:" + menuItem.rect.xMin + " " + menuItem.rect.yMin + " " + menuItem.rect.xMax + " " + menuItem.rect.yMax);
            //    if (menuItem.rect.Contains(mousePos - new Vector2(panel.position.x, menuItem.position.y))) {
            //        Debug.LogError("Ovaj je!");
            //    }
            //}
        }

        public void EnablePanel() {
            if (panelAdd.gameObject.activeSelf) {
                HidePanelAdd();
            }
            panel.gameObject.SetActive(true);
            gameObject.SetActive(true);
            InitRatioScreenCanvas();
        }

        public void EnableAddPanel(bool enable) {
            if (panel.gameObject.activeSelf) {
                HidePanel();
            }
            panelAdd.gameObject.SetActive(enable);
            gameObject.SetActive(enable);
            if (enable) {
                InitRatioScreenCanvas();
            }
        }

        void InitRatioScreenCanvas() {
            ratioScreenCanvas.x = Screen.width / canvas.rect.width;
            ratioScreenCanvas.y = Screen.height / canvas.rect.height;
            invRatioScreenCanvas.x = 1f / ratioScreenCanvas.x;
            invRatioScreenCanvas.y = 1f / ratioScreenCanvas.y;
        }

        public void SetPos(Vector2 pos) {
            if (panel.gameObject.activeSelf) {
                float ySing = (pos.y > panel.sizeDelta.y) ? -1 : 1;
                panel.position = pos + new Vector2(panel.sizeDelta.x, ySing * panel.sizeDelta.y) * 0.5f;
            } else if (panelAdd.gameObject.activeSelf) {
                panelAdd.position = pos + new Vector2(panelAdd.sizeDelta.x, -panelAdd.sizeDelta.y) * 0.5f;
            }
        }

        public void SetListener(InterfaceContextMenuListener interfaceContextMenuListener) {
            this.interfaceContextMenuListener = interfaceContextMenuListener;
        }

        public void SetSelectedOption(ContextMenu contextMenu) {
            RectTransform rectTransform = null;
            if (contextMenu.clampedAuto) {
                rectTransform = clampedAuto;
            } else if (contextMenu.auto) {
                rectTransform = auto;
            } else if (contextMenu.freeSmooth) {
                rectTransform = freeSmooth;
            } else if (contextMenu.broken) {
                rectTransform = broken;
            }
            if (rectTransform != null) {
                checkMark.localPosition = new Vector2(-panel.sizeDelta.x * 0.5f + checkMark.sizeDelta.x, rectTransform.localPosition.y);
                if (contextMenu.freeSmooth && contextMenu.flat) {
                    checkMark2 = Instantiate(checkMark.gameObject).GetComponent<RectTransform>();
                    checkMark2.SetParent(checkMark.parent);
                    checkMark2.localPosition = new Vector2(checkMark.localPosition.x, flat.localPosition.y);
                }
            } else {
                Debug.LogError("None of the menu options is selected!");
            }
            this.contextMenu = contextMenu;
        }

        public bool Hover(Vector2 mousePos) {
            bool hover = false;
            if (panel.gameObject.activeSelf) {
                if ((tangentMenuItemHovered != null) && panelTangent.rect.Contains(mousePos - (Vector2)panelTangent.position)) {
                    hover = true;
                } else if (panel.rect.Contains(mousePos - (Vector2)panel.position)) {
                    hover = true;
                }
            } else if (panelAdd.gameObject.activeSelf) {
                hover = MouseOverPanelAdd();
            }
            return hover;
        }

        void CheckHovering() {
            Vector2 mousePos = Input.mousePosition;

            if (panel.gameObject.activeSelf) {
                if ((tangentMenuItemHovered != null) && panelTangent.rect.Contains(new Vector2((mousePos.x - panelTangent.position.x) * invRatioScreenCanvas.x, (mousePos.y - panelTangent.position.y) * invRatioScreenCanvas.y))) {
                    bool hover = false;
                    foreach (RectTransform menuItem in tangentTypesMenuItems) {
                        Vector2 menuItemPos = new Vector2(panelTangent.position.x, menuItem.position.y);

                        if (menuItem.rect.Contains(new Vector2((mousePos.x - menuItemPos.x) * invRatioScreenCanvas.x, (mousePos.y - menuItemPos.y) * invRatioScreenCanvas.y))) {
                            tangentOverlay.position = new Vector2(panelTangent.position.x, menuItem.position.y);
                            hover = true;
                        }
                    }
                    if (hover != tangentOverlay.gameObject.activeSelf) {
                        tangentOverlay.gameObject.SetActive(hover);
                    }
                } else {
                    bool hover = false;
                    foreach (RectTransform menuItem in menuItems) {
                        Vector2 menuItemPos = new Vector2(panel.position.x, menuItem.position.y);

                        if (menuItem.rect.Contains(new Vector2((mousePos - menuItemPos).x * invRatioScreenCanvas.x, (mousePos - menuItemPos).y * invRatioScreenCanvas.y))) {
                            overlay.position = new Vector2(panel.position.x, menuItem.position.y);
                            hover = true;
                            if (tangentsMenuItems.Contains(menuItem)) {
                                tangentMenuItemHovered = menuItem;
                            } else {
                                tangentMenuItemHovered = null;
                            }
                            break;
                        }
                    }
                    if (hover != overlay.gameObject.activeSelf) {
                        if (hover) {
                            overlay.gameObject.SetActive(hover);
                        } else if (panelTangent == null) {
                            overlay.gameObject.SetActive(hover);
                        }
                    }

                    if (tangentMenuItemHovered != null) {
                        panelTangent.gameObject.SetActive(true);
                        panelTangent.position = new Vector2(panel.position.x + (panel.sizeDelta.x + panelTangent.sizeDelta.x) * 0.5f * ratioScreenCanvas.x,
                                                            tangentMenuItemHovered.position.y + (tangentMenuItemHovered.sizeDelta.y - panelTangent.sizeDelta.y) * 0.5f * ratioScreenCanvas.y);
                        if (contextMenu.broken) {
                            if (checkMark2 == null) {
                                checkMark2 = Instantiate(checkMark.gameObject).GetComponent<RectTransform>();
                                checkMark2.SetParent(panelTangent);
                                checkMark2.localPosition = new Vector2(-panelTangent.sizeDelta.x * 0.5f + checkMark.sizeDelta.x, 0);
                            }
                            checkMark2.gameObject.SetActive(true);

                            if (((tangentMenuItemHovered == leftTangent) && contextMenu.leftTangent.free) ||
                                ((tangentMenuItemHovered == rightTangent) && contextMenu.rightTangent.free) ||
                                ((tangentMenuItemHovered == bothTangents) && contextMenu.bothTangents.free)) {
                                checkMark2.localPosition = new Vector2(checkMark2.localPosition.x, freeTangent.localPosition.y);
                            } else if (((tangentMenuItemHovered == leftTangent) && contextMenu.leftTangent.linear) ||
                                ((tangentMenuItemHovered == rightTangent) && contextMenu.rightTangent.linear) ||
                                ((tangentMenuItemHovered == bothTangents) && contextMenu.bothTangents.linear)) {
                                checkMark2.localPosition = new Vector2(checkMark2.localPosition.x, linearTangent.localPosition.y);
                            } else if (((tangentMenuItemHovered == leftTangent) && contextMenu.leftTangent.constant) ||
                                ((tangentMenuItemHovered == rightTangent) && contextMenu.rightTangent.constant) ||
                                ((tangentMenuItemHovered == bothTangents) && contextMenu.bothTangents.constant)) {
                                checkMark2.localPosition = new Vector2(checkMark2.localPosition.x, constantTangent.localPosition.y);
                            } else {
                                checkMark2.gameObject.SetActive(false);
                            }
                        }
                    } else {
                        panelTangent.gameObject.SetActive(false);
                    }

                    if (tangentOverlay.gameObject.activeSelf) {
                        tangentOverlay.gameObject.SetActive(false);
                    }
                }
            } else if (panelAdd.gameObject.activeSelf) {
                bool hover = false;
                if (MouseOverPanelAdd()) {
                    addKeyOverlay.position = addKey.position;
                    hover = true;
                }
                if (hover != addKeyOverlay.gameObject.activeSelf) {
                    addKeyOverlay.gameObject.SetActive(hover);
                }
            }
        }

        bool MouseOverPanelAdd() {
            Vector2 mousePos = Input.mousePosition;
            return (panelAdd.rect.Contains(new Vector2((mousePos.x - panelAdd.position.x) /* invRatioScreenCanvas.x*/, (mousePos.y - panelAdd.position.y) /* invRatioScreenCanvas.y*/)));
        }

        void HidePanel() {
            if (panelTangent.gameObject.activeSelf) {
                panelTangent.gameObject.SetActive(false);
                tangentOverlay.gameObject.SetActive(false);
            }
            panel.gameObject.SetActive(false);
            gameObject.SetActive(false);
            tangentMenuItemHovered = null;
            overlay.gameObject.SetActive(false);
            if (checkMark2 != null) {
                Destroy(checkMark2.gameObject);
                checkMark2 = null;
            }
            interfaceContextMenuListener = null;
        }

        void HidePanelAdd() {
            panelAdd.gameObject.SetActive(false);
            gameObject.SetActive(false);
            addKeyOverlay.gameObject.SetActive(false);
            interfaceContextMenuListener = null;
        }

        void Hide() {
            if (panel.gameObject.activeSelf) {
                HidePanel();
            } else if (panelAdd.gameObject.activeSelf) {
                HidePanelAdd();
            }
        }

        void CheckSelectedItem() {
            Vector2 mousePos = Input.mousePosition;
            if (panel.gameObject.activeSelf) {
                bool tangMenuItemClicked = false;
                RectTransform selectedItem = null;
                foreach (RectTransform menuItem in menuItems) {
                    Vector2 menuItemPos = new Vector2(panel.position.x, menuItem.position.y);
                    if (menuItem.rect.Contains(new Vector2((mousePos.x - menuItemPos.x) * invRatioScreenCanvas.x, (mousePos.y - menuItemPos.y) * invRatioScreenCanvas.y))) {
                        if (tangentsMenuItems.Contains(menuItem)) {
                            tangMenuItemClicked = true;
                        } else {
                            selectedItem = menuItem;
                        }
                        break;
                    }
                }

                if (!tangMenuItemClicked) {
                    if (selectedItem == null) {
                        foreach (RectTransform menuItem in tangentTypesMenuItems) {
                            Vector2 menuItemPos = new Vector2(panelTangent.position.x, menuItem.position.y);
                            if (menuItem.rect.Contains(new Vector2((mousePos.x - menuItemPos.x) * invRatioScreenCanvas.x, (mousePos.y - menuItemPos.y) * invRatioScreenCanvas.y))) {
                                selectedItem = menuItem;
                                break;
                            }
                        }
                    }
                    TriggerListnerer(selectedItem);
                    Hide();
                }
            } else if (panelAdd.gameObject.activeSelf) {
                if (MouseOverPanelAdd()) {
                    interfaceContextMenuListener.AddKey();
                }
                Hide();
            }
        }

        void TriggerListnerer(RectTransform selectedItem) {
            if (deleteKey == selectedItem) {
                interfaceContextMenuListener.DeleteKey();
            } else if (editKey == selectedItem) {
                interfaceContextMenuListener.EditKey();
            } else if (clampedAuto == selectedItem) {
                interfaceContextMenuListener.ClampedAutoKey(true);
            } else if (auto == selectedItem) {
                interfaceContextMenuListener.AutoKey(true);
            } else if (freeSmooth == selectedItem) {
                interfaceContextMenuListener.FreeSmoothKey(true);
            } else if (flat == selectedItem) {
                interfaceContextMenuListener.FlatKey(true);
            } else if (broken == selectedItem) {
                interfaceContextMenuListener.BrokenKey(true);
            } else if (freeTangent == selectedItem) {
                interfaceContextMenuListener.Free(GetTangentPart(), true);
            } else if (linearTangent == selectedItem) {
                interfaceContextMenuListener.Linear(GetTangentPart(), true);
            } else if (constantTangent == selectedItem) {
                interfaceContextMenuListener.Constant(GetTangentPart(), true);
            }
        }

        TangentPart GetTangentPart() {
            TangentPart tangentPart = TangentPart.Left;
            if (tangentMenuItemHovered == rightTangent) {
                tangentPart = TangentPart.Right;
            } else if (tangentMenuItemHovered == bothTangents) {
                tangentPart = TangentPart.Both;
            }
            return tangentPart;
        }

    }
}