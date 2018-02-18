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
    public class hull : MonoBehaviour
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
        [SerializeField] private Image srcImage;
        //[SerializeField] private Image dstImage;
        Mat imgMat, gray1Mat, downScaleRgbaMat;
        int EDGE_DETECT_VALUE = 70;
        double POINT_RATE = 0.075;
        int POINT_MAX_NUM = 2500;
        Subdiv2D subdiv;
        List<Point> pointList;

        void Start()
        {
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath("sp_human_face_68.dat");
            faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath);

            imgTexture = Resources.Load("changer") as Texture2D;

            //Run();
            CvHull();
        }

        void CvHull()
        {
            //Mat imgMat = new Mat(500, 500, CvType.CV_8UC3, new Scalar(0, 0, 0));
            imgMat = Imgcodecs.imread(Application.dataPath + "/Resources/aragaki.jpg");

            //1. 人脸dlib检测
            List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect>();
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, imgMat);
            List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();
            foreach (UnityEngine.Rect unityRect in result)
            {
                detectResult.Add(new OpenCVForUnity.Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
            }
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, imgMat);
            List<List<Vector2>> landmarkPoints = new List<List<Vector2>>();
            OpenCVForUnity.Rect openCVRect = detectResult[0];
            UnityEngine.Rect rect = new UnityEngine.Rect(openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);
            List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect); //通过检测器从rect中提取point
            landmarkPoints.Add(points);


            //2. 计算凸包
            pointList = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                //绘制点
                //Imgproc.circle(imgMat, new Point(points[i].x, points[i].y), 2, new Scalar(255, 255, 255), -1);
                Point pt = new Point(landmarkPoints[0][i].x, landmarkPoints[0][i].y);
                Imgproc.circle(imgMat, pt, 0, new Scalar(0, 255, 0, 255), 2, Imgproc.LINE_8, 0); //绘制68点
                pointList.Add(pt);
            }
            //Debug.Log(pointList.Count);

            MatOfPoint pointsMat = new MatOfPoint();
            pointsMat.fromList(pointList);

            MatOfInt hullInt = new MatOfInt();
            Imgproc.convexHull(pointsMat, hullInt);

            List<Point> pointMatList = pointsMat.toList();
            List<int> hullIntList = hullInt.toList();
            List<Point> hullPointList = new List<Point>();

            for (int j = 0; j < hullInt.toList().Count; j++)
            {
                hullPointList.Add(pointMatList[hullIntList[j]]);
            }

            MatOfPoint hullPointMat = new MatOfPoint();
            hullPointMat.fromList(hullPointList);

            List<MatOfPoint> hullPoints = new List<MatOfPoint>();
            hullPoints.Add(hullPointMat);

            Imgproc.drawContours(imgMat, hullPoints, -1, new Scalar(0, 255, 0), 2);
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_BGR2RGB);

            //3. 三角剖份
            Triangle();


            //4. 显示
            Texture2D t2d = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D(imgMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            srcImage.sprite = sp;
            srcImage.preserveAspect = true;
        }

        void Triangle()
        {
            gray1Mat = new Mat();
            downScaleRgbaMat = imgMat.clone();
            Imgproc.cvtColor(imgMat, gray1Mat, Imgproc.COLOR_RGBA2GRAY);

            //init subdiv
            subdiv = new Subdiv2D();
            subdiv.initDelaunay(new OpenCVForUnity.Rect(0, 0, downScaleRgbaMat.width(), downScaleRgbaMat.height()));
            for (int i = 0; i < pointList.Count; i++)
            {
                subdiv.insert(pointList[i]);
            }
            Debug.Log(pointList.Count); //68

            //框上添加8个点
            subdiv.insert(new Point(0, 0));
            subdiv.insert(new Point(gray1Mat.width() / 2 - 1, 0));
            subdiv.insert(new Point(gray1Mat.width() - 1, 0));
            subdiv.insert(new Point(gray1Mat.width() - 1, gray1Mat.height() / 2 - 1));
            subdiv.insert(new Point(gray1Mat.width() - 1, gray1Mat.height() - 1));
            subdiv.insert(new Point(gray1Mat.width() / 2 - 1, gray1Mat.height() - 1));
            subdiv.insert(new Point(0, gray1Mat.height() - 1));
            subdiv.insert(new Point(0, gray1Mat.height() / 2 - 1));

            using (MatOfFloat6 triangleList = new MatOfFloat6())
            {
                subdiv.getTriangleList(triangleList);
                //Debug.Log(triangleList.size().area()); //154

                float[] pointArray = triangleList.toArray();
                //Debug.Log(pointArray.Length); //154*6

                byte[] color = new byte[4];
                for (int i = 0; i < pointArray.Length / 6; i++)
                {
                    Point p0 = new Point(pointArray[i * 6 + 0], pointArray[i * 6 + 1]);
                    Point p1 = new Point(pointArray[i * 6 + 2], pointArray[i * 6 + 3]);
                    Point p2 = new Point(pointArray[i * 6 + 4], pointArray[i * 6 + 5]);

                    if (p0.x < 0 || p0.x > imgMat.width())
                        continue;
                    if (p0.y < 0 || p0.y > imgMat.height())
                        continue;
                    if (p1.x < 0 || p1.x > imgMat.width())
                        continue;
                    if (p1.y < 0 || p1.y > imgMat.height())
                        continue;
                    if (p2.x < 0 || p2.x > imgMat.width())
                        continue;
                    if (p2.y < 0 || p2.y > imgMat.height())
                        continue;

                    //get center of gravity
                    int cx = (int)((p0.x + p1.x + p2.x) * 0.33333);
                    int cy = (int)((p0.y + p1.y + p2.y) * 0.33333);
                    //Debug.Log ("cx " + cx + " cy " + cy );

                    //get center of gravity color
                    imgMat.get(cy, cx, color);
                    //Debug.Log ("r " + color[0] + " g " + color[1] + " b " + color[2] + " a " + color[3]);

                    //fill Polygon
                    //Imgproc.fillConvexPoly(rgbaMat, new MatOfPoint(p0, p1, p2), new Scalar(color[0], color[1], color[2], color[3]), Imgproc.LINE_AA, 0);
                    Imgproc.line(imgMat, p0, p1, new Scalar(64, 255, 128, 255));
                    Imgproc.line(imgMat, p1, p2, new Scalar(64, 255, 128, 255));
                    Imgproc.line(imgMat, p2, p0, new Scalar(64, 255, 128, 255));
                }
            }
        }

        void Run()
        {
            Mat rgbaMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);
            OpenCVForUnity.Utils.texture2DToMat(imgTexture, rgbaMat);
            //Mat dstMat = Imgcodecs.imread(Application.dataPath + "/Resources/changer.jpg");
            //Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGB);

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
            OpenCVForUnity.Rect openCVRect = detectResult[0];
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
                log += "new Vector2(" + ((int)points[i].x + 500) + "," + (int)points[i].y + "),\n";
                //Imgproc.circle(rgbaMat, new Point(10, 10), 0, new Scalar(255, 0, 0, 255), 4, Imgproc.LINE_8, 0); //检查原点
                //Imgproc.putText(rgbaMat, i.ToString(), new Point(landmarkPoints[0][i].x, landmarkPoints[0][i].y), 1, 1, new Scalar(0, 255, 0, 255));
                //Imgproc.circle(rgbaMat, new Point(landmarkPoints[0][i].x, landmarkPoints[0][i].y), 0, new Scalar(0, 255, 0, 255), 2, Imgproc.LINE_8, 0); //绘制68点
                //Imgproc.putText(rgbaMat, rect.x + "x" + rect.y, new Point(rect.x + 5, rect.y - 5), 1, 1, new Scalar(0, 255, 0, 255));
                //Imgproc.putText(rgbaMat, rect.x + "x" + rect.y, new Point(rect.x + 5, rect.y - 5), 1, 1, new Scalar(0, 255, 0, 255));
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
            }
            //Debug.Log(log);


            //手写一张UV的landmark点
            List<Vector2> uvs = new List<Vector2>()
            {
                //0-7
                new Vector2(583,210), //0
                new Vector2(584,250), //1
                new Vector2(585,305), //2
                new Vector2(600,360), //3
                new Vector2(630,410), //4
                new Vector2(670,425), //5
                new Vector2(710,440), //6
                new Vector2(740,445), //7
                //8
                new Vector2(770,450), //8+轴
                //9-16
                new Vector2(800,445), //9
                new Vector2(830,440), //10
                new Vector2(870,425), //11
                new Vector2(910,410), //12
                new Vector2(940,360), //13
                new Vector2(955,305), //14
                new Vector2(956,250), //15
                new Vector2(957,210), //16
                //17-21 leftBrow
                new Vector2(655,165), //17
                new Vector2(680,155), //18
                new Vector2(710,160), //19
                new Vector2(730,170), //20
                new Vector2(750,190), //21
                //22-26 rightBrow
                new Vector2(790,190), //22
                new Vector2(810,170), //23
                new Vector2(830,160), //24
                new Vector2(860,155), //25
                new Vector2(885,165), //26
                //27-30 nose竖
                new Vector2(770,220), //27
                new Vector2(770,250), //28
                new Vector2(770,275), //29
                new Vector2(770,300), //30
                //31-35 nose横
                new Vector2(740,312), //31
                new Vector2(755,316), //32
                new Vector2(770,320), //33
                new Vector2(785,316), //34
                new Vector2(800,312), //35
                //36-41 leftEye
                new Vector2(670,215), //36
                new Vector2(690,200), //37
                new Vector2(715,205), //38
                new Vector2(730,225), //39
                new Vector2(710,230), //40
                new Vector2(690,227), //41
                //42-47 rightEye
                new Vector2(810,225), //42
                new Vector2(825,205), //43
                new Vector2(850,200), //44
                new Vector2(870,215), //45
                new Vector2(855,227), //46
                new Vector2(830,230), //47
                //48-59 mouth
                new Vector2(720,360), //48-l
                new Vector2(735,355), //49
                new Vector2(750,350), //50
                new Vector2(770,352), //51+u
                new Vector2(790,350), //52
                new Vector2(805,355), //53
                new Vector2(820,360), //54-r
                new Vector2(805,375), //55
                new Vector2(790,382), //56
                new Vector2(770,380), //57+d
                new Vector2(750,382), //58
                new Vector2(735,375), //59
                //60-67 mouth2
                new Vector2(730,365), //60-l
                new Vector2(750,357), //61
                new Vector2(770,354), //62+u
                new Vector2(790,357), //63
                new Vector2(810,365), //64-r
                new Vector2(790,370), //65
                new Vector2(770,375), //66+d
                new Vector2(750,370), //67
            };
            landmarkPoints.Add(uvs);


            for (int i = 0; i < landmarkPoints[1].Count; i++)
            {
                Imgproc.putText(rgbaMat, i.ToString(), new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 1, 1, new Scalar(255, 0, 0, 255));
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
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //红
                }
                else if (i >= 17 && i <= 21)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //橙
                }
                else if (i >= 22 && i <= 26)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //黄
                }
                else if (i >= 27 && i <= 35)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //绿
                }
                else if (i >= 36 && i <= 21)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //青
                }
                else if (i >= 42 && i <= 47)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //蓝
                }
                else if (i >= 48 && i <= 59)
                {
                    Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //紫
                }
                else if (i >= 60 && i <= 67)
                {
                    //Imgproc.circle(rgbaMat, new Point(landmarkPoints[1][i].x, landmarkPoints[1][i].y), 0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_8, 0); //粉红
                }
                */
            }






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
                faceChanger.SetTargetImage(rgbaMat); //目标Mat
                faceChanger.AddFaceChangeData(rgbaMat, landmarkPoints[0], landmarkPoints[1], 1); //源Mat，从0拷贝到1
                faceChanger.ChangeFace();
                faceChanger.Dispose();
            }

            frontalFaceChecker.Dispose();

            //背景图
            Texture2D t2d = new Texture2D(rgbaMat.width(), rgbaMat.height(), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D(rgbaMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            srcImage.sprite = sp;
            srcImage.preserveAspect = true;

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
