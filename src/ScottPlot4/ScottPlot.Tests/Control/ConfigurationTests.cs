﻿using NUnit.Framework;
using ScottPlot.Control;
using System;

namespace ScottPlotTests.Control
{

    [TestFixture]
    public class ConfigurationTests
    {
        [TestCase(0.01)]
        [TestCase(0.15)]
        [TestCase(0.7)]
        [TestCase(0.5)]
        public void ScrollWheelZoomIncrement_WithinValidRange_NotThrows(double value)
        {
            Configuration config = new Configuration();
            config.ScrollWheelZoomFraction = value;
        }

        [TestCase(-5)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(0)]
        [TestCase(1)]
        public void ScrollWheelZoomIncrement_WithoutValidRange_Throws(double value)
        {
            Configuration config = new Configuration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.ScrollWheelZoomFraction = value);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(7)]
        public void Test_RenderCount_Increments(int renderCount)
        {
            ControlBackEnd backend = new(400, 300);
            for (int i = 0; i < renderCount; i++)
                backend.Render();
            Assert.AreEqual(renderCount + 1, backend.RenderCount);
        }
    }
}
