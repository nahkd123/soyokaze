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
using osu.Game.Rulesets.Soyokaze.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Soyokaze.Objects.Drawables
{
    public class DrawableSpinner : DrawableSoyokazeHitObject
    {
        public new Spinner HitObject => base.HitObject as Spinner;

        public int Hits { get; private set; } = 0;

        private Drawable spinnerCircle;
        private Drawable spinnerProgress;

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
            AddInternal(spinnerCircle = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 1000f,
                Height = 1000f,
                Alpha = 0f,
                Children = new[]
                {
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4Extensions.FromHex("#5B5B5B30"),
                        Width = 1000f,
                        Height = 1000f,
                    },
                    spinnerProgress = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4Extensions.FromHex("#0075FF40"),
                        Width = 1000f,
                        Height = 1000f,
                    }
                }
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

            spinnerCircle.ScaleTo(0.5f, 0);
            spinnerProgress.ScaleTo(0f, 0);

            spinnerCircle.FadeInFromZero(System.Math.Min(HitObject.FadeIn * 2, HitObject.Preempt / 2));
            using (BeginDelayedSequence(HitObject.Preempt / 2))
            {
                spinnerCircle.ScaleTo(1.0f, HitObject.Preempt / 2, Easing.OutCubic);
            }
        }

        private void updateProgress(float progress)
        {
            double scaleTime;
            if (progress > 1)
            {
                scaleTime = 150;
                spinnerProgress.FadeColour(Color4Extensions.FromHex("#FFE86F7F"), 30);
                using (spinnerProgress.BeginDelayedSequence(30))
                {
                    spinnerProgress.FadeColour(Color4Extensions.FromHex("#FFB84D6D"), 100);
                }
            }
            else scaleTime = 350;
            spinnerProgress.ScaleTo(System.Math.Min(progress, 1f), scaleTime, Easing.Out);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    spinnerCircle.ScaleTo(2.0f, 350, Easing.OutCubic);
                    spinnerCircle.FadeOutFromOne(350);
                    this.MoveToOffset(Vector2.Zero, 350).Expire();
                    break;
                case ArmedState.Miss:
                    spinnerCircle.FadeOutFromOne(150);
                    this.MoveToOffset(Vector2.Zero, 150).Expire();
                    break;
            }
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
                updateProgress(progress);
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
        private int repeatingHitsCount = 0;

        public override bool Hit(SoyokazeAction action)
        {
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime) return false;

            // Player have to hit alternating between DPAD-L and DPAD-R
            bool isLeftSide = (int)action < 4;
            if (previousIsLeftSide == isLeftSide)
            {
                if (repeatingHitsCount > 2) return false;
                repeatingHitsCount++;
            }
            else
            {
                repeatingHitsCount = 0;
                previousIsLeftSide = isLeftSide;
            }

            UpdateResult(true);
            return true;
        }
    }
}
