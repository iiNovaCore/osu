// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneExpandingControlContainer : OsuManualInputManagerTestScene
    {
        private TestExpandingContainer container;
        private SettingsToolboxGroup toolboxGroup;

        private SettingsSlider<float, SizeSlider> slider1;
        private SettingsSlider<double> slider2;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = container = new TestExpandingContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Height = 0.33f,
                Child = toolboxGroup = new SettingsToolboxGroup("sliders")
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 1,
                    Children = new Drawable[]
                    {
                        slider1 = new SettingsSlider<float, SizeSlider>
                        {
                            Current = new BindableFloat
                            {
                                Default = 1.0f,
                                MinValue = 1.0f,
                                MaxValue = 10.0f,
                                Precision = 0.01f,
                            },
                        },
                        slider2 = new SettingsSlider<double>
                        {
                            Current = new BindableDouble
                            {
                                Default = 1.0,
                                MinValue = 1.0,
                                MaxValue = 10.0,
                                Precision = 0.01,
                            },
                        },
                    }
                }
            };

            slider1.Current.BindValueChanged(v =>
            {
                slider1.LabelText = $"Slider One ({v.NewValue:0.##x})";
                slider1.ContractedLabelText = $"S. 1. ({v.NewValue:0.##x})";
            }, true);

            slider2.Current.BindValueChanged(v =>
            {
                slider2.LabelText = $"Slider Two ({v.NewValue:N2})";
                slider2.ContractedLabelText = $"S. 2. ({v.NewValue:N2})";
            }, true);
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("switch to contracted", () => container.Expanded.Value = false);
            AddStep("switch to expanded", () => container.Expanded.Value = true);
            AddStep("set left origin", () => container.Origin = Anchor.CentreLeft);
            AddStep("set centre origin", () => container.Origin = Anchor.Centre);
            AddStep("set right origin", () => container.Origin = Anchor.CentreRight);
        }

        /// <summary>
        /// Tests hovering over controls expands the parenting container appropriately and does not contract until hover is lost from container.
        /// </summary>
        [Test]
        public void TestHoveringControlExpandsContainer()
        {
            AddAssert("ensure container contracted", () => !container.Expanded.Value);

            AddStep("hover slider", () => InputManager.MoveMouseTo(slider1));
            AddAssert("container expanded", () => container.Expanded.Value);
            AddAssert("controls expanded", () => slider1.Expanded.Value && slider2.Expanded.Value);

            AddStep("hover group top", () => InputManager.MoveMouseTo(toolboxGroup.ScreenSpaceDrawQuad.TopLeft + new Vector2(5)));
            AddAssert("container still expanded", () => container.Expanded.Value);
            AddAssert("controls still expanded", () => slider1.Expanded.Value && slider2.Expanded.Value);

            AddStep("hover away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("container contracted", () => !container.Expanded.Value);
            AddAssert("controls contracted", () => !slider1.Expanded.Value && !slider2.Expanded.Value);
        }

        /// <summary>
        /// Tests dragging a UI control (e.g. <see cref="OsuSliderBar{T}"/>) outside its parenting container does not contract it until dragging is finished.
        /// </summary>
        [Test]
        public void TestDraggingControlOutsideDoesntContractContainer()
        {
            AddStep("hover slider", () => InputManager.MoveMouseTo(slider1));
            AddAssert("container expanded", () => container.Expanded.Value);

            AddStep("hover slider nub", () => InputManager.MoveMouseTo(slider1.ChildrenOfType<Nub>().Single()));
            AddStep("hold slider nub", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag outside container", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("container still expanded", () => container.Expanded.Value);

            AddStep("release slider nub", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("container contracted", () => !container.Expanded.Value);
        }

        /// <summary>
        /// Tests expanding a container will expand underlying groups if contracted.
        /// </summary>
        [Test]
        public void TestExpandingContainerExpandsContractedGroup()
        {
            AddStep("contract group", () => toolboxGroup.Expanded.Value = false);

            AddStep("expand container", () => container.Expanded.Value = true);
            AddAssert("group expanded", () => toolboxGroup.Expanded.Value);
            AddAssert("controls expanded", () => slider1.Expanded.Value && slider2.Expanded.Value);

            AddStep("contract container", () => container.Expanded.Value = false);
            AddAssert("group contracted", () => !toolboxGroup.Expanded.Value);
            AddAssert("controls contracted", () => !slider1.Expanded.Value && !slider2.Expanded.Value);
        }

        /// <summary>
        /// Tests contracting a container does not contract underlying groups if expanded by user (i.e. by setting <see cref="SettingsToolboxGroup.Expanded"/> directly).
        /// </summary>
        [Test]
        public void TestContractingContainerDoesntContractUserExpandedGroup()
        {
            AddAssert("ensure group expanded", () => toolboxGroup.Expanded.Value);

            AddStep("expand container", () => container.Expanded.Value = true);
            AddAssert("group still expanded", () => toolboxGroup.Expanded.Value);
            AddAssert("controls expanded", () => slider1.Expanded.Value && slider2.Expanded.Value);

            AddStep("contract container", () => container.Expanded.Value = false);
            AddAssert("group still expanded", () => toolboxGroup.Expanded.Value);
            AddAssert("controls contracted", () => !slider1.Expanded.Value && !slider2.Expanded.Value);
        }

        /// <summary>
        /// Tests expanding a container via <see cref="ExpandingControlContainer{TControl}.Expanded"/> does not get contracted by losing hover.
        /// </summary>
        [Test]
        public void TestExpandingContainerDoesntGetContractedByHover()
        {
            AddStep("expand container", () => container.Expanded.Value = true);

            AddStep("hover control", () => InputManager.MoveMouseTo(slider1));
            AddAssert("container still expanded", () => container.Expanded.Value);

            AddStep("hover away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("container still expanded", () => container.Expanded.Value);
        }

        private class TestExpandingContainer : ExpandingControlContainer<ISettingsItem>
        {
            public TestExpandingContainer()
                : base(120, 250)
            {
            }
        }
    }
}
