/*
 *This code works with the wit-motion gyroscope and unity 2019.2.17f1
 * Specifically BTW901CL
 * It is built largely from the code here:http://dil33pm.in/reading-serial-data-in-unity-using-c/
 * the decoding function comes from the examples included from the wit-motion software
 * you probably still need to install drivers, though honestly I think that happened automatically in Windows 10 when I connected to H6 via bluetooth
 */


using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Threading;

public class SerialGyroController : MonoBehaviour
{

    public SerialPort sp;
    public Thread serialThread;
    public double x1, y1, z1;
    public bool readyToMove = false;
    //public float prevY = 0.0f;

    public string portName = "COM10";
    public Int32 Baund = 115200;

    public bool sign = false;
    public byte[] jBuff = new byte[11];
    public int counter = 0;

    double[] Angle = new double[4];

    void Start()
    {
        sp = new SerialPort("COM10", Baund);
        connect();
    }


    void recData()
    {        
        while ((sp != null) && (sp.IsOpen))
        {
            jBuff[counter] = (byte) sp.ReadByte();

                if (counter==0 && jBuff[0] != 0x55) { return; }
            counter++;
            if (counter == 11)
            {
                counter = 0;
                sign = true;
            }
            if (sign) { decode(); }
        }       
    }

    void decode()
    {
        

            sign = false;
            if (jBuff[0] == 0x55)
            {
                double[] Data = new double[4];
                //double TimeElapse = (Time.time - Time.timeSinceLevelLoad);
                //double TimeElapse = 0;
                Data[0] = BitConverter.ToInt16(jBuff, 2);
                Data[1] = BitConverter.ToInt16(jBuff, 4);
                Data[2] = BitConverter.ToInt16(jBuff, 6);
                Data[3] = BitConverter.ToInt16(jBuff, 8);
                switch (jBuff[1])
                {
                    case 0x53:
                        //Data[3] = Data[3] / 32768 * double.Parse(textBox9.Text) + double.Parse(textBox8.Text);
                       
                        Data[0] = Data[0] / 32768.0 * 180;
                        Data[1] = Data[1] / 32768.0 * 180;
                        Data[2] = Data[2] / 32768.0 * 180;
                        Angle[0] = Data[0];
                        Angle[1] = Data[1];
                        Angle[2] = Data[2];
                        Angle[3] = Data[3];
                        //if ((TimeElapse - LastTime[3]) < 0.1) return;
                        //LastTime[3] = TimeElapse;

                        x1 = Angle[0];
                        y1 = Angle[1];
                        z1 = Angle[2];
                        readyToMove = true;
                        break;
                }
            }
        
    }

    void rotateObject()
    {
        transform.rotation = Quaternion.Euler((float) x1,(float) z1,(float) y1);
        readyToMove = false;
    }


    void Update()
    {
        if (readyToMove)
        {
            rotateObject();
        }
        
    }

    void connect()
    {
        Debug.Log("Connection started");
        try
        {
            sp.Open();
            sp.ReadTimeout = 400;
            sp.Handshake = Handshake.None;
            serialThread = new Thread(recData);
            serialThread.Start();
            Debug.Log("Port Opened!");
        }
        catch (SystemException e)
        {
            Debug.Log("Error opening = " + e.Message);
        }

    }

}
