using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezerBonus : Bonus
{

    void Start()
    {

        effect = BonusModel.Effect.Freezer;
        targets = BonusModel.Targets.EveryoneExpectPicker;

    }

}
