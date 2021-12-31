// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Soyokaze.Objects
{
    public class Spinner : SoyokazeHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        public int HitsRequired { get; protected set; } = 1;

        public int MaximumBonusHits { get; protected set; } = 1;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            var seconds = Duration / 1000;
            HitsRequired = (int)(seconds * 12);
            MaximumBonusHits = (int)(seconds * 24) - HitsRequired;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);
            int totalHits = MaximumBonusHits + HitsRequired;
            for (int i = 0; i < totalHits; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double tickStartTime = StartTime + (float)(i + 1) / totalHits * Duration;
                var tick = new SpinnerTick
                {
                    StartTime = tickStartTime,
                    IsBonus = i >= HitsRequired,
                };
                tick.Samples = tick.IsBonus ? new[] { new HitSampleInfo("spinnerbonus") } : Samples;
                AddNested(tick);
            }
        }
    }
}
