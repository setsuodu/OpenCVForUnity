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

                foreach (var unityRect in result)
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

            //2、关键点定位。检测人脸标记点landmark
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
            List<List<Vector2>> landmarkPoints = new List<List<Vector2>>();
            foreach (var openCVRect in detectResult)
            {
                UnityEngine.Rect rect = new UnityEngine.Rect(openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);
                
                Debug.Log("face : " + rect);
                //OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 0, 0, 255), 2); //画框rect

                List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);
                //OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, points, new Scalar(0, 255, 0, 255), 2); //画框landmark
                landmarkPoints.Add(points);
            }

            /*
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
            */

            //开始换脸
            int[] face_nums = new int[landmarkPoints.Count];
            for (int i = 0; i < face_nums.Length; i++)
            {
                face_nums[i] = i;
            }
            face_nums = face_nums.OrderBy(i => System.Guid.NewGuid()).ToArray(); //随机
            if (landmarkPoints.Count >= 2)
            {
                DlibFaceChanger faceChanger = new DlibFaceChanger();
                faceChanger.isShowingDebugFacePoints = displayDebugFacePoints;
                faceChanger.SetTargetImage(rgbaMat);

                for (int i = 1; i < face_nums.Length; i++)
                {
                    faceChanger.AddFaceChangeData(rgbaMat, landmarkPoints[face_nums[0]], landmarkPoints[face_nums[i]], 1);
                }

                faceChanger.ChangeFace();
                faceChanger.Dispose();
            }

            /*
            //画人脸rect
            if (displayFaceRects && face_nums.Count() > 0)
            {
                int ann = face_nums[0];
                UnityEngine.Rect rect_ann = new UnityEngine.Rect(detectResult[ann].x, detectResult[ann].y, detectResult[ann].width, detectResult[ann].height);
                OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect_ann, new Scalar(255, 255, 0, 255), 2);

                int bob = 0;
                for (int i = 1; i < face_nums.Length; i++)
                {
                    bob = face_nums[i];
                    UnityEngine.Rect rect_bob = new UnityEngine.Rect(detectResult[bob].x, detectResult[bob].y, detectResult[bob].width, detectResult[bob].height);
                    OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect_bob, new Scalar(255, 0, 0, 255), 2);
                }
            }

            frontalFaceChecker.Dispose();
            */

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
