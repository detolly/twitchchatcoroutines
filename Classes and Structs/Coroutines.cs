using System;
using System.Collections;
using System.Collections.Generic;

namespace CoroutineSystem
{
    static class CoroutineManager
    {
        #region Declarations
        #region Variables
        #region Privates

        private static List<IEnumerator> currentCoroutines;
        private static List<IEnumerator> endlessCoroutines;
        private static List<IEnumerator> lateCoroutines;
        private static List<IEnumerator> toRemove;
        private static List<Tuple<IEnumerator, IEnumerator>> listOfAfter;

        #endregion
        #endregion
        #region Properties
        #region Private
        private static string exampleProperty
        {
            get; set;
        }
        #endregion
        #endregion
        #endregion

        #region Logic
        #region Public
        public static void Interval()
        {
            List<int> ints = new List<int>();
            foreach(var current in currentCoroutines)
            {
                if (current.Current is null)
                    if (!current.MoveNext()) toRemove.Add(current);
                if (current.Current is WaitForTime)
                    if (((WaitForTime)current.Current).finished)
                        if (!current.MoveNext())
                        {
                            Tuple<IEnumerator, IEnumerator> t1 = null;
                            foreach (Tuple<IEnumerator, IEnumerator> t in listOfAfter)
                            {
                                if (t.Item2 == current)
                                {
                                    StartCoroutine(t.Item1);
                                    t1 = t;
                                    break;
                                }
                            }
                            if (t1 != null) listOfAfter.Remove(t1);
                            toRemove.Add(current);
                        }
            }
            foreach (var current in endlessCoroutines)
            {
                if (current.Current is null)
                    if (!current.MoveNext()) toRemove.Add(current);
                if (current.Current is WaitForTime)
                    if (((WaitForTime)current.Current).finished)
                        if (!current.MoveNext()) current.Reset();
            }
            foreach(var c in toRemove)
                currentCoroutines.Remove(c);
            toRemove.Clear();
            if (lateCoroutines.Count > 0)
            {
                for (int i = 0; i < lateCoroutines.Count; i++)
                {
                    currentCoroutines.Add(lateCoroutines[i]);
                }
                lateCoroutines.Clear();
            }
        }

        public static void StartCoroutineAfterCoroutine(IEnumerator method, IEnumerator methodtoStartAfter)
        {
            listOfAfter.Add(new Tuple<IEnumerator, IEnumerator>(method, methodtoStartAfter));
        }

        public static void StopAllCoroutines()
        {
            currentCoroutines.Clear();
        }

        public static void StartCoroutine(IEnumerator method)
        {
            currentCoroutines.Add(method);
        }

        public static void StartLateCoroutine(IEnumerator method)
        {
            lateCoroutines.Add(method);
        }
        #endregion
        #endregion

        #region Init
        public static void Init()
        {
            currentCoroutines = new List<IEnumerator>();
            lateCoroutines = new List<IEnumerator>();
            endlessCoroutines = new List<IEnumerator>();
            toRemove = new List<IEnumerator>();
            listOfAfter = new List<Tuple<IEnumerator, IEnumerator>>();
        }
        #endregion

    }

    public abstract class WaitForTime
    {
        public bool finished
        {
            get
            {
                return origin < DateTime.Now - TimeSpan.FromMilliseconds(GetTimeInMiliseconds());
            }
        }

        public abstract int GetTimeInMiliseconds();

        private DateTime originTime;

        public DateTime origin
        {
            get
            {
                return originTime;
            }
            protected set
            {
                originTime = value;
            }
        }

        private int measureToWaitForIndeed;

        public int measureToWaitFor
        {
            get
            {
                return measureToWaitForIndeed;
            }
            protected set
            {
                measureToWaitForIndeed = value;
            }
        }
    }

    public class WaitForSeconds : WaitForTime
    {
        public WaitForSeconds(int secondsToWaitFor)
        {
            origin = DateTime.Now;
            measureToWaitFor = secondsToWaitFor;
        }

        public override int GetTimeInMiliseconds()
        {
            return measureToWaitFor * 1000;
        }
    }

    public class WaitForMilliseconds : WaitForTime
    {
        public WaitForMilliseconds(int millisecondsToWaitFor)
        {
            origin = DateTime.Now;
            measureToWaitFor = millisecondsToWaitFor;
        }

        public override int GetTimeInMiliseconds()
        {
            return measureToWaitFor;
        }
    }
}
