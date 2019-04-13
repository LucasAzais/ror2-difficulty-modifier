﻿using BepInEx;
using RoR2;
using UnityEngine;
using System;
using MonoMod.Cil;

namespace DifficultyModifier
{
    [BepInPlugin("com.elzheiz.difficultymodifier", "DifficultyModifier", "1.0")]

    public class DifficultyModifier : BaseUnityPlugin
    {
        private int difficultyIncrementIndex = 0;
        private float[] difficultyIncrements = { 1.0f, 10.0f, 60.0f, 600.0f, 3600.0f };
        private float totalDifficultyIncrement = 0.0f;

        public void Awake()
        {
            IL.RoR2.Run.OnFixedUpdate += (il) =>
            {
                ILCursor c = new ILCursor(il).Goto(0);
                // Get to the next fixedTime load, go to the previous instruction (which should be "this")
                // Then add the new instruction and remove "this.fixedTime"
                while (c.Goto(0).TryGotoNext(x => x.MatchLdfld<Run>("fixedTime")))
                {
                    c.GotoPrev();
                    c.EmitDelegate<Func<float>>(() =>
                    {
                        if (!Run.instance) { return 0.0f; }

                        return Run.instance.fixedTime + totalDifficultyIncrement;
                    });
                    c.RemoveRange(2);
                }
            };

            // Reset the increment when the run is terminated.
            On.RoR2.Run.OnDestroy += (orig, self) =>
            {
                totalDifficultyIncrement = 0;
                orig(self);
            };
        }

        public void Update()
        {
            // Exit if we're not in a run.
            if (!Run.instance) { return; }

            // Otherwise control time using the + - * / buttons
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                /*// I -> Inspect the current difficuly increment and coefficients.
                if (Input.GetKeyDown(KeyCode.I))
                {
                    Debug.Log("compensatedDifficultyCoefficient = " + Run.instance.compensatedDifficultyCoefficient + "\n" +
                        "difficultyCoefficient = " + Run.instance.difficultyCoefficient + "\n" +
                        "targetMonsterLevel = " + Run.instance.targetMonsterLevel + "\n" +
                        "Total Difficulty Increment = " + totalDifficultyIncrement);
                }*/
                
                // + -> Increments the timer
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    totalDifficultyIncrement += difficultyIncrements[difficultyIncrementIndex];
                    Debug.Log("Slide difficulty bar by +" + difficultyIncrements[difficultyIncrementIndex] + "s (Additional difficulty is: " + totalDifficultyIncrement + "s)");
                }
                // - -> Decrements the timer as much as possible
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (Run.instance.fixedTime + totalDifficultyIncrement - difficultyIncrements[difficultyIncrementIndex] > 0)
                    {
                        totalDifficultyIncrement -= difficultyIncrements[difficultyIncrementIndex];
                        Debug.Log("Slide difficulty bar by -" + difficultyIncrements[difficultyIncrementIndex] + "s (Additional difficulty is: " + totalDifficultyIncrement + "s)");
                    }
                    else
                    {
                        totalDifficultyIncrement = -Run.instance.fixedTime;
                        Debug.Log("Slide difficulty bar by -" + Run.instance.fixedTime + "s (Additional difficulty is: " + totalDifficultyIncrement + "s)");
                    }
                }
                // * -> Increases the timer increment step
                else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
                {
                    if (difficultyIncrementIndex + 1 < difficultyIncrements.Length)
                    {
                        difficultyIncrementIndex++;
                        Debug.Log("Difficulty bar increment is now " + difficultyIncrements[difficultyIncrementIndex] + "s");
                    }
                }
                // / -> Decreases the timer increment step
                else if (Input.GetKeyDown(KeyCode.KeypadDivide))
                {
                    if (difficultyIncrementIndex - 1 >= 0)
                    {
                        difficultyIncrementIndex--;
                        Debug.Log("Difficulty bar increment is now " + difficultyIncrements[difficultyIncrementIndex] + "s");
                    }
                }
            }
        }
    }
}
