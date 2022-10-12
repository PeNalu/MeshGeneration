using ApexInspector;
using UnityEngine;

public class ButtonsExample : MonoBehaviour
{
    [InlineButton("ValueInlineFunction", Label = "Inline Button")]
    public float a;

    [InlineButton("ValueInlineFunctionWithStyle", Label = "@_Popup", Style = "IconButton")]
    public float b;

    [BottomButton("BottomFunction")]
    public float c;

    [BottomButton("GroupedBottomFunction1", Group = "SomeGroup", Style = "ButtonLeft")]
    [BottomButton("GroupedBottomFunction2", Group = "SomeGroup", Style = "ButtonRight")]
    public float d;

    [Button]
    public void Function1()
    {
        Debug.Log("Called Function1()");
    }

    [Button]
    public void Function2()
    {
        Debug.Log("Called Function2()");
    }

    [Button]
    [ButtonHorizontalGroup("GroupedFunctions")]
    public void GroupedFunction1()
    {
        Debug.Log("Called GroupedFunction1()");
    }

    [Button]
    [ButtonHorizontalGroup("GroupedFunctions")]
    public void GroupedFunction2()
    {
        Debug.Log("Called GroupedFunction2()");
    }

    public void ValueInlineFunction()
    {
        Debug.Log("Called Value [a] Inline Function");
    }

    public void ValueInlineFunctionWithStyle()
    {
        Debug.Log("Called Value [b] Inline Function With Style");
    }

    public void BottomFunction()
    {
        Debug.Log("Called BottomFunction()");
    }

    public void GroupedBottomFunction1()
    {
        Debug.Log("Called GroupedBottomFunction1()");
    }

    public void GroupedBottomFunction2()
    {
        Debug.Log("Called GroupedBottomFunction2()");
    }
}
