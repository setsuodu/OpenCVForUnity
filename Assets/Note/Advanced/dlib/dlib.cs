using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DlibFaceLandmarkDetector;
using OpenCVForUnity;
using OpenCVForUnity.FaceChange;

namespace FaceSwapperExample
{
    public class dlib : MonoBehaviour
    {
        public bool useDlibFaceDetecter = false;
        public Toggle useDlibFaceDetecterToggle;

        public bool filterNonFrontalFaces;
        public Toggle filterNonFrontalFacesToggle;

        [Range(0.0f, 1.0f)]
        public float frontalFaceRateLowerLimit;
        public bool displayFaceRects = false;

        public Toggle displayFaceRectsToggle;
        public bool displayDebugFacePoints = false;

        public Toggle displayDebugFacePointsToggle;

        CascadeClassifier cascade;
        FaceLandmarkDetector faceLandmarkDetector;
        string haarcascade_frontalface_alt_xml_filepath;
        string sp_human_face_68_dat_filepath;

        Texture2D imgTexture;
        [SerializeField] private Image dstImage;

        void Start()
        {
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath("sp_human_face_68.dat");
            faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath);

            imgTexture = Resources.Load("mc") as Texture2D;

            Run();
        }

        void Run()
        {
            Mat rgbaMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);
            OpenCVForUnity.Utils.texture2DToMat(imgTexture, rgbaMat);

            FrontalFaceChecker frontalFaceChecker = new FrontalFaceChecker(imgTexture.width, imgTexture.height); //检测正面脸，范围整个t2d

            //1、人脸检测。两种检测人脸rect方法，toggle切换
            List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect>();
            if (useDlibFaceDetecter)
            {
                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();

                foreach (UnityEngine.Rect unityRect in result)
                {
                    detectResult.Add(new OpenCVForUnity.Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                }
            }
            else
            {
                cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);

                Mat gray = new Mat();
                Imgproc.cvtColor(rgbaMat, gray, Imgproc.COLOR_RGBA2GRAY);

                MatOfRect faces = new MatOfRect();
                Imgproc.equalizeHist(gray, gray);
                cascade.detectMultiScale(gray, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size(gray.cols() * 0.05, gray.cols() * 0.05), new Size());

                detectResult = faces.toList();

                // adjust to Dilb's result.
                foreach (OpenCVForUnity.Rect r in detectResult)
                {
                    r.y += (int)(r.height * 0.1f);
                }
                gray.Dispose();
            }

            //0-拷贝到->1，靠list中排序，改变顺序
            detectResult.Sort(delegate (OpenCVForUnity.Rect x, OpenCVForUnity.Rect y)
            {
                return x.x.CompareTo(y.x);   //顺序与delegate中一致，升序排列
                //return y.x.CompareTo(x.x); //顺序与delegate中不同，降序排列
            });
            //Debug.Log("[0]" + detectResult[0].x + ", [1]" + detectResult[1].x);

            //2、关键点定位。通过上一步的rect，检测人脸标记点landmark
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
            List<List<Vector2>> landmarkPoints = new List<List<Vector2>>();


            //TODO...只检测一张脸，另一张改成UV图，手动输入68个points
            /*
            foreach (OpenCVForUnity.Rect openCVRect in detectResult)
            {
                UnityEngine.Rect rect = new UnityEngine.Rect(openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);
                //Debug.Log("face : " + rect);

                OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 0, 0, 255), 2); //画框rect
                Imgproc.circle(rgbaMat, new Point(rect.x, rect.y), 0, new Scalar(0, 255, 0, 255), 5, Imgproc.LINE_8, 0);
                Imgproc.putText(rgbaMat, rect.x + "x" + rect.y, new Point(rect.x + 5, rect.y -5), 1, 1, new Scalar(0, 255, 0, 255));

                List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);
                //OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, points, new Scalar(0, 255, 0, 255), 2); //画框landmark
                landmarkPoints.Add(points);
            }
            */

