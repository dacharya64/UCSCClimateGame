using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowScript2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Bounce(this.transform);
    }

    void Bounce(Transform alert)
    {
        DOTween.Sequence()
            .Append(alert.DOMoveX(alert.position.x - 10f, 0.3f))
            .Append(alert.DOMoveX(alert.position.x + 10f, 0.3f))
            .SetLoops(-1, LoopType.Restart);
    }
}
