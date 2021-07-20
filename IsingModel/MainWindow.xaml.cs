using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace IsingModel
{

    public class IsingSquareGrid
    {


        #region Private Fields


        private int dimension;
        private double[,] grid;
        private Random randomizer = new Random();


        #endregion Private Fields

        #region Properties

        public int Dimension
        {
            get
            {
                return dimension;
            }
            set
            {
                dimension = value > 0 ? value : 4;
            }
        }

        public double[,] Grid
        {
            get
            {
                return grid;
            }
            set
            {
                grid = value;
                for(int i = 0; i < dimension;i++)
                {
                    for(int j = 0; j < dimension;j++)
                    {
                        //grid[i, j] = (randomizer.NextDouble() * 6.28) - 3.14;
                        grid[i, j] = 1.67;
                    }
                }
            }
        }

        #endregion Properties

        #region Constructors
        public IsingSquareGrid(int dimension)
        {
            Dimension = dimension;
            Grid = new double[Dimension, Dimension];
        }
        #endregion Constructors

        #region Private Methods
        private double CalculateEnergyForSpinByPosition(int i, int j)
        {
            int NearUp = (i - 1) >= 0 ? (i - 1) : (dimension - 1);
            int NearDown = (i + 1) < dimension ? (i + 1) : 0;
            int NearLeft = (j - 1) >= 0 ? (j - 1) : (dimension - 1);
            int NearRight = (j + 1) < dimension ? (j + 1) : 0;

            return ( ( Math.Cos(grid[i, j] - grid[i, NearLeft]) + Math.Cos(grid[i, j] - grid[i, NearRight]) + Math.Cos(grid[i, j] - grid[NearUp, j]) + Math.Cos(grid[i, j] - grid[NearDown, i]) ) * -1 );
        }

        private double CalculateEnergyForSpinByPosition(int i, int j, bool isReverse)
        {
            if (!isReverse) return CalculateEnergyForSpinByPosition(i, j);
            int NearUp = (i - 1) >= 0 ? (i - 1) : (dimension - 1);
            int NearDown = (i + 1) < dimension ? (i + 1) : 0;
            int NearLeft = (j - 1) >= 0 ? (j - 1) : (dimension - 1);
            int NearRight = (j + 1) < dimension ? (j + 1) : 0;

            
            return ( Math.Cos(grid[i, j] - grid[i, NearLeft]) + Math.Cos(grid[i, j] - grid[i, NearRight]) + Math.Cos(grid[i, j] - grid[NearUp, j]) + Math.Cos(grid[i, j] - grid[NearDown, i]) );         
        }

        private void ApplyMetropolisForSpinByPosition(int i, int j, double temperature)
        {
            double OriginalEnergy = CalculateEnergyForSpinByPosition(i, j);
            double ReverseEnergy = CalculateEnergyForSpinByPosition(i, j, true);
            if (OriginalEnergy > ReverseEnergy)
            {
                grid[i, j] *= (-1);
            }
            else
            {
                double ChanceToReverse = Math.Exp( ((-1)*(ReverseEnergy - OriginalEnergy)) / temperature );
                grid[i, j] = ChanceToReverse < randomizer.NextDouble() ? grid[i, j] : (-1 * grid[i, j]);
            }
        }

        #endregion Private Methods

        #region Public Methods

        public double EnergyForRandomSpin()
        {
            int randomIndexI = randomizer.Next(dimension);
            int randomIndexJ = randomizer.Next(dimension);
            return CalculateEnergyForSpinByPosition(randomIndexI, randomIndexJ);
        }

        public double EnergyForAllSystem()
        {
            double sum = 0;
            for(int i = 0; i < dimension; i++)
            {
                for(int j = 0; j < dimension; j++)
                {
                    sum += CalculateEnergyForSpinByPosition(i, j);
                }
            }

            return (sum / (dimension * dimension));
        }

        public double MagnetizationOfAllSystem()
        {
            double sum = 0;
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    sum += grid[i,j];
                }
            }

            return Math.Abs(sum / (dimension * dimension));
        }

        public void ApplyMetropolisForAllSystem(int iterationsAmount,double temperature)
        {
            for(int i = 0; i < iterationsAmount;i++)
            {
                int randomIndexI = randomizer.Next(dimension);
                int randomIndexJ = randomizer.Next(dimension);

                ApplyMetropolisForSpinByPosition(randomIndexI, randomIndexJ, temperature);
            }
        }

        #endregion Public Methods
    }

    public class OutputManager
    {
        private StreamWriter writer;
        public void DisplayIsingGrid(IsingSquareGrid isingGrid, TextBlock display)
        {
            display.Text = "";
            for (int i = 0; i < isingGrid.Dimension; i++)
            {
                for (int j = 0; j < isingGrid.Dimension; j++)
                {
                    if (isingGrid.Grid[i, j] >= 0) display.Inlines.Add(new Run("S  ") { FontWeight = FontWeights.Bold, Foreground = Brushes.Green });
                    else display.Inlines.Add(new Run("S  ") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                }
                display.Inlines.Add("\n");
            }
        }

        public void DisplayOneSpinEnergy(double energyValue, TextBlock display)
        {
            display.Text = "";
            display.Inlines.Add("Энергия \nслучайного спина  = ");
            display.Inlines.Add(energyValue.ToString());
            
        }

        public void DisplaySystemEnergy(double energyValue, TextBlock display)
        {
            display.Text = "";
            display.Inlines.Add("Энергия \nвсей системы  = ");
            display.Inlines.Add(energyValue.ToString());
        }

        public void DisplaySystemMagnetization(double energyValue, TextBlock display)
        {
            display.Text = "";
            display.Inlines.Add("Намагниченность \nвсей системы  = ");
            display.Inlines.Add(energyValue.ToString());
        }

        public void ArgumentAndValueToFile(string filename, List<double> arguments, List<double> values, double error)
        {
            if (arguments.Count != values.Count) return;
            writer = new StreamWriter(filename + ".txt", true);
            for(int i = 0; i < arguments.Count; i++)
            {
                writer.WriteLine(arguments[i].ToString() + " " + values[i].ToString() + " " + error);
            }
            writer.Close();
        }

        public void ArgumentAndValueToFile(string filename, double argument, double value, double error)
        {
            writer = new StreamWriter(filename + ".txt",true);
            writer.WriteLine(argument.ToString() + " " + value.ToString() + " " + error.ToString());
            writer.Close();
        }

        public void GridToFile(string filename, IsingSquareGrid grid)
        {
            writer = new StreamWriter(filename + ".txt", true);
            for(int i=0; i<grid.Dimension;i++)
            {
                for(int j=0; j<grid.Dimension;j++)
                {
                    writer.WriteLine(i.ToString() + " " + j.ToString() + " 0\t" + Math.Cos(grid.Grid[i,j]).ToString() + " " + Math.Sin(grid.Grid[i, j]).ToString() + " 0");
                }
            }
            writer.Close();
        }

        public void ArgumentAndValueToFile(string filename, double argument, double value)
        {
            writer = new StreamWriter(filename + ".txt", true);
            writer.WriteLine(argument.ToString() + " " + value.ToString());
            writer.Close();
        }

        public void CreateEmptyFile(string filename) 
        {
            writer = new StreamWriter(filename + ".txt",false);
            writer.Close();
        }
    }

    public class AlhorithmCalculator
    {
        public double CalculateError(List<double> values)
        {
            double averageValue = CalculateAverage(values);
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += (values[i] - averageValue) * (values[i] - averageValue);
            }

            return (sum / values.Count);
        }

        public double CalculateAverage(List<double> values)
        {
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }
            return (sum / values.Count);
        }

        public double CalculateAverageOfSquares(List<double> values)
        {
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i] * values[i];
            }
            return (sum / values.Count);
        }
    }

    public partial class MainWindow : Window
    {
        private OutputManager   output = new OutputManager();
        private IsingSquareGrid squareGrid = null;
        AlhorithmCalculator     calc = new AlhorithmCalculator();
        private double          t = 0.01;

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        private void GenerateGridClick(object sender, RoutedEventArgs e)
        {
            squareGrid = new IsingSquareGrid(Convert.ToInt32(txtboxDimension.Text));
            output.DisplayIsingGrid(squareGrid, ArrayOutput);
        }

        private void CalculateValues(object sender, RoutedEventArgs e)
        {
            List<double> errors = new List<double>();
            List<double> temperatures = new List<double>();
            List<double> energies = new List<double>();
            List<double> magnetizations = new List<double>();

            if (squareGrid == null) return;
            output.DisplayOneSpinEnergy(squareGrid.EnergyForRandomSpin(), SpinEnergyOutput);
            output.DisplaySystemEnergy(squareGrid.EnergyForAllSystem(), SystemEnergyOutput);
            output.DisplaySystemMagnetization(squareGrid.MagnetizationOfAllSystem(), MagnetizationEnergyOutput);

            output.CreateEmptyFile(txtboxETFilename.Text);
            output.CreateEmptyFile(txtboxMTFilename.Text);
            output.CreateEmptyFile(txtboxCTFilename.Text);

            squareGrid.ApplyMetropolisForAllSystem(100000, t);

            int confNumbers = 0;

            while (t < 4)
            {
                temperatures.Add(t);
                for (int i = 0; i < 10; i++)
                {
                    squareGrid.ApplyMetropolisForAllSystem(100000, t);
                    energies.Add(squareGrid.EnergyForAllSystem());
                    magnetizations.Add(squareGrid.MagnetizationOfAllSystem());
                }
                output.ArgumentAndValueToFile(txtboxETFilename.Text, t, calc.CalculateAverage(energies), calc.CalculateError(energies));
                output.ArgumentAndValueToFile(txtboxMTFilename.Text, t, calc.CalculateAverage(magnetizations), calc.CalculateError(magnetizations));
                double tmp = calc.CalculateAverage(energies);
                output.ArgumentAndValueToFile(txtboxCTFilename.Text, t, ( (calc.CalculateAverageOfSquares(energies) - (tmp * tmp)) / (t * t)) );

                energies.Clear();
                magnetizations.Clear();
                t += 0.1;

                string animationFileName;
                animationFileName = "conf-" + Convert.ToString(confNumbers, 2);
                output.CreateEmptyFile(animationFileName);
                output.GridToFile(animationFileName, squareGrid);

                confNumbers++;
            }      
            
            output.DisplayIsingGrid(squareGrid, ArrayOutput);
             

        }
    }


}
