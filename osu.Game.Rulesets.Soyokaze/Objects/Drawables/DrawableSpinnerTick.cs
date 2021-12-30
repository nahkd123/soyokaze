// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Soyokaze.UI;

namespace osu.Game.Rulesets.Soyokaze.Objects.Drawables
{
    public class DrawableSpinnerTick : DrawableSoyokazeHitObject
    {
        public new SpinnerTick HitObject => base.HitObject as SpinnerTick;
        public DrawableSpinner Spinner => (DrawableSpinner)ParentHitObject;

        public DrawableSpinnerTick()
            : this(null)
        {
        }

        public DrawableSpinnerTick(SpinnerTick tick = null)
            : base(tick)
        {
        }

        protected override double MaximumJudgementOffset => Spinner.HitObject.Duration;

        public override bool Hit(SoyokazeAction action)
        {
            return true;
        }

        public void TriggerResult(bool hit) => ApplyResult(r => r.Type = hit ? r.Judgement.MaxResult : r.Judgement.MinResult);
    }
}
