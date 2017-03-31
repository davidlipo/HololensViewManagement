using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class TargetScriptTest {
	[Test]
    public void GetLabelMessage()
    {
        TargetScript target = new TargetScript();
        string text = "Test";
        target.labelMessage = text;

        Assert.AreEqual(target.getLabelMessage(), text);
    }

    [Test]
    public void GetPriority()
    {
        TargetScript target = new TargetScript();
        int priority = 2;
        target.priority = priority;

        Assert.AreEqual(target.getPriority(), priority);
    }

    [Test]
    public void GetScreenBounds()
    {
        Renderer ren = new Renderer();
        Camera cam = NSubstitute.Substitute.For<Camera>();
        cam.WorldToScreenPoint().Returns(new Vector2(1, 2));
        Rect value = TargetScript.GetScreenBounds(ren, cam);
        Rect expectedValue = new Rect(1, 2, 0, 0);

        Assert.AreEqual(value, expectedValue);
    }
}
