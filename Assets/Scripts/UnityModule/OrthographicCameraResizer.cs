using System.Linq;
using AccessorUtility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UnityModule
{
    /// <summary>
    /// Orthographic な Camera のサイズを変更する
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(RectTransform))]
    public class OrthographicCameraResizer : MonoBehaviour, IRectTransformAccessor
    {
        /// <summary>
        /// Canvas インスタンスの実体
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// Canvas インスタンス
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                if (this.canvas == default(Canvas))
                {
                    this.canvas = this.gameObject.GetComponent<Canvas>();
                }

                return this.canvas;
            }
            set { this.canvas = value; }
        }

        /// <summary>
        /// CanvasScaler インスタンスの実体
        /// </summary>
        private CanvasScaler canvasScaler;

        /// <summary>
        /// CanvasScaler インスタンス
        /// </summary>
        public CanvasScaler CanvasScaler
        {
            get
            {
                if (this.canvasScaler == default(CanvasScaler))
                {
                    this.canvasScaler = this.gameObject.GetComponent<CanvasScaler>();
                }

                return this.canvasScaler;
            }
            set { this.canvasScaler = value; }
        }

        /// <summary>
        /// 対象カメラの実体
        /// </summary>
        private Camera targetCamera;

        /// <summary>
        /// 対象カメラ
        /// </summary>
        public Camera TargetCamera
        {
            get
            {
                if (this.targetCamera == default(Camera))
                {
                    this.targetCamera = this.Canvas.worldCamera;
                }

                return this.targetCamera;
            }
            set
            {
                this.subjectRectTransformSizeDeltaY.OnNext(this.RectTransform().sizeDelta.y);
                this.targetCamera = value;
            }
        }

        /// <summary>
        /// RectTransform の sizeDelta.y の変化を通知するストリーム
        /// </summary>
        private BehaviorSubject<float> subjectRectTransformSizeDeltaY;

        /// <summary>
        /// Unity lifecycle: Start
        /// </summary>
        /// <remarks>RectTransform.sizeDelta.y の変化を監視して、Camera.orthographicSize を変更するイベントを仕込む</remarks>
        private void Start()
        {
            this.subjectRectTransformSizeDeltaY = new BehaviorSubject<float>(this.RectTransform().sizeDelta.y);
            this.ObserveEveryValueChanged(_ => this.RectTransform().sizeDelta.y)
                .Subscribe(this.subjectRectTransformSizeDeltaY);
            this.subjectRectTransformSizeDeltaY
                .Where(_ => this.TargetCamera != default(Camera))
                .Subscribe(
                    (value) =>
                    {
                        gameObject
                            .scene
                            .GetRootGameObjects()
                            .SelectMany(x => x.GetComponentsInChildren<Camera>())
                            .ToList()
                            .ForEach(
                                x => { x.orthographicSize = value / 2.0f / this.CanvasScaler.referencePixelsPerUnit; }
                            );
                    }
                );
        }
    }
}