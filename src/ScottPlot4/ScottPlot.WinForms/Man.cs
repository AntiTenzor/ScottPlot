using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//using ScottPlot.UserControls;

namespace ScottPlot
{
    /// <summary>
    /// Static manager to simplify access to the drawing surface
    /// </summary>
    public sealed class Man
    {
        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly Man instance = new Man();
        }

        private Thread UiThread;
        /// <summary>
        /// Access to the WINDOW with charting area
        /// </summary>
        private readonly StaticPlot chartForm;

        /// <summary>
        /// We have to make private ctor to implement a Singleton pattern
        /// </summary>
        private Man()
        {
            chartForm = new StaticPlot();

            UiThread = new Thread(RunUiThread);
            UiThread.IsBackground = true;
            UiThread.SetApartmentState(ApartmentState.STA);
            UiThread.Start();
        }

        private void RunUiThread()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread.Sleep(700);

            // This call blocks execution of the current thread.
            Application.Run(Man.ChartForm);
        }

        public static StaticPlot ChartForm
        {
            get
            {
                //if (chartForm == null)
                //{
                //    //Thread.Sleep(100);
                //    chartForm = new StaticPlot() { Visible = false };
                //}

                //return chartForm;

                return Nested.instance.chartForm;
            }
        }

        public static void RenderAll(bool skipIfCurrentlyRendering = false, bool lowQuality = false)
        {
            for(int row = 0; row < ChartForm.ChartsTable.GetLength(0); row++)
            {
                for (int col = 0; col < ChartForm.ChartsTable.GetLength(1); col++)
                {
                    FormsPlot chart = ChartForm.ChartsTable[row, col];
                    chart.Render(skipIfCurrentlyRendering, lowQuality);
                }
            }
        }

        public static DialogResult ShowDialog()
        {
            RenderAll();
            DialogResult res = ChartForm.ShowDialog();
            return res;
        }

        public static void Show()
        {
            RenderAll();
            ChartForm.Show();
        }

        public static void Hide()
        {
            ChartForm.Hide();
        }

        public static void Exit()
        {
            ChartForm.Exit = true;
            ChartForm.Close();
            //Application.Exit();
        }

        public static void Clear()
        {
            for (int row = 0; row < ChartForm.ChartsTable.GetLength(0); row++)
            {
                for (int col = 0; col < ChartForm.ChartsTable.GetLength(1); col++)
                {
                    FormsPlot chart = ChartForm.ChartsTable[row, col];
                    chart.plt.Clear();
                }
            }

            RenderAll();
        }

        public static void SetLayout(int rows, int cols)
        {
            rows = Math.Max(1, rows);
            cols = Math.Max(1, cols);

            var oldChartsTable = ChartForm.ChartsTable;
            int oldRows = oldChartsTable.GetLength(0);
            int oldCols = oldChartsTable.GetLength(1);

            ChartForm.tableLayout.SuspendLayout();
            ChartForm.SuspendLayout();

            // 1. Добавляем новые строки. При этом желательно оси синхронизировать?
            var firstRowStyle = ChartForm.tableLayout.RowStyles[0];
            if (oldRows < rows)
            {
                // Поехали переносить готовые графики и создавать новые
                FormsPlot[,] newChartsTable = new FormsPlot[rows, oldCols];

                ChartForm.tableLayout.RowCount = rows;
                for (int j = 0; j < rows; j++)
                {
                    if (j < oldRows)
                    {
                        // Переносим старые графики в новую таблицу?
                        newChartsTable[j, 0] = oldChartsTable[j, 0];
                    }
                    else
                    {
                        // Добавляем новый стиль строки и создаём новый график!
                        ChartForm.tableLayout.RowStyles.Add(new RowStyle(firstRowStyle.SizeType, firstRowStyle.Height));

                        var newChart = new ScottPlot.FormsPlot();
                        newChartsTable[j, 0] = newChart;

                        newChart.Dock = System.Windows.Forms.DockStyle.Fill;
                        newChart.Location = new System.Drawing.Point(3, 3);
                        newChart.Name = "Chart[" + j + "," + 0 + "]";
                        newChart.Size = new System.Drawing.Size(320, 200);

                        ChartForm.tableLayout.Controls.Add(newChart, 0, j);
                    }
                }

                // xx. Заменяю таблицу с чартами
                ChartForm.ChartsTable = newChartsTable;
                oldChartsTable = ChartForm.ChartsTable;
            }

            // 3. Добавляю новые столбцы
            var firstColStyle = ChartForm.tableLayout.ColumnStyles[0];
            if (oldCols < cols)
            {
                ChartForm.tableLayout.ColumnCount = cols;
                for (int j = 0; j < cols - oldCols; j++)
                {
                    ChartForm.tableLayout.ColumnStyles.Add(new ColumnStyle(firstColStyle.SizeType, firstColStyle.Width));
                }
            }

            ChartForm.tableLayout.ResumeLayout();
            ChartForm.ResumeLayout();

            RenderAll();
        }

        /// <summary>
        /// Очищает, рисует новый график, показывает окно.
        /// </summary>
        /// <param name="dataXs"></param>
        /// <param name="dataYs"></param>
        /// <param name="label"></param>
        public static FormsPlot PlotScatter(double[] dataXs, double[] dataYs,
            int row = 0, int col = 0, bool clear = true,
            string title = null, string xLabel = null, string yLabel = null,

            Color? color = null,
            double lineWidth = 1,
            double markerSize = 5,
            string label = null,
            double[] errorX = null,
            double[] errorY = null,
            double errorLineWidth = 1,
            double errorCapSize = 3,
            MarkerShape markerShape = MarkerShape.filledCircle,
            LineStyle lineStyle = LineStyle.Solid)
        {
            var chart = ChartForm.ChartsTable[row, col];
            if (clear)
                chart.plt.Clear();

            chart.plt.PlotScatter(dataXs, dataYs,
                color, lineWidth, markerSize, label,
                errorX, errorY, errorLineWidth, errorCapSize,
                markerShape, lineStyle);

            if (clear)
            {
                chart.plt.Title(title);
                chart.plt.XLabel(xLabel);
                chart.plt.YLabel(yLabel);
            }

            chart.plt.AxisAuto();
            chart.Render();

            // Если окно спрятано, восстанавливаем его
            ChartForm.Show();

            return chart;
        }

        #region User friendly display sugar
        public static void SetTitle(int row = 0, int col = 0,
            string title = null)
        {
            var chart = ChartForm.ChartsTable[row, col];
            chart.plt.Title(title);
        }

        public static void SetXLabel(int row = 0, int col = 0,
            string xLabel = null)
        {
            var chart = ChartForm.ChartsTable[row, col];
            chart.plt.XLabel(xLabel);
        }

        public static void SetYLabel(int row = 0, int col = 0,
            string yLabel = null)
        {
            var chart = ChartForm.ChartsTable[row, col];
            chart.plt.YLabel(yLabel);
        }

        public static void SetLegend(int row = 0, int col = 0,
            bool enableLegend = true,
            string fontName = "Segoe UI",
            float fontSize = 12,
            bool bold = false,
            Color? fontColor = null,
            Color? backColor = null,
            Color? frameColor = null,
            Alignment legendLocation = Alignment.LowerRight,
            Alignment shadowDirection = Alignment.LowerRight,
            bool? fixedLineWidth = null)
        {
            var chart = ChartForm.ChartsTable[row, col];
            chart.plt.Legend(enable: enableLegend);
        }

        public static void SetTicks(int row = 0, int col = 0,
            bool? displayTicksX = null,
            bool? displayTicksY = null,
            bool? displayTicksXminor = null,
            bool? displayTicksYminor = null,
            Color? color = null,
            bool? useMultiplierNotation = false,
            bool? useOffsetNotation = null,
            bool? useExponentialNotation = null,
            bool? dateTimeX = null,
            bool? dateTimeY = null,
            bool? rulerModeX = null,
            bool? rulerModeY = null,
            bool? invertSignX = null,
            bool? invertSignY = null)
        {
            var chart = ChartForm.ChartsTable[row, col];
            //var settings = chart.plt.GetSettings();

            // tick display options
            //chart.Plot.Ticks(useMultiplierNotation: useMultiplierNotation);
            chart.Plot.XAxis.TickLabelNotation(exponential: useMultiplierNotation);
            chart.Plot.XAxis2.TickLabelNotation(exponential: useMultiplierNotation);

            chart.Plot.YAxis.TickLabelNotation(exponential: useMultiplierNotation);
            chart.Plot.YAxis2.TickLabelNotation(exponential: useMultiplierNotation);
        }
        #endregion User friendly display sugar
    }
}
