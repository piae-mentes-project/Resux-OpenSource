using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;
using Resux.Extension;

namespace Resux.UI
{
    // [RequireComponent(typeof(ExtendEventTrigger))]
    public class ScrollImage : MonoBehaviour
    {
        [Serializable]
        public class ImageAnimationArg
        {
            [Header("Rect Transform")]
            public Vector3 position;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector3 scale;

            public ImageAnimationArg() { }
            public ImageAnimationArg(RectTransform rt)
            {
                position = rt.anchoredPosition;
                anchorMin = rt.anchorMin;
                anchorMax = rt.anchorMax;
                scale = rt.localScale;
            }
        }

        public interface ITextureProvider
        {
            Texture2D GetTexture(int index);
        }

        public enum InputAhead
        {
            None,
            Previous,
            Next,
        }

        public enum PlayingAnimType
        {
            Prev,
            None,
            Next,
        }

        [Header("Image Settings")]
        [SerializeField] private RawImage[] _images;
        [SerializeField] private int _currentImageIndex;

        [Header("Arguments")]
        [SerializeField] private ITextureProvider _textureProvider;

        [Header("Events")]
        public readonly UnityEvent onIndexChanged = new UnityEvent();

        [Header("Animation")]
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _animCurve;
        [SerializeField] private ImageAnimationArg _prevAdditionalArg;
        [SerializeField] private ImageAnimationArg _nextAdditionalArg;
        private ImageAnimationArg[] _imageAnimArgs;
        private RectTransform[] _imageRectTransforms;
        private PlayingAnimType _playingAnimType = PlayingAnimType.None;
        private float _startPlayTime;
        private int _focusIndex;

        [Header("Drag")]
        // [SerializeField] private ExtendEventTrigger _eventTrigger;
        [SerializeField] private DraggableUIElement _draggableElement;
        [SerializeField] private float _dragChangeX = 1080;

        private int _index = 0;
        private InputAhead _inputAhead = InputAhead.None;
        private int _maxIndex = 0;
        private float _dragStartX = 0;
        private int _prevImageNumber = 0;
        private int _nextImageNumber = 0;
        private bool isDragEnable = true;

