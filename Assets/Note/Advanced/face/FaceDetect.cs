using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;
using OpenCVForUnityExample;
using DlibFaceLandmarkDetector;

public class FaceDetect : MonoBehaviour
{
    [SerializeField] private Image m_srcImage;
    [SerializeField] private Image m_webcamImage;
    [SerializeField]  Texture2D texture;
    Color32[] colors;
    Mat rgbaMat;
    string sp_human_face_68_dat_filepath;

    WebCamTextureToMatHelper webCamTextureToMatHelper;

    void Start()
    {
        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
        webCamTextureToMatHelper.Initialize();

        Run();
    }

    void Update()
    {
        if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
        {
            rgbaMat = webCamTextureToMatHelper.GetMat();
            OpenCVForUnity.Utils.matToTexture2D(rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors());
        }
    }

    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log("On Initialized");

        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
        //Debug.Log(webCamTextureMat);

        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        Sprite sp = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero);
        m_webcamImage.sprite = sp;
        m_webcamImage.preserveAspect = true;
    }

    public void OnWebCamTextureToMatHelperDisposed()
    {
        Debug.Log("On Disposed");

        rgbaMat.Dispose();
    }

    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("On Error Occurred " + errorCode);
    }

    void Run()
    {
        //人脸68关键点定位
        sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath("sp_human_face_68.dat");
        FaceLandmarkDetector faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath); 

        Mat imgMat = Imgcodecs.imread(Application.dataPath + "/Textures/face.jpg");
        Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_BGR2RGB);
        FaceSwapperExample.OpenCVForUnityUtils.SetImage(faceLandmarkDetector, imgMat);

        //detect face rectdetecton
        List<FaceLandmarkDetector.RectDetection> detectResult = faceLandmarkDetector.DetectRectDetection();

        foreach (var result in detectResult)
        {
            Debug.Log("rect : " + result.rect);
            Debug.Log("detection_confidence : " + result.detection_confidence);
            Debug.Log("weight_index : " + result.weight_index);
            //Debug.Log ("face : " + rect);

            Imgproc.putText(imgMat, "" + result.detection_confidence, new Point(result.rect.xMin, result.rect.yMin - 20), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
            Imgproc.putText(imgMat, "" + result.weight_index, new Point(result.rect.xMin, result.rect.yMin - 5), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

            //detect landmark points
            List<Vector2> points = faceLandmarkDetector.DetectLandmark(result.rect);

            Debug.Log("face points count : " + points.Count);
            //draw landmark points
            FaceSwapperExample.OpenCVForUnityUtils.DrawFaceLandmark(imgMat, points, new Scalar(0, 255, 0, 255), 1); //线，不是点

            //draw face rect
            FaceSwapperExample.OpenCVForUnityUtils.DrawFaceRect(imgMat, result.rect, new Scalar(255, 0, 0, 255), 1);
        }
        faceLandmarkDetector.Dispose();

        Texture2D t2d = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.Utils.matToTexture2D(imgMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_srcImage.sprite = sp;
        m_srcImage.preserveAspect = true;
    }
}
