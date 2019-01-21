using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/** 
 * For determining the tracking quality this function helps us record the camera position ofer a given period of time which
 * we determine by how many frames we wanna capture in the List. When we captured enought position values, one from each frame,
 * we write tham to a text file called posLox.txt this file will be stored directly on the phone under 
 * Android/data/com.___The_name_of_this_app___/files/posLog.txt The data can  than be plotted. I use Excel for that.
 * By doing this you can simply see when and possibly why frames drop and improve tracking further with those informations.
 */ 
public class RecordMovemenT : MonoBehaviour {

    public Transform aRCoreCamera;
    private Vector3 arCamPos;

    public Transform userCamera;
    private Vector3 userCamPos;

    public StreamWriter writer;

    public List<string> positionDataList = new List<string>(4000); // Enogth space for 4000 frames is reserved 

    private int counter = 0; // the counter is used to count the frames so we know when we have collected enought frames for our data analysis. 

 
	void LateUpdate () {

        // To avoide out ob bondery errors we are conservative and only fill 3990 frames
        if (counter < 3990)
        {
            userCamPos = userCamera.transform.position;
            positionDataList.Add("UserCamPos      " + DateTime.Now + "   " + userCamPos.sqrMagnitude);
            counter++;
        }
        
        /** we use 3900 of the frames and write them to the txt file. The writing process is computational more expensive than storing it 
         * in a list. That helps us avoide to reduce the load on the running application. 
         */
        if (counter == 3900)
        {
            for (int i = 0; i < positionDataList.Count; i++)
            {
                writer = new StreamWriter(Application.persistentDataPath + "/posLog.txt", true);
                writer.WriteLine(positionDataList[i]);
                writer.Close();
            }
        }
        
    }
}
