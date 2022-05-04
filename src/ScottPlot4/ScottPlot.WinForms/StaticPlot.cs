using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScottPlot
{
    public partial class StaticPlot : Form
    {
        public bool Exit { get; set; }

        /// <summary>
        /// Table of all charts that has one-to-one mapping to the TableLayout cells
        /// </summary>
        public FormsPlot[,] ChartsTable = new FormsPlot[1, 1];

        public StaticPlot()
        {
            CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();

            ChartsTable[0, 0] = Chart;

            //Chart.plt.Ticks(useMultiplierNotation: false);
            Chart.Plot.XAxis.TickLabelNotation(exponential: false);
            Chart.Plot.XAxis2.TickLabelNotation(exponential: false);

            Chart.Plot.YAxis.TickLabelNotation(exponential: false);
            Chart.Plot.YAxis2.TickLabelNotation(exponential: false);
        }

        private void StaticPlot_Load(object sender, EventArgs e)
        {
            Hide();
        }

        private void StaticPlot_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            if (!Exit)
                e.Cancel = true;
        }
    }
}
