// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Soyokaze.Skinning;
using osu.Game.Rulesets.Soyokaze.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Soyokaze.Objects.Drawables
{
    public class DrawableSpinner : DrawableSoyokazeHitObject
    {
        public new Spinner HitObject => base.HitObject as Spinner;

        public int Hits { get; private set; } = 0;

        public SkinnableSpinner Spinner;
        private Container<DrawableSpinnerTick> ticks;

        public DrawableSpinner()
            : this(null)
        {
        }

        public DrawableSpinner(Spinner spinner = null)
            : base(spinner)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(Spinner = new SkinnableSpinner
            {
                Alpha = 1f,
            });
            AddInternal(ticks = new Container<DrawableSpinnerTick>());
        }

        protected override void UpdatePosition()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Spinner.ScaleTo(0.5f, 0);
            Spinner.FadeInFromZero(System.Math.Min(HitObject.FadeIn * 2, HitObject.Preempt / 2));
            Spinner.UpdateProgress(0f, true);
            this.ScaleTo(2f, 0);

            using (BeginDelayedSequence(HitObject.Preempt / 2))
            {
                Spinner.ScaleTo(1.0f, HitObject.Preempt / 2, Easing.OutCubic);
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            int numHits = ticks.Count(v => v.IsHit);
            float progress = (float)numHits / HitObject.HitsRequired;

            switch (state)
            {
                case ArmedState.Hit:
                    Spinner.ScaleTo(1.4f, 350, Easing.OutCubic);
                    Spinner.FadeOutFromOne(350);
                    this.RotateTo(60f, 350, Easing.OutQuint);
                    this.MoveToOffset(Vector2.Zero, 350).Expire();
                    break;
                case ArmedState.Miss:
                    Spinner.FadeOutFromOne(150);
                    this.MoveToOffset(Vector2.Zero, 150).Expire();
                    break;
            }

            Spinner.UpdateProgress(progress, true);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            switch (hitObject)
            {
                case DrawableSpinnerTick tick:
                    ticks.Add(tick);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            ticks.Clear(false);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
            {
                DrawableSpinnerTick tick = null;
                foreach (var t in ticks)
                    if (!t.Result.HasResult)
                    {
                        tick = t;
                        break;
                    }

                if (tick == null) return;
                tick.TriggerResult(true);

                int numHits = ticks.Count(v => v.IsHit);
                float progress = (float)numHits / HitObject.HitsRequired;
                Spinner.UpdateProgress(progress);
            }
            else
            {
                if (timeOffset < 0) return;

                int numHits = 0;
                foreach (var tick in ticks)
                {
                    if (tick.IsHit)
                        numHits++;
                    else if (!tick.Result.HasResult)
                        tick.TriggerResult(false);
                }

                ApplyResult(
                    r => r.Type =
                        numHits >= HitObject.HitsRequired ? HitResult.Perfect :
                        numHits >= HitObject.HitsRequired / 2 ? HitResult.Ok :
                        r.Judgement.MinResult
                );
            }
        }

        private bool? previousIsLeftSide;

        public override bool Hit(SoyokazeAction action)
        {
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime) return false;

            // Player have to hit alternating between DPAD-L and DPAD-R
            bool isLeftSide = (int)action < 4;
            if (previousIsLeftSide == isLeftSide) return false;
            previousIsLeftSide = isLeftSide;

            UpdateResult(true);
            return true;
        }
    }
}
