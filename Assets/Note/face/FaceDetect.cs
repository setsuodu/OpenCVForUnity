using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;
using OpenCVForUnityExample;

public class FaceDetect : MonoBehaviour
{
    [SerializeField] private Image m_dstImage;
    [SerializeField]  Texture2D texture;
    Color32[] colors;
    Mat rgbaMat;

    WebCamTextureToMatHelper webCamTextureToMatHelper;

    void Start()
    {
        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
        webCamTextureToMatHelper.Initialize();
    }

    void Update()
    {
        if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
        {
            rgbaMat = webCamTextureToMatHelper.GetMat();
            Utils.matToTexture2D(rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors());
        }
    }


    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log("On Initialized");

        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
        //Debug.Log(webCamTextureMat);

        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        Sprite sp = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero);
        m_dstImage.sprite = sp;
        m_dstImage.preserveAspect = true;
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
}
