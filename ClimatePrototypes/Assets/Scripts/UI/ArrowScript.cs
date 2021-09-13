using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Bounce(this.transform);//.parent.transform);
    }

    void Bounce(Transform alert)
    {
        DOTween.Sequence()
            .Append(alert.DOMoveY(alert.position.y + 10f, 0.3f))
            .Append(alert.DOMoveY(alert.position.y - 10f, 0.3f))
            .SetLoops(-1, LoopType.Restart);
    }
}
