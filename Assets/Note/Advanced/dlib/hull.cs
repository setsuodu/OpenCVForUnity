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
        CascadeClassifier cascade;
        FaceLandmarkDetector faceLandmarkDetector;
        string haarcascade_frontalface_alt_xml_filepath;
        string sp_human_face_68_dat_filepath;

        [SerializeField] private Image srcImage;
        [SerializeField] private Image dstImage;
        Mat rgbaMat, dstMat;
        List<Point> pointList;

        void Start()
        {
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath("sp_human_face_68.dat");
            faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath);

            Run();
        }

        void Run()
        {
            rgbaMat = Imgcodecs.imread(Application.dataPath + "/Resources/aragaki.jpg", 1);
            Imgproc.cvtColor(rgbaMat, rgbaMat, Imgproc.COLOR_BGR2RGBA);
            dstMat = Imgcodecs.imread(Application.dataPath + "/Resources/lena.jpg", 1);
            Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGBA);

            //1. 人脸dlib检测
            List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect>();
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
            List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();
            foreach (UnityEngine.Rect unityRect in result)
            {
                detectResult.Add(new OpenCVForUnity.Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
            }
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
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
                //Imgproc.circle(rgbaMat, new Point(points[i].x, points[i].y), 2, new Scalar(255, 255, 255), -1);
                Point pt = new Point(landmarkPoints[0][i].x, landmarkPoints[0][i].y);
                Imgproc.circle(rgbaMat, pt, 0, new Scalar(0, 255, 0, 255), 2, Imgproc.LINE_8, 0); //绘制68点
                pointList.Add(pt);
            }
            //Debug.Log(pointList.Count);

            //3. 三角剖份
            TriangleDivide();

            //4. （遍历三角形）仿射变换
            Affine();

            //5. 显示
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
            Imgproc.drawContours(rgbaMat, hullPoints, -1, new Scalar(255, 255, 0, 255), 2);

            Texture2D t2d = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D(rgbaMat, t2d);
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            srcImage.sprite = sp;
            srcImage.preserveAspect = true;

            Texture2D dst_t2d = new Texture2D(dstMat.cols(), dstMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D(dstMat, dst_t2d);
            Sprite dst_sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
            dstImage.sprite = dst_sp;
            dstImage.preserveAspect = true;
        }

        void TriangleDivide()
        {
            //init subdiv
            Subdiv2D subdiv = new Subdiv2D();
            subdiv.initDelaunay(new OpenCVForUnity.Rect(0, 0, rgbaMat.width(), rgbaMat.height()));
            for (int i = 0; i < pointList.Count; i++)
            {
                subdiv.insert(pointList[i]);
            }
            subdiv.insert(new Point(0, 0));
            subdiv.insert(new Point(rgbaMat.width() / 2 - 1, 0));
            subdiv.insert(new Point(rgbaMat.width() - 1, 0));
            subdiv.insert(new Point(rgbaMat.width() - 1, rgbaMat.height() / 2 - 1));
            subdiv.insert(new Point(rgbaMat.width() - 1, rgbaMat.height() - 1));
            subdiv.insert(new Point(rgbaMat.width() / 2 - 1, rgbaMat.height() - 1));
            subdiv.insert(new Point(0, rgbaMat.height() - 1));
            subdiv.insert(new Point(0, rgbaMat.height() / 2 - 1));

            using (MatOfFloat6 triangleList = new MatOfFloat6())
            {
                subdiv.getTriangleList(triangleList);
                float[] pointArray = triangleList.toArray();
                //Debug.Log(pointArray.Length); //11000+
                float downScaleRatio = 1;

                byte[] color = new byte[4];
                for (int i = 0; i < pointArray.Length / 6; i++)
                {
                    Point p0 = new Point(pointArray[i * 6 + 0] * downScaleRatio, pointArray[i * 6 + 1] * downScaleRatio);
                    Point p1 = new Point(pointArray[i * 6 + 2] * downScaleRatio, pointArray[i * 6 + 3] * downScaleRatio);
                    Point p2 = new Point(pointArray[i * 6 + 4] * downScaleRatio, pointArray[i * 6 + 5] * downScaleRatio);

                    if (p0.x < 0 || p0.x > rgbaMat.width())
                        continue;
                    if (p0.y < 0 || p0.y > rgbaMat.height())
                        continue;
                    if (p1.x < 0 || p1.x > rgbaMat.width())
                        continue;
                    if (p1.y < 0 || p1.y > rgbaMat.height())
                        continue;
                    if (p2.x < 0 || p2.x > rgbaMat.width())
                        continue;
                    if (p2.y < 0 || p2.y > rgbaMat.height())
                        continue;

                    //get center of gravity
                    int cx = (int)((p0.x + p1.x + p2.x) * 0.33333);
                    int cy = (int)((p0.y + p1.y + p2.y) * 0.33333);
                    //Debug.Log ("cx " + cx + " cy " + cy );

                    //get center of gravity color
                    rgbaMat.get(cy, cx, color);
                    //Debug.Log ("r " + color[0] + " g " + color[1] + " b " + color[2] + " a " + color[3]);

                    //fill Polygon
                    //Imgproc.fillConvexPoly(rgbaMat, new MatOfPoint(p0, p1, p2), new Scalar(color[0], color[1], color[2], color[3]), Imgproc.LINE_AA, 0);
                    Imgproc.line(rgbaMat, p0, p1, new Scalar(64, 255, 128, 255));
                    Imgproc.line(rgbaMat, p1, p2, new Scalar(64, 255, 128, 255));
                    Imgproc.line(rgbaMat, p2, p0, new Scalar(64, 255, 128, 255));
                }
            }
        }

        void Affine()
        {

        }
    }
}
