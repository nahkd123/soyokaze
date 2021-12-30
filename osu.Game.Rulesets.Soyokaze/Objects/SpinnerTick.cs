// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Soyokaze.Judgements;

namespace osu.Game.Rulesets.Soyokaze.Objects
{
    public class SpinnerTick : SoyokazeHitObject
    {
        public override Judgement CreateJudgement() => new SpinnerTickJudgement { IsBonus = IsBonus };
        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public bool IsBonus { get; set; } = false;

        public class SpinnerTickJudgement : SoyokazeJudgement
        {
            public override HitResult MaxResult => IsBonus ? HitResult.LargeBonus : HitResult.SmallBonus;
            public bool IsBonus { get; set; }
        }
    }
}
