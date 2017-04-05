using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class LabelPositionerTest
{
    [Test]
    public void SetTargetPosition()
    {
        LabelPositioner label = new LabelPositioner();
        label.transform.position = new Vector3(5, 6, 7);
        Vector3 pos = new Vector3(1, 2, 3);
        label.setTargetPosition(pos);

        Assert.AreEqual(label.targetPosition, text);
    }
}
