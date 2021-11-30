using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationBonus : Bonus
{

    private void Start()
    {
        effect = BonusModel.Effect.Acceleration;
        targets = BonusModel.Targets.Picker;
    }





}
