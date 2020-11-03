using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomWebRequest : DownloadHandlerScript
{
    int counter = 2; //header
    bool dataStart = false;
    byte prevByte = 0x00;

    public byte[] completeImageByte = new byte[500000];
    public bool newDataReceived = false;
    byte[] receivedBytes = new byte[500000];

    public Texture2D camTex = new Texture2D(2, 2);
    public RawImage target;

    // Standard scripted download handler - will allocate memory on each ReceiveData callback
    public CustomWebRequest()
        : base()
    {
    }

    // Pre-allocated scripted download handler
    // Will reuse the supplied byte array to deliver data.
    // Eliminates memory allocation.
    public CustomWebRequest(byte[] buffer)
        : base(buffer)
    {
    }

    // Required by DownloadHandler base class. Called when you address the 'bytes' property.
    protected override byte[] GetData() { return null; }

    // Called once per frame when data has been received from the network.
    //static byte[] jpgHeader = { 0xFF, 0xD8 };
    //static byte[] jpgFooter = { 0xFF, 0xD9 };
    protected override bool ReceiveData(byte[] byteFromCamera, int dataLength)
    {
        if (byteFromCamera == null || byteFromCamera.Length < 1)
        {
            //Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
            return false;
        }

        //Search of JPEG Image here
        foreach (byte b in byteFromCamera)
        {
            if (dataStart)
            {
                receivedBytes[counter] = b;
                if (prevByte == 0xFF && b == 0xD9)
                {
                    System.Buffer.BlockCopy(receivedBytes, 0, completeImageByte, 0, counter+1);
                    Debug.Log("Img ended with " + completeImageByte[counter - 1].ToString() + completeImageByte[counter].ToString());

                    dataStart = false;
                    counter = 1;

                    camTex.LoadImage(completeImageByte);
                    target.texture = camTex;
                    //break;
                }
                prevByte = b;
                counter++;
            }
            else
            {
                if (prevByte == 0xFF && b == 0xD8)
                {
                    receivedBytes[0] = prevByte;
                    receivedBytes[1] = b;
                    dataStart = true;

                    Debug.Log("Img started" + receivedBytes[0].ToString() + receivedBytes[1].ToString());
                }
                else prevByte = b;
            }
        }
        return true;
    }

    // Called when all data has been received from the server and delivered via ReceiveData
    protected override void CompleteContent()
    {
        //Debug.Log("CustomWebRequest :: CompleteContent - DOWNLOAD COMPLETE!");
    }

    // Called when a Content-Length header is received from the server.
    protected override void ReceiveContentLength(int contentLength)
    {
        //Debug.Log(string.Format("CustomWebRequest :: ReceiveContentLength - length {0}", contentLength));
    }
}