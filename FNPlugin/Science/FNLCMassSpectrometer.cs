﻿using UnityEngine;
using FNPlugin.Resources;
using KSP.Localization;

namespace FNPlugin
{
    class FNLCMassSpectrometer : PartModule
    {
        protected Rect windowPosition = new Rect(20, 20, 300, 100);
        protected int windowID = 9875875;
        protected bool render_window;
        protected GUIStyle bold_label;
        protected int analysis_count;
        protected static int analysis_length = 1500;

        [KSPEvent(guiActive = true, guiName = "#LOC_KSPIE_MassSpectrometer_ShowWindow", active = true)]//Show Spectrometry Results
        public void ShowWindow()
        {
            render_window = true;
        }

        [KSPEvent(guiActive = true, guiName = "#LOC_KSPIE_MassSpectrometer_HideWindow", active = true)]//Hide Spectrometry Results
        public void HideWindow()
        {
            render_window = false;
        }

        public override void OnStart(StartState state)
        {
        }

        public override void OnUpdate()
        {
            Events[nameof(ShowWindow)].active = !render_window;
            Events[nameof(HideWindow)].active = render_window;
        }

        private void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && render_window)
            {
                windowPosition = GUILayout.Window(windowID, windowPosition, Window, Localizer.Format("#LOC_KSPIE_FNLCMassSpectrometer_Windowtitle"));//"LC/MS - Ocean Composition"
                if (analysis_count <= analysis_length)
                    analysis_count++;
            }
        }

        private void Window(int windowId)
        {
            bold_label = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};

            if (GUI.Button(new Rect(windowPosition.width - 20, 2, 18, 18), "x"))
                render_window = false;

            GUILayout.BeginVertical();
            if (vessel.Splashed)
            {
                if (analysis_count > analysis_length)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localizer.Format("#LOC_KSPIE_FNLCMassSpectrometer_Liquid"), bold_label, GUILayout.Width(150));//"Liquid"
                    GUILayout.Label(Localizer.Format("#LOC_KSPIE_MassSpectrometer_Abundance"), bold_label, GUILayout.Width(150));//"Abundance"
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);

                    foreach (OceanicResource oceanicResource in OceanicResourceHandler.GetOceanicCompositionForBody(vessel.mainBody.flightGlobalsIndex))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(oceanicResource.DisplayName, GUILayout.Width(150));
                        string resourceAbundanceStr;
                        if (oceanicResource.ResourceAbundance > 0.001)
                            resourceAbundanceStr = (oceanicResource.ResourceAbundance * 100.0) + "%";
                        else
                        {
                            if (oceanicResource.ResourceAbundance > 0.000001)
                                resourceAbundanceStr = (oceanicResource.ResourceAbundance * 1e6) + " ppm";
                            else
                                resourceAbundanceStr = (oceanicResource.ResourceAbundance * 1e9) + " ppb";
                        }
                        GUILayout.Label(resourceAbundanceStr, GUILayout.Width(150));
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    double percentAnalysed = (double)analysis_count / analysis_length * 100;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localizer.Format("#LOC_KSPIE_MassSpectrometer_Analysing"), GUILayout.Width(150));//"Analysing..."
                    GUILayout.Label(percentAnalysed.ToString("0.00") + "%", GUILayout.Width(150));
                    GUILayout.EndHorizontal();
                }

            }
            else
            {
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_FNLCMassSpectrometer_NoOceantoSample"), GUILayout.ExpandWidth(true));//"--No Ocean to Sample--"
                analysis_count = 0;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
