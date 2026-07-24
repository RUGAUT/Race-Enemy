/**************************************************\
|* Script Author: Vishnu Sivan                    *|
|* Script Year: 2024                              *|
|* Script Version: 1.0.0                          *|
|* Script License: MIT License (MIT)              *|
\**************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace com.vsoft.input.swipe
{
    public class SwipeInput
    {
        // "Core" Variables.
        private static Vector2[] tapPositions = new Vector2[2];
        private static Vector2[] swipePositions = new Vector2[2];

        // "Offset" Variables.
        private static float offsetTap = 15.0F;
        private static float offsetSwipe = 40.0F;

        // "Flag" Variables.
        private static bool fTapAllowed = false;
        private static bool fSwipeAllowed = false;

        // "Other" Variables.
        private static float tempX = 0.0F;
        private static float tempY = 0.0F;

        // "Core" Methods.
        public static void ProcessTouches()
        {
            if (Input.touchCount > 0) // Check If User Is Touching The Screen.
            {
                Touch touch = Input.touches[0];

                if (touch.phase == TouchPhase.Began) // Begin Phase.
                {
                    SwipeInput.tapPositions[0] = touch.position;
                    SwipeInput.swipePositions[0] = touch.position;
                }
                else if (touch.phase == TouchPhase.Canceled) // Canceled Phase.
                {
                    SwipeInput.ResetPositions();
                }
                else if (touch.phase == TouchPhase.Ended) // Ended Phase.
                {
                    SwipeInput.tapPositions[1] = touch.position;
                    SwipeInput.swipePositions[1] = touch.position;

                    SwipeInput.fTapAllowed = true;
                    SwipeInput.fSwipeAllowed = true;
                }
                else if (touch.phase == TouchPhase.Moved) // Moved Phase.
                {
                    // NO CODE ATM FOR MOVED
                }
                else if (touch.phase == TouchPhase.Stationary) // Stationary Phase.
                {
                    // NO CODE ATM FOR STATIONARY
                }
            }
        }

        private static void ResetPositions()
        {
            SwipeInput.tapPositions = new Vector2[2];

            SwipeInput.fTapAllowed = false;
            SwipeInput.fSwipeAllowed = false;
        }

        // "Controls" Methods.

        public static bool Tap()
        {
            bool result = false;

            if (SwipeInput.fTapAllowed)
            {
                SwipeInput.tempX = Mathf.Abs(
                    SwipeInput.tapPositions[0].x - SwipeInput.tapPositions[1].x
                );
                SwipeInput.tempY = Mathf.Abs(
                    SwipeInput.tapPositions[0].y - SwipeInput.tapPositions[1].y
                );

                if (tempX <= SwipeInput.offsetTap && tempY <= SwipeInput.offsetTap)
                {
                    result = true;
                }

                SwipeInput.tapPositions = new Vector2[2];
                SwipeInput.fTapAllowed = false;
            }

            return result;
        }

        public static bool SwipeLeft()
        {
            bool result = false;

            if (SwipeInput.fSwipeAllowed)
            {
                SwipeInput.tempX = SwipeInput.swipePositions[0].x - SwipeInput.swipePositions[1].x;

                if (tempX >= SwipeInput.offsetSwipe)
                {
                    SwipeInput.swipePositions = new Vector2[2];
                    SwipeInput.fSwipeAllowed = false;
                    result = true;
                }
            }

            return result;
        }

        public static bool SwipeRight()
        {
            bool result = false;

            if (SwipeInput.fSwipeAllowed)
            {
                SwipeInput.tempX = SwipeInput.swipePositions[1].x - SwipeInput.swipePositions[0].x;

                if (tempX >= SwipeInput.offsetSwipe)
                {
                    SwipeInput.swipePositions = new Vector2[2];
                    SwipeInput.fSwipeAllowed = false;
                    result = true;
                }
            }

            return result;
        }

        public static bool SwipeUp()
        {
            bool result = false;

            if (SwipeInput.fSwipeAllowed)
            {
                SwipeInput.tempY = SwipeInput.swipePositions[1].y - SwipeInput.swipePositions[0].y;

                if (tempY >= SwipeInput.offsetSwipe)
                {
                    SwipeInput.swipePositions = new Vector2[2];
                    SwipeInput.fSwipeAllowed = false;
                    result = true;
                }
            }

            return result;
        }

        public static bool SwipeDown()
        {
            bool result = false;

            if (SwipeInput.fSwipeAllowed)
            {
                SwipeInput.tempY = SwipeInput.swipePositions[0].y - SwipeInput.swipePositions[1].y;

                if (tempY >= SwipeInput.offsetSwipe)
                {
                    SwipeInput.swipePositions = new Vector2[2];
                    SwipeInput.fSwipeAllowed = false;
                    result = true;
                }
            }

            return result;
        }
    }
}
