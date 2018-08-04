using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public static class CurrentAnimations
    {
        public static List<Image> CurrentlyAnimated { get; private set; } = new List<Image>();
        public static List<Control> RegisteredControls { get; private set; } = new List<Control>();
    }

    public static class Animations
    {
        public static List<Animation> animations;

        static Animations()
        {
            animations = new List<Animation>();
        }

        public static Animation GetAnimation(int index)
        {
            return animations[index];
        }

        public static void AddAnimation(Animation a)
        {
            animations.Add(a);
        }

        public static void RemoveAnimation(Animation a)
        {
            animations.Remove(a);
        }
    }

    public class Animation
    {
        private int[] array;
        private List<int> upTillNow;

        public int StepCount
        {
            get { return array.Length; }
        }

        public Animation()
        {
            upTillNow = new List<int>();
            Animations.AddAnimation(this);
        }

        ~Animation()
        {
            Animations.RemoveAnimation(this);
        }

        public void AddStep(int step)
        {
            upTillNow.Add(step);
        }

        public int Finish()
        {
            array = upTillNow.ToArray();
            return array.Length;
        }

        public int GetNext(int currentIndex)
        {
            if (currentIndex+1 > array.Length-1)
            {
                throw new Exception("Animation step not in bounds of the array");
            }
            return array[currentIndex + 1];
        }

    }
}
