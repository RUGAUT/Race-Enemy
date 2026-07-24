using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.vsoft.input.swipe;

namespace com.vsoft.input.swipe
{
    public class SwipeInputController : MonoBehaviour
    {
        Text Debug;

        // Start is called before the first frame update
        void Start()
        {
            Debug = GameObject.Find("Debug").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 pos = this.transform.position;

            SwipeInput.ProcessTouches();

            if (SwipeInput.Tap())
            {
                // Does nothing...
            }
            else if (SwipeInput.SwipeUp())
            {
                pos.z += 1;
                Debug.text = "Swipe Up";
            }
            else if (SwipeInput.SwipeDown())
            {
                pos.z -= 1;
                Debug.text = "Swipe Down";
            }
            else if (SwipeInput.SwipeLeft())
            {
                pos.x -= 1;
                Debug.text = "Swipe Left";
            }
            else if (SwipeInput.SwipeRight())
            {
                pos.x += 1;
                Debug.text = "Swipe Right";
            }

            this.transform.position = pos;
        }
    }
}