            //检测一张照片的脸的landmark点
            OpenCVForUnity.Rect openCVRect = detectResult[1];
            UnityEngine.Rect rect = new UnityEngine.Rect(openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);
            //OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 0, 0, 255), 2); //画框rect
            //Imgproc.circle(rgbaMat, new Point(rect.x, rect.y), 0, new Scalar(0, 255, 0, 255), 5, Imgproc.LINE_8, 0);
            //Imgproc.putText(rgbaMat, rect.x + "x" + rect.y, new Point(rect.x + 5, rect.y - 5), 1, 1, new Scalar(0, 255, 0, 255));
            List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect); //通过检测器从rect中提取point
            //OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, points, new Scalar(0, 255, 0, 255), 2); //画框landmark
            landmarkPoints.Add(points);

            string log = "";
            for (int i = 0; i < points.Count; i++)
            {
                //Debug.Log("[" + i + "] " + points[i]);
                log += "new Vector2(" + ((int)points[i].x - 100) + "," + (int)points[i].y + "),\n";
                //Imgproc.circle(rgbaMat, new Point(landmarkPoints[0][i].x, landmarkPoints[0][i].y), 0, new Scalar(0, 255, 0, 255), 2, Imgproc.LINE_8, 0); //绘制68点
                /* * * * * * * * * * * * *
                 * jaw;       // [0-16]  *
                 * rightBrow; // [17-21] *
                 * leftBrow;  // [22-26] *
                 * nose;      // [27-35] *
                 * rightEye;  // [36-41] *
                 * leftEye;   // [42-47] *
                 * mouth;     // [48-59] *
                 * mouth2;    // [60-67] *
                 * * * * * * * * * * * * */
                /*
                if (i >= 0 && i <= 16)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(255, 0, 0, 255), 2, Imgproc.LINE_8, 0); //红
                }
                else if (i >= 17 && i <= 21)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(255, 127, 0, 255), 2, Imgproc.LINE_8, 0); //橙
                }
                else if (i >= 22 && i <= 26)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(255, 255, 0, 255), 2, Imgproc.LINE_8, 0); //黄
                }
                else if (i >= 27 && i <= 35)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(0, 255, 0, 255), 2, Imgproc.LINE_8, 0); //绿
                }
                else if (i >= 36 && i <= 21)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(0, 255, 170, 255), 2, Imgproc.LINE_8, 0); //青
                }
                else if (i >= 42 && i <= 47)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(0, 0, 255, 255), 2, Imgproc.LINE_8, 0); //蓝
                }
                else if (i >= 48 && i <= 59)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(128, 255, 0, 255), 2, Imgproc.LINE_8, 0); //紫
                }
                else if (i >= 60 && i <= 67)
                {
                    Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 0, new Scalar(255, 180, 255, 255), 2, Imgproc.LINE_8, 0); //粉红
                }
                */
                Imgproc.circle(rgbaMat, new Point(10, 10), 0, new Scalar(255, 0, 0, 255), 4, Imgproc.LINE_8, 0);
                Imgproc.putText(rgbaMat, rect.x + "x" + rect.y, new Point(rect.x + 5, rect.y - 5), 1, 1, new Scalar(0, 255, 0, 255));
            }
            //Debug.Log(log);


            //手写一张UV的landmark点
            List<Vector2> uvs = new List<Vector2>()
            {
                new Vector2(192,109),
                new Vector2(193,123),
                new Vector2(196,137),
                new Vector2(200,152),
                new Vector2(205,167),
                new Vector2(211,182),
                new Vector2(220,195),
                new Vector2(231,206),
                new Vector2(246,209),
                new Vector2(263,206),
                new Vector2(279,197),
                new Vector2(293,184),
                new Vector2(303,169),
                new Vector2(309,152),
                new Vector2(311,134),
                new Vector2(312,117),
                new Vector2(312,99),
                new Vector2(192,97),
                new Vector2(196,89),
                new Vector2(206,87),
                new Vector2(216,90),
                new Vector2(226,94),
                new Vector2(245,92),
                new Vector2(256,86),
                new Vector2(269,84),
                new Vector2(281,85),
                new Vector2(290,92),
                new Vector2(235,103),
                new Vector2(235,114),
                new Vector2(235,124),
                new Vector2(234,134),
                new Vector2(227,143),
                new Vector2(232,145),
                new Vector2(238,146),
                new Vector2(244,144),
                new Vector2(251,141),
                new Vector2(204,107),
                new Vector2(209,102),
                new Vector2(217,102),
                new Vector2(225,107),
                new Vector2(217,109),
                new Vector2(209,110),
                new Vector2(255,106),
                new Vector2(262,100),
                new Vector2(270,100),
                new Vector2(278,104),
                new Vector2(271,107),
                new Vector2(263,107),
                new Vector2(220,164),
                new Vector2(227,160),
                new Vector2(235,157),
                new Vector2(241,158),
                new Vector2(247,156),
                new Vector2(259,157),
                new Vector2(271,159),
                new Vector2(261,171),
                new Vector2(250,176),
                new Vector2(242,178),
                new Vector2(236,178),
                new Vector2(228,174),
                new Vector2(223,165),
                new Vector2(235,162),
                new Vector2(241,161),
                new Vector2(248,160),
                new Vector2(267,161),
                new Vector2(249,169),
                new Vector2(242,171),
                new Vector2(236,170),
            };
            landmarkPoints.Add(uvs);

            //过滤非正面的脸
            if (filterNonFrontalFaces)
            {
                for (int i = 0; i < landmarkPoints.Count; i++)
                {
                    if (frontalFaceChecker.GetFrontalFaceRate(landmarkPoints[i]) < frontalFaceRateLowerLimit) //阈值0~1
                    {
                        detectResult.RemoveAt(i);
                        landmarkPoints.RemoveAt(i);
                        i--;
                    }
                }
            }

            //1张脸时只提取关键点，2张脸才换脸
            if (landmarkPoints.Count == 2)
            {
                //开始换脸
                DlibFaceChanger faceChanger = new DlibFaceChanger();
                faceChanger.isShowingDebugFacePoints = displayDebugFacePoints;
                faceChanger.SetTargetImage(rgbaMat);
                //从0拷贝到1
                //faceChanger.AddFaceChangeData(rgbaMat, landmarkPoints[0], landmarkPoints[1], 1); //src-> dst
                faceChanger.ChangeFace();
                faceChanger.Dispose();
            }

            frontalFaceChecker.Dispose();

            Texture2D t2d = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D(rgbaMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            dstImage.sprite = sp;
            dstImage.preserveAspect = true;

            rgbaMat.Dispose();
        }

        public void OnShuffleButtonClick()
        {
            if (imgTexture != null)
                Run();
        }

        public void OnUseDlibFaceDetecterToggleValueChanged()
        {
            if (useDlibFaceDetecterToggle.isOn)
            {
                useDlibFaceDetecter = true;
            }
            else
            {
                useDlibFaceDetecter = false;
            }

            if (imgTexture != null)
                Run();
        }

        public void OnFilterNonFrontalFacesToggleValueChanged()
        {
            if (filterNonFrontalFacesToggle.isOn)
            {
                filterNonFrontalFaces = true;
            }
            else
            {
                filterNonFrontalFaces = false;
            }

            if (imgTexture != null)
                Run();
        }

        public void OnDisplayFaceRectsToggleValueChanged()
        {
            if (displayFaceRectsToggle.isOn)
            {
                displayFaceRects = true;
            }
            else
            {
                displayFaceRects = false;
            }

            if (imgTexture != null)
                Run();
        }

        public void OnDisplayDebugFacePointsToggleValueChanged()
        {
            if (displayDebugFacePointsToggle.isOn)
            {
                displayDebugFacePoints = true;
            }
            else
            {
                displayDebugFacePoints = false;
            }

            if (imgTexture != null)
                Run();
        }
    }
}
