using System;
using UnityEngine;
using UnityEngine.Playables;

public class CTimeLineNGUIColorPlayable : PlayableBehaviour
{
    private Color       m_ColorDefault;
    private UIWidget    m_widget;
    private bool        m_bFirstFrameFlag = false;


    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_widget = playerData as UIWidget;

        if(m_widget == null)
            return;

        if(!m_bFirstFrameFlag)
        {
            m_ColorDefault = m_widget.color;
            m_bFirstFrameFlag = true;
        }

        int inputCount = playable.GetInputCount ();

        Color blendedColor = Color.clear;
        float totalWeight = 0f;
        float greatestWeight = 0f;
        int currentInputs = 0;

        for(int i = 0 ; i < inputCount ; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<ScreenFaderBehaviour> inputPlayable = (ScriptPlayable<ScreenFaderBehaviour>)playable.GetInput(i);
            ScreenFaderBehaviour input = inputPlayable.GetBehaviour ();

            blendedColor += input.color * inputWeight;
            totalWeight += inputWeight;

            if(inputWeight > greatestWeight)
            {
                greatestWeight = inputWeight;
            }

            if(!Mathf.Approximately(inputWeight, 0f))
                currentInputs++;
        }

        m_widget.color = blendedColor + m_ColorDefault * (1f - totalWeight);
    }

    public override void OnGraphStop(Playable playable)
    {
        //m_widget.color = m_ColorDefault;
        m_bFirstFrameFlag = false;
    }
}
