﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Leap;

namespace LeapHelper
{
    public class LeapListener:Listener
    {
        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            //lock (thisLock)
            //{
                Debug.WriteLine(line);
            //}

        }


        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            //controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Leap.Frame frame = controller.Frame();

            if (!frame.Hands.Empty)
            {
                if (frame.Fingers.Count>=2&&LeapFingerReady!=null)
                {
                        LeapFingerReady(this,frame.Fingers[0],frame.Fingers[1]);
                }

            }

            // Get gestures
            GestureList gestures = frame.Gestures();
            for (int i = 0; i < gestures.Count; i++)
            {
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPECIRCLE:
                        CircleGesture circle = new CircleGesture(gesture);

                        // Calculate clock direction using the angle between circle normal and pointable
                        if (circle.State == Gesture.GestureState.STATESTART)
                        {
                            Thread.Sleep(500);
                            LeapCircleReady(this);
                        }
                       
                        break;
                    case Gesture.GestureType.TYPESWIPE:
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        
                        if (swipe.State == Gesture.GestureState.STATESTART&&LeapSwipeReady!=null)
                        {
                            if (swipe.Direction.x < -0.5)
                            {
                                LeapSwipeReady(this, SwipeType.SwipeLeft);                               
                            }
                            else if (swipe.Direction.x > 0.5)
                            {
                                LeapSwipeReady(this, SwipeType.SwipeRight);
                            }
                            //else if (swipe.Direction.z < -0.2)
                            //{
                            //    LeapSwipeReady(this, SwipeType.SwpieIn);
                            //}
                            //else if (swipe.Direction.z > 0.2)
                            //{
                            //    Thread.Sleep(300);
                            //    LeapSwipeReady(this, SwipeType.SwipeOut);
                            //}
                            

                        }
                        break;
                    case Gesture.GestureType.TYPEKEYTAP:
                        KeyTapGesture keytap = new KeyTapGesture(gesture);
                        SafeWriteLine("Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction);

                        break;
                    case Gesture.GestureType.TYPESCREENTAP:
                        ScreenTapGesture screentap = new ScreenTapGesture(gesture);
                        if ( screentap.State==Gesture.GestureState.STATESTOP&&LeapTapScreenReady!=null)
                        {
                            if(frame.Fingers.Count>1)
                                Thread.Sleep(800);
                            LeapTapScreenReady(this);
                        }
                        break;
                }
            }


        }

        public delegate void SwipeEventHandler(object sender, SwipeType type);

        public event SwipeEventHandler LeapSwipeReady;

        public delegate void FingerEventHandler(object sender,Finger first,Finger second);

        public event FingerEventHandler LeapFingerReady;

        public delegate void CircleEventHandler(object sender);

        public event CircleEventHandler LeapCircleReady;

        public delegate void TypeScreenEventHandler(object sender);

        public event TypeScreenEventHandler LeapTapScreenReady;
    }
}
