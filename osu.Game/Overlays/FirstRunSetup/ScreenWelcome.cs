// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.WelcomeTitle))]
    public class ScreenWelcome : FirstRunSetupScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        // Avoid height changes when changing language.
                        new Dimension(GridSizeMode.AutoSize, minSize: 100),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                            {
                                Text = FirstRunSetupOverlayStrings.WelcomeDescription,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            },
                        },
                    }
                },
                new LanguageSelectionFlow
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }
            };
        }

        private class LanguageSelectionFlow : FillFlowContainer
        {
            private Bindable<string> frameworkLocale = null!;

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager frameworkConfig)
            {
                Direction = FillDirection.Full;
                Spacing = new Vector2(5);

                ChildrenEnumerable = Enum.GetValues(typeof(Language))
                                         .Cast<Language>()
                                         .Select(l => new LanguageButton(l)
                                         {
                                             Action = () => frameworkLocale.Value = l.ToCultureCode()
                                         });

                frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
                frameworkLocale.BindValueChanged(locale =>
                {
                    if (!LanguageExtensions.TryParseCultureCode(locale.NewValue, out var language))
                        language = Language.en;

                    foreach (var c in Children.OfType<LanguageButton>())
                        c.Selected = c.Language == language;
                }, true);
            }

            private class LanguageButton : OsuClickableContainer
            {
                public readonly Language Language;

                private Box backgroundBox = null!;

                private OsuSpriteText text = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                private bool selected;

                public bool Selected
                {
                    get => selected;
                    set
                    {
                        if (selected == value)
                            return;

                        selected = value;

                        if (IsLoaded)
                            updateState();
                    }
                }

                public LanguageButton(Language language)
                {
                    Language = language;

                    Size = new Vector2(160, 50);
                    Masking = true;
                    CornerRadius = 10;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    InternalChildren = new Drawable[]
                    {
                        backgroundBox = new Box
                        {
                            Alpha = 0,
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colourProvider.Light1,
                            Text = Language.GetDescription(),
                        }
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    updateState();
                }

                protected override bool OnHover(HoverEvent e)
                {
                    if (!selected)
                        updateState();
                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (!selected)
                        updateState();
                    base.OnHoverLost(e);
                }

                private void updateState()
                {
                    const double duration = 1000;

                    if (selected)
                    {
                        backgroundBox.FadeTo(1, duration, Easing.OutQuint);
                        text.FadeColour(colourProvider.Content1, duration, Easing.OutQuint);
                        text.ScaleTo(1.2f, duration, Easing.OutQuint);
                    }
                    else
                    {
                        backgroundBox.FadeTo(IsHovered ? 0.4f : 0, duration / 2, Easing.OutQuint);
                        text.ScaleTo(1, duration / 2, Easing.OutQuint);
                        text.FadeColour(colourProvider.Light1, duration / 2, Easing.OutQuint);
                    }
                }
            }
        }
    }
}
