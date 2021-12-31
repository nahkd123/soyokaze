// Copyright (c) Alden Wu <aldenwu0@gmail.com>. Licensed under the MIT Licence.
// See the LICENSE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Soyokaze.Beatmaps;

namespace osu.Game.Rulesets.Soyokaze.Mods
{
    public class SoyokazeModSpinners : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Spinners";
        public override string Acronym => "SP";
        public override string Description => "Smashing keys";
        public override double ScoreMultiplier => 1.0;
        public override IconUsage? Icon => FontAwesome.Regular.Circle;
        public override ModType Type => ModType.DifficultyIncrease;

        public void ApplyToBeatmapConverter(IBeatmapConverter converter)
        {
            var soyokazeConverter = converter as SoyokazeBeatmapConverter;
            soyokazeConverter.CreateSpinners.Value = true;
        }
    }
}
