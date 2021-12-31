// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Soyokaze.Objects.Drawables;
using osu.Game.Rulesets.Soyokaze.Skinning.Defaults;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Soyokaze.Skinning
{
    public class SkinnableSpinner : Container
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; }

        private Colour4 normalColor;
        private Colour4 bonusColorLight;
        private Colour4 bonusColor;

        private SkinnableDrawable spinnerDisc;
        private SkinnableDrawable spinnerFill;
        private SkinnableDrawable spinnerOverlay;

        public SkinnableSpinner()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            spinnerDisc = new SkinnableDrawable(
                new SoyokazeSkinComponent(SoyokazeSkinComponents.SpinnerDisc),
                _ => new DefaultSpinnerDisc()
            );
            spinnerFill = new SkinnableDrawable(
                new SoyokazeSkinComponent(SoyokazeSkinComponents.SpinnerFill),
                _ => new DefaultSpinnerFill()
            );
            spinnerOverlay = new SkinnableDrawable(
                new SoyokazeSkinComponent(SoyokazeSkinComponents.SpinnerOverlay),
                _ => new DefaultSpinnerOverlay()
            );

            normalColor = Color4Extensions.FromHex("#0075FF");
            bonusColorLight = Color4Extensions.FromHex("#FFF6C4");
            bonusColor = Color4Extensions.FromHex("#FFB84D");
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new[]
            {
                spinnerDisc,
                spinnerFill,
                spinnerOverlay,
            };

            spinnerFill.Colour = normalColor;
            spinnerFill.Anchor = spinnerDisc.Anchor = spinnerOverlay.Anchor = Anchor.Centre;
            spinnerFill.Origin = spinnerDisc.Origin = spinnerOverlay.Origin = Anchor.Centre;
        }

        public void UpdateProgress(float progress, bool instant = false)
        {
            const int normal_animation_duration = 350;
            const int normal_color_transition = 100;
            const int bonus_animation_duration = 150;
            const int bonus_flash_duration = 30;
            const int hit_spin_duration = 450;
            const int hit_spin_angle = 60;

            var hits = ((DrawableSpinner)drawableObject).HitObject.HitsRequired * progress;
            if (instant)
            {
                spinnerFill.Colour = bonusColor;
                spinnerFill.Scale = new Vector2(System.Math.Min(progress, 1f));
                Rotation = hits * hit_spin_angle;
            }
            else
            {
                double scaleTime;
                if (progress > 1)
                {
                    scaleTime = bonus_animation_duration;
                    spinnerFill.FadeColour(bonusColorLight, bonus_flash_duration);
                    using (spinnerFill.BeginDelayedSequence(bonus_flash_duration))
                    {
                        spinnerFill.FadeColour(bonusColor, bonus_animation_duration - bonus_flash_duration);
                    }
                }
                else
                {
                    scaleTime = normal_animation_duration;
                    spinnerFill.FadeColour(normalColor, normal_color_transition);
                }
                spinnerFill.ScaleTo(System.Math.Min(progress, 1f), scaleTime, Easing.Out);

                var rotateAngle = hits * hit_spin_angle;
                spinnerDisc.RotateTo(rotateAngle, hit_spin_duration, Easing.OutQuint);
                spinnerFill.RotateTo(rotateAngle, hit_spin_duration, Easing.OutQuint);
            }
        }
    }
}