        public ITextureProvider TextureProvider
        {
            get => _textureProvider;
            set
            {
                _textureProvider = value;
            }
        }
        public int MaxIndex
        {
            get => _maxIndex;
            set
            {
                if (value < 0 || value < _index) { throw new ArgumentException("MaxIndex can't smaller than 0 or current selected index"); }
                _maxIndex = value;
            }
        }
        public int Index
        {
            get => _index;
            /// <summary>
            /// 不会产生事件和动画
            /// </summary>
            set
            {
                _index = value;
                onIndexChanged.Invoke();
            }
        }
        public int FocusIndex
        {
            get => _focusIndex;
            set => _focusIndex = value;
        }
        public bool IsDraging { get; private set; }
        public bool IsAnimationPlaying { get; private set; }
        private int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                _currentImageIndex = value;
                _images.GetCircle(_currentImageIndex).gameObject.name = "Current";
                for (var i = 1; i < _prevImageNumber + 1; i++)
                {
                    _images.GetCircle(_currentImageIndex - i).gameObject.name = "Previous " + i.ToString();
                }
                for (var i = 1; i < _nextImageNumber + 1; i++)
                {
                    _images.GetCircle(_currentImageIndex + i).gameObject.name = "Next " + i.ToString();
                }
                _images.GetCircle(AdditionalImageIndex).gameObject.name = "Additional";
            }
        }
        private int AdditionalImageIndex => CurrentImageIndex + _nextImageNumber + 1;

        public void Init(int maxIndex, int index)
        {
            _maxIndex = maxIndex;
            _index = index;
            _focusIndex = index;

            _draggableElement.onDrag += OnDrag;
            _draggableElement.onEndDrag += (e, direction) => OnEndDrag(e);

            _imageRectTransforms = _images.Select(img => img.GetComponent<RectTransform>()).ToArray();
            _imageAnimArgs = _imageRectTransforms.Take(_images.Length - 1).Select(rt => new ImageAnimationArg(rt)).ToArray();
            _prevImageNumber = CurrentImageIndex;
            _nextImageNumber = _images.Length - _prevImageNumber - 2;

            _images[CurrentImageIndex].gameObject.SetActive(true);
            _images[CurrentImageIndex].texture = TextureProvider.GetTexture(FocusIndex);
            for (int i = 1; i < _prevImageNumber + 1; i++)
            {
                if (FocusIndex - i < 0)
                {
                    _images[CurrentImageIndex - i].gameObject.SetActive(false);
                    return;
                }
                _images[CurrentImageIndex - i].gameObject.SetActive(true);
                _images[CurrentImageIndex - i].texture = TextureProvider.GetTexture(FocusIndex - i);
            }
            for (int i = 1; i < _nextImageNumber + 1; i++)
            {
                if (FocusIndex + 1 > MaxIndex)
                {
                    _images[CurrentImageIndex + i].gameObject.SetActive(false);
                    return;
                }
                _images[CurrentImageIndex + i].gameObject.SetActive(true);
                _images[CurrentImageIndex + i].texture = TextureProvider.GetTexture(FocusIndex + i);
            }
            _images[AdditionalImageIndex].gameObject.SetActive(false);
        }

        #region Drag

        private void OnDrag(PointerEventData data)
        {
            if (!isDragEnable)
            {
                return;
            }

            if (IsDraging)
            {
                var deltaX = data.position.x - _dragStartX;
                if ((deltaX < 0 && Index >= MaxIndex) || deltaX > 0 && Index <= 0)
                {
                    _dragStartX = data.position.x;
                    _playingAnimType = PlayingAnimType.None;
                    _images.GetCircle(AdditionalImageIndex).gameObject.SetActive(false);

                    var start = CurrentImageIndex - _prevImageNumber;
                    for (var i = 0; i < _imageAnimArgs.Length; i++)
                    {
                        SetImageAnim(_imageRectTransforms.GetCircle(start + i), _imageAnimArgs[i], _imageAnimArgs[i], 1f, _animCurve);
                    }
                    SetImageAnim(_imageRectTransforms.GetCircle(AdditionalImageIndex), _nextAdditionalArg, _nextAdditionalArg, 1f, _animCurve);
                    return;
                }
                if (deltaX >= _dragChangeX)
                {
                    _dragStartX += _dragChangeX;
                    Index--;
                    FocusIndex--;
                    CurrentImageIndex--;
                    _playingAnimType = PlayingAnimType.Prev;
                    UpdateAdditionalImage();
                    OnDrag(data);
                    return;
                }
                else if (deltaX <= -_dragChangeX)
                {
                    _dragStartX -= _dragChangeX;
                    Index++;
                    FocusIndex++;
                    CurrentImageIndex++;
                    _playingAnimType = PlayingAnimType.Next;
                    UpdateAdditionalImage();
                    OnDrag(data);
                    return;
                }
                _playingAnimType = deltaX < 0 ? PlayingAnimType.Next : PlayingAnimType.Prev;
                SetImagesAnim(_playingAnimType, Mathf.Abs(deltaX) / _dragChangeX, _animCurve);
                UpdateAdditionalImage();
            }
            else
            {
                if (IsAnimationPlaying) { return; }
                _dragStartX = data.position.x;
                IsDraging = true;
                OnDrag(data);
            }
        }

        private void OnEndDrag(PointerEventData data)
        {
            if (!isDragEnable)
            {
                return;
            }

            if (!IsDraging || _playingAnimType == PlayingAnimType.None)
            {
                IsDraging = false;
                return;
            }
            IsDraging = false;
            
            var deltaX = Mathf.Abs(data.position.x - _dragStartX);
            if (deltaX == 0f) { return; }
            if (deltaX / _dragChangeX < 0.5f)
            {
                if (_playingAnimType == PlayingAnimType.Next)
                {
                    _playingAnimType = PlayingAnimType.Prev;
                    CurrentImageIndex++;
                    FocusIndex++;
                }
                else
                {
                    _playingAnimType = PlayingAnimType.Next;
                    CurrentImageIndex--;
                    FocusIndex--;
                }
            }
            else { Index += (int)_playingAnimType - 1; }
            IsAnimationPlaying = true;
            _startPlayTime = Time.time - _duration * Mathf.Max(deltaX, _dragChangeX - deltaX) / _dragChangeX;
        }
        #endregion

        public void SelectPrevious()
        {
            if (IsAnimationPlaying)
            {
                _inputAhead = InputAhead.Previous;
                return;
            }
            _inputAhead = InputAhead.None;
            Select(Index - 1);
        }
        public void SelectNext()
        {
            if (IsAnimationPlaying)
            {
                _inputAhead = InputAhead.Next;
                return;
            }
            _inputAhead = InputAhead.None;
            Select(Index + 1);
        }
        public void Select(int index)
        {
            if (Index == index || index < 0 || index > MaxIndex || IsDraging || IsAnimationPlaying) { return; }
            Index = index;
            OnAnimationFinish();
        }

        private void Update()
        {
            if (IsAnimationPlaying)
            {
                var progress = Mathf.Clamp01((Time.time - _startPlayTime) / _duration);
                SetImagesAnim(_playingAnimType, progress, _animCurve);
                if (progress >= 1f)
                {
                    IsAnimationPlaying = false;
                    OnAnimationFinish();
                }
            }
        }

        public void SetDragEnable(bool enable) => isDragEnable = enable;

        #region Animation
        private void SetImagesAnim(PlayingAnimType playingType, float progress, AnimationCurve curve)
        {
            if (playingType == PlayingAnimType.Next)  // 选择下一个，全部往前动
            {
                var start = CurrentImageIndex - _prevImageNumber;
                SetImageAnim(_imageRectTransforms.GetCircle(start), _imageAnimArgs[0], _prevAdditionalArg, progress, curve);
                for (var i = 1; i < _imageAnimArgs.Length; i++)
                {
                    SetImageAnim(_imageRectTransforms.GetCircle(start + i), _imageAnimArgs[i], _imageAnimArgs[i - 1], progress, curve);
                }
                SetImageAnim(_imageRectTransforms.GetCircle(AdditionalImageIndex), _nextAdditionalArg, _imageAnimArgs[_imageAnimArgs.Length - 1], progress, curve);
            }
            else if (playingType == PlayingAnimType.Prev)  // 选择上一个，全部往后动
            {
                var start = CurrentImageIndex - _prevImageNumber;
                SetImageAnim(_imageRectTransforms.GetCircle(CurrentImageIndex + _nextImageNumber), _imageAnimArgs[_imageAnimArgs.Length - 1], _nextAdditionalArg, progress, curve);
                for (var i = 0; i < _imageAnimArgs.Length - 1; i++)
                {
                    SetImageAnim(_imageRectTransforms.GetCircle(start + i), _imageAnimArgs[i], _imageAnimArgs[i + 1], progress, curve);
                }
                SetImageAnim(_imageRectTransforms.GetCircle(AdditionalImageIndex), _prevAdditionalArg, _imageAnimArgs[0], progress, curve);
            }
        }

        private void SetImageAnim(RectTransform rt, ImageAnimationArg from, ImageAnimationArg to, float progress, AnimationCurve curve)
        {
            rt.anchoredPosition = from.position + (to.position - from.position) * curve.Evaluate(progress);
            rt.anchorMin = from.anchorMin + (to.anchorMin - from.anchorMin) * curve.Evaluate(progress);
            rt.anchorMax = from.anchorMax + (to.anchorMax - from.anchorMax) * curve.Evaluate(progress);
            rt.localScale = from.scale + (to.scale - from.scale) * curve.Evaluate(progress);
        }

        private void OnAnimationFinish()
        {
            if (_playingAnimType == PlayingAnimType.Prev)
            {
                FocusIndex--;
                CurrentImageIndex--;
            }
            else if (_playingAnimType == PlayingAnimType.Next)
            {
                FocusIndex++;
                CurrentImageIndex++;
            }

            if (FocusIndex < Index) { _playingAnimType = PlayingAnimType.Next; }
            else if (FocusIndex > Index) { _playingAnimType = PlayingAnimType.Prev; }
            else
            {
                _playingAnimType = PlayingAnimType.None;
                IsAnimationPlaying = false;
                if (_inputAhead == InputAhead.Next) { SelectNext(); }
                else if (_inputAhead == InputAhead.Previous) { SelectPrevious(); }
                return;
            }

            UpdateAdditionalImage();
            _startPlayTime = Time.time;
            IsAnimationPlaying = true;
        }
        #endregion

        #region Texture
        private void UpdateAdditionalImage()
        {
            var additionalImage = _images.GetCircle(AdditionalImageIndex);
            var index = 0;
            if (_playingAnimType == PlayingAnimType.Next) { index = FocusIndex + _nextImageNumber + 1; }
            else if (_playingAnimType == PlayingAnimType.Prev) { index = FocusIndex - _nextImageNumber - 1; }
            if (_playingAnimType != PlayingAnimType.None && index >= 0 && index <= MaxIndex)
            {
                additionalImage.gameObject.SetActive(true);
                additionalImage.texture = TextureProvider.GetTexture(index);
            }
            else { additionalImage.gameObject.SetActive(false); }
        }

        public void ReplaceTexture(Texture2D texture, int index)
        {
            var image = GetRawImage(index);
            if (image == null) { return; }
            image.texture = texture;
        }

        private RawImage GetRawImage(int index)
        {
            if (index < 0 || index > MaxIndex) { return null; }
            if (index - FocusIndex > _nextImageNumber + (_playingAnimType == PlayingAnimType.Next ? 1 : 0)) { return null; }
            if (FocusIndex - index > _prevImageNumber + (_playingAnimType == PlayingAnimType.Prev ? 1 : 0)) { return null; }
            return _images.GetCircle(CurrentImageIndex + (index - FocusIndex));
        }
        #endregion
    }
}
