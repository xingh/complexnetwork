﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;

using StatisticAnalyzer;

using SettingsConfiguration;
using RandomGraph.Common.Model;
using RandomGraph.Common.Model.Generation;
using CommonLibrary.Model.Attributes;
using CommonLibrary.Model.Result;
using ZedGraph;

namespace StatisticAnalyzerUI
{
    public partial class StatisticAnalyzer : Form
    {
        // Implementation members //
        private StLoader loader;

        // GUI members //
        private List<ComboBox> generationParametersComboBoxes = new List<ComboBox>();
        private ApproximationTypes approximationType;
        private Dictionary<GraphicalInformation, Graphic> existingGraphics;

        public StatisticAnalyzer()
        {
            InitializeComponent();
            InitializeConfigurationMembers();
            InitializeGUIMembers();
        }

        // Event Handlers //

        private void OnLoad(object sender, EventArgs e)
        {
            this.ByJobsRadio.Checked = true;
            this.ApproximationTypeCmb.SelectedIndex = 0;

            InitializeCurveLineCmb();
            InitializeModelNameCmb();
        }

        private void ModelNameSelChange(object sender, EventArgs e)
        {
            loader.ModelName = this.ModelNameCmb.Text;
            loader.InitAssemblies();
            InitializeGenerationParameters();
            FillJobs();
        }

        // By Jobs mode //
        private void ByJobsRadio_CheckedChanged(object sender, EventArgs e)
        {
            this.JobsCmb.Enabled = true;
            this.DeleteJob.Enabled = true;
            this.RefreshBtn.Enabled = false;
            this.GenerationParametersGrp.Enabled = false;
            this.ByAllJobsCheck.Enabled = false;

            RefreshParameters();
        }

        private void DeleteJob_Click(object sender, EventArgs e)
        {
            string name = (string)this.JobsCmb.SelectedItem;
            if (name != null)
            {
                loader.DeleteJob(name);
                FillJobs();
            }
        }

        private void JobsCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshParameters();
        }

        // By Parameters mode //
        private void ByParametersRadio_CheckedChanged(object sender, EventArgs e)
        {
            this.JobsCmb.Enabled = false;
            this.DeleteJob.Enabled = false;
            this.RefreshBtn.Enabled = true;
            this.GenerationParametersGrp.Enabled = true;
            this.ByAllJobsCheck.Enabled = true;
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshAssemblies();
        }

        private void control_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            FillNextGenerationParameterCombos(cmb.TabIndex + 1);
        }

        private void LocalPropertiesList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int index = 0;
            if (e.NewValue == CheckState.Checked)
            {
                index = this.LocalAnalyzeOptionsGrd.Rows.Add();
                this.LocalAnalyzeOptionsGrd.Rows[index].Cells[0].Value = this.LocalPropertiesList.Items[e.Index].ToString();
                this.LocalAnalyzeOptionsGrd.Rows[index].Cells[1].Value = 0;
                this.LocalAnalyzeOptionsGrd.Rows[index].Cells[2].Value = 0;
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                for (int i = 0; i < this.LocalAnalyzeOptionsGrd.Rows.Count; ++i)
                {
                    if (this.LocalAnalyzeOptionsGrd.Rows[i].Cells[0].Value.ToString() ==
                        this.LocalPropertiesList.Items[e.Index].ToString())
                    {
                        index = i;
                        break;
                    }
                }

                this.LocalAnalyzeOptionsGrd.Rows.RemoveAt(index);
            }
        }

        private void LocalAnalyzeOptionsGrd_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = this.LocalAnalyzeOptionsGrd.Rows[e.RowIndex].Cells;
            if (cells[e.ColumnIndex].Value.ToString() != "0")
            {
                for (int i = 1; i < cells.Count; ++i)
                {
                    if (i != e.ColumnIndex)
                        cells[i].Value = "0";
                }
            }
        }

        private void ApproximationTypeCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.ApproximationTypeCmb.SelectedIndex)
            {
                case 0:
                    {
                        approximationType = ApproximationTypes.None;
                        break;
                    }
                case 1:
                    {
                        approximationType = ApproximationTypes.Degree;
                        break;
                    }
                case 2:
                    {
                        approximationType = ApproximationTypes.Exponential;
                        break;
                    }
                case 3:
                    {
                        approximationType = ApproximationTypes.Gaus;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        // Analyzers //
        private void GlobalDrawGraphics_Click(object sender, EventArgs e)
        {
            ResultAssembly res = null;
            if (this.ByJobsRadio.Checked)
                res = loader.SelectAssemblyByJob(this.JobsCmb.Text);
            else
                res = loader.SelectAssemblyByParameters(Values(0, generationParametersComboBoxes.Count));
            StAnalyzer analyzer = new StAnalyzer(res);

            if (this.GlobalPropertiesList.GetItemChecked(0))
                analyzer.options |= AnalyseOptions.AveragePath;
            if (this.GlobalPropertiesList.GetItemChecked(1))
                analyzer.options |= AnalyseOptions.Diameter;
            if (this.GlobalPropertiesList.GetItemChecked(2))
                analyzer.options |= AnalyseOptions.ClusteringCoefficient;
            if (this.GlobalPropertiesList.GetItemChecked(3))
                analyzer.options |= AnalyseOptions.DegreeDistribution;
            if (this.GlobalPropertiesList.GetItemChecked(4))
                analyzer.options |= AnalyseOptions.Cycles3;
            if (this.GlobalPropertiesList.GetItemChecked(5))
                analyzer.options |= AnalyseOptions.Cycles4;
            if (this.GlobalPropertiesList.GetItemChecked(6))
                analyzer.options |= AnalyseOptions.MaxFullSubgraph;
            if (this.GlobalPropertiesList.GetItemChecked(7))
                analyzer.options |= AnalyseOptions.LargestConnectedComponent;
            if (this.GlobalPropertiesList.GetItemChecked(8))
                analyzer.options |= AnalyseOptions.MinEigenValue;
            if (this.GlobalPropertiesList.GetItemChecked(9))
                analyzer.options |= AnalyseOptions.MaxEigenValue;

            analyzer.GlobalAnalyze();
            StAnalyzeResult result = analyzer.Result;
        }

        private void GetGlobalResult_Click(object sender, EventArgs e)
        {
            ResultAssembly res = null;
            if (this.ByJobsRadio.Checked)
                res = loader.SelectAssemblyByJob(this.JobsCmb.Text);
            else
                res = loader.SelectAssemblyByParameters(Values(0, generationParametersComboBoxes.Count));
            StAnalyzer analyzer = new StAnalyzer(res);

            if (this.GlobalPropertiesList.GetItemChecked(0))
                analyzer.options |= AnalyseOptions.AveragePath;
            if (this.GlobalPropertiesList.GetItemChecked(1))
                analyzer.options |= AnalyseOptions.Diameter;
            if (this.GlobalPropertiesList.GetItemChecked(2))
                analyzer.options |= AnalyseOptions.ClusteringCoefficient;
            if (this.GlobalPropertiesList.GetItemChecked(3))
                analyzer.options |= AnalyseOptions.DegreeDistribution;
            if (this.GlobalPropertiesList.GetItemChecked(4))
                analyzer.options |= AnalyseOptions.Cycles3;
            if (this.GlobalPropertiesList.GetItemChecked(5))
                analyzer.options |= AnalyseOptions.Cycles4;
            if (this.GlobalPropertiesList.GetItemChecked(6))
                analyzer.options |= AnalyseOptions.MaxFullSubgraph;
            if (this.GlobalPropertiesList.GetItemChecked(7))
                analyzer.options |= AnalyseOptions.LargestConnectedComponent;
            if (this.GlobalPropertiesList.GetItemChecked(8))
                analyzer.options |= AnalyseOptions.MinEigenValue;
            if (this.GlobalPropertiesList.GetItemChecked(9))
                analyzer.options |= AnalyseOptions.MaxEigenValue;

            analyzer.GlobalAnalyze();
            
            StAnalyzeResult result = analyzer.Result;           
            Information inform = new Information(result.resultAvgValues);
            inform.Show();
        }

        private void LocalDrawGraphics_Click(object sender, EventArgs e)
        {
            ResultAssembly res = null;
            if (this.ByJobsRadio.Checked)
                res = loader.SelectAssemblyByJob(this.JobsCmb.Text);
            else
                res = loader.SelectAssemblyByParameters(Values(0, generationParametersComboBoxes.Count));
            StAnalyzer analyzer = new StAnalyzer(res);
            
            if (this.LocalPropertiesList.GetItemChecked(0))
                analyzer.options |= AnalyseOptions.ClusteringCoefficient;
            if (this.LocalPropertiesList.GetItemChecked(1))
                analyzer.options |= AnalyseOptions.DegreeDistribution;
            if (this.LocalPropertiesList.GetItemChecked(2))
                analyzer.options |= AnalyseOptions.ConnSubGraph;
            if (this.LocalPropertiesList.GetItemChecked(3))
                analyzer.options |= AnalyseOptions.MinPathDist;
            if (this.LocalPropertiesList.GetItemChecked(4))
                analyzer.options |= AnalyseOptions.EigenValue;
            if (this.LocalPropertiesList.GetItemChecked(5))
                analyzer.options |= AnalyseOptions.DistEigenPath;
            if (this.LocalPropertiesList.GetItemChecked(6))
                analyzer.options |= AnalyseOptions.Cycles;

            MakeParameters(analyzer);
            analyzer.LocalAnalyze();

            StAnalyzeResult result = analyzer.Result;
        }

        private void MotifDrawGraphics_Click(object sender, EventArgs e)
        {
        }

        public void DestroyGraphic(GraphicalInformation gr)
        {
            existingGraphics[gr] = null;
        }

        // Menu Event Handlers //
        private void MenuSetProvider_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            StorgeProvider storageProvider;
            if (config.AppSettings.Settings["Storage"].Value == "XmlProvider")
            {
                storageProvider = StorgeProvider.XMLProvider;
            }
            else
            {
                storageProvider = StorgeProvider.SQLProvider;
            }
            string storageDirectory = config.AppSettings.Settings["XmlProvider"].Value;
            string connection = config.AppSettings.Settings["SQLProvider"].Value;
            string connectionString = config.ConnectionStrings.ConnectionStrings[connection].ConnectionString;

            StartUpWindow window = new StartUpWindow(storageProvider, storageDirectory, connectionString);

            if (window.ShowDialog() == DialogResult.OK)
            {
                if (window.Storge == StorgeProvider.XMLProvider)
                {
                    config.AppSettings.Settings["Storage"].Value = "XmlProvider";
                    config.AppSettings.Settings["XmlProvider"].Value = window.StorageDirectory;
                }
                else
                {
                    config.AppSettings.Settings["Storage"].Value = "SQLProvider";
                    config.ConnectionStrings.ConnectionStrings[config.AppSettings.Settings["SQLProvider"].Value].ConnectionString = window.ConnectionString;
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.RefreshSection("connectionStrings");
            }

            loader.InitStorage();
            loader.InitAssemblies();
            FillJobs();
        }

        // Utilities //

        private void InitializeGUIMembers()
        {
            existingGraphics = new Dictionary<GraphicalInformation, Graphic>();

            // Global Graphics //
            existingGraphics[new GraphicalInformation(AnalyseOptions.AveragePath, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.ClusteringCoefficient, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.CycleEigen3, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.CycleEigen4, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.DegreeDistribution, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.Diameter, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.LargestConnectedComponent, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.MaxEigenValue, StatAnalyzeMode.GlobalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.MinEigenValue, StatAnalyzeMode.GlobalMode)] = null;

            // Local Graphics //
            existingGraphics[new GraphicalInformation(AnalyseOptions.ClusteringCoefficient, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.ConnSubGraph, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.Cycles, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.DegreeDistribution, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.DistEigenPath, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.EigenValue, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.FullSubGraph, StatAnalyzeMode.LocalMode)] = null;
            existingGraphics[new GraphicalInformation(AnalyseOptions.MinPathDist, StatAnalyzeMode.LocalMode)] = null;

            // Motif Graphics //
        }

        private void InitializeConfigurationMembers()
        {
            loader = new StLoader();
        }

        private void InitializeModelNameCmb()
        {
            this.ModelNameCmb.Items.AddRange(StLoader.GetAvailableModelNames());
            this.ModelNameCmb.SelectedIndex = 0;
        }

        private void InitializeCurveLineCmb()
        {
            this.CurveLineCmb.Items.Add(Color.Black);
            this.CurveLineCmb.Items.Add(Color.Blue);
            this.CurveLineCmb.Items.Add(Color.Brown);
            this.CurveLineCmb.Items.Add(Color.Chocolate);
            this.CurveLineCmb.Items.Add(Color.Crimson);
            this.CurveLineCmb.Items.Add(Color.DarkGreen);
            this.CurveLineCmb.Items.Add(Color.DarkOliveGreen);
            this.CurveLineCmb.Items.Add(Color.DarkRed);
            this.CurveLineCmb.Items.Add(Color.DarkViolet);
            this.CurveLineCmb.Items.Add(Color.ForestGreen);

            this.CurveLineCmb.SelectedIndex = 0;
        }

        private void InitializeGenerationParameters()
        {
            generationParametersComboBoxes.Clear();
            this.GenerationParametersGrp.Controls.Clear();

            Type modelType = StLoader.models[this.ModelNameCmb.Text].Item2;
            List<RequiredGenerationParam> generationParameters = 
                new List<RequiredGenerationParam>((RequiredGenerationParam[])modelType.
                GetCustomAttributes(typeof(RequiredGenerationParam), false));

            int position = 30;
            int index = 0;
            foreach (RequiredGenerationParam requiredGenerationParam in generationParameters)
            {
                GenerationParamInfo paramInfo =
                    (GenerationParamInfo)(requiredGenerationParam.GenParam.GetType().GetField(
                    requiredGenerationParam.GenParam.ToString()).GetCustomAttributes(typeof(GenerationParamInfo), false)[0]);

                System.Windows.Forms.Label comboBoxLabel = new System.Windows.Forms.Label() { Width = 100 };
                comboBoxLabel.Location = new Point(20, position);
                comboBoxLabel.Text = paramInfo.Name;

                ComboBox control = new ComboBox();
                control.Name = "GenerationParameterCombo";
                control.Width = 150;
                control.Location = new Point(150, position);
                control.TabIndex = index++;
                control.SelectedIndexChanged += new EventHandler(control_SelectedIndexChanged);
                generationParametersComboBoxes.Add(control);

                this.GenerationParametersGrp.Controls.Add(control);
                this.GenerationParametersGrp.Controls.Add(comboBoxLabel);
                position += 30;
            }

            //this.ByAllJobsCheck.Visible = m_analyzer.ByAllJobsOptionValidation;
        }

        private void FillJobs()
        {
            this.JobsCmb.Text = "";

            this.JobsCmb.Items.Clear();
            this.JobsCmb.Items.AddRange(loader.GetAvailableJobs());
            if (this.JobsCmb.Items.Count != 0)
                this.JobsCmb.SelectedIndex = 0;
        }

        private void RefreshParameters()
        {
            string name = (string)this.JobsCmb.SelectedItem;
            if (name != null)
            {
                Type modelType = StLoader.models[this.ModelNameCmb.Text].Item2;
                List<RequiredGenerationParam> generationParameters =
                    new List<RequiredGenerationParam>((RequiredGenerationParam[])modelType.
                    GetCustomAttributes(typeof(RequiredGenerationParam), false));

                Control[] c = this.GenerationParametersGrp.Controls.Find("GenerationParameterCombo", false);
                for (int i = 0; i < c.Length; ++i)
                {
                    c[i].Text = loader.GetParameterValue(name, generationParameters[i].GenParam); 
                }

                //this.RealizationsTxt.Text = m_analyzer.GetRealizationsCount(name);
            }
        }

        private void RefreshAssemblies()
        {
            loader.InitAssemblies();
            FillFirstGenerationParameterCombo();
        }

        // CHECK THE LOGIC AND CORRECT TABINDEX PART //
        private void FillFirstGenerationParameterCombo()
        {
            if (generationParametersComboBoxes.Count != 0)
            {
                generationParametersComboBoxes[0].Text = "";

                generationParametersComboBoxes[0].Items.Clear();
                Type modelType = StLoader.models[this.ModelNameCmb.Text].Item2;
                List<RequiredGenerationParam> generationParameters =
                    new List<RequiredGenerationParam>((RequiredGenerationParam[])modelType.
                    GetCustomAttributes(typeof(RequiredGenerationParam), false));
                List<string> valuesStr = loader.GetParameterValues(generationParameters[0].GenParam);
                foreach (string v in valuesStr)
                    generationParametersComboBoxes[0].Items.Add(v);
                if (generationParametersComboBoxes[0].Items.Count != 0)
                    generationParametersComboBoxes[0].SelectedIndex = 0;
            }
        }

        private void FillNextGenerationParameterCombos(int firstComboIndex)
        {
            for (int i = firstComboIndex; i < generationParametersComboBoxes.Count; ++i)
            {
                generationParametersComboBoxes[i].Text = "";

                generationParametersComboBoxes[i].Items.Clear();
                Type modelType = StLoader.models[this.ModelNameCmb.Text].Item2;
                List<RequiredGenerationParam> generationParameters =
                    new List<RequiredGenerationParam>((RequiredGenerationParam[])modelType.
                    GetCustomAttributes(typeof(RequiredGenerationParam), false));
                List<string> valuesStr = loader.GetParameterValues(Values(firstComboIndex, i), 
                    generationParameters[i].GenParam);
                foreach (string v in valuesStr)
                    generationParametersComboBoxes[i].Items.Add(v);
                if (generationParametersComboBoxes[i].Items.Count != 0)
                    generationParametersComboBoxes[i].SelectedIndex = 0;
            }
        }

        private Dictionary<GenerationParam, string> Values(int firstIndex, int lastIndex)
        {
            Dictionary<GenerationParam, string> values = new Dictionary<GenerationParam, string>();
            for (int i = firstIndex; i < lastIndex; ++i)
            {
                Type modelType = StLoader.models[this.ModelNameCmb.Text].Item2;
                List<RequiredGenerationParam> generationParameters =
                    new List<RequiredGenerationParam>((RequiredGenerationParam[])modelType.
                    GetCustomAttributes(typeof(RequiredGenerationParam), false));
                values.Add(generationParameters[i].GenParam, generationParametersComboBoxes[i].Text);
            }
            return values;
        }
        // CHECK THE LOGIC AND CORRECT TABINDEX PART //
                
        private void MakeParameters(StAnalyzer analyzer)
        {
            Dictionary<AnalyseOptions, StAnalyzeOptions> localOptions =
                new Dictionary<AnalyseOptions, StAnalyzeOptions>();
            int index = 0;
            for (int i = 0; i < this.LocalPropertiesList.Items.Count; ++i)
            {
                if (this.LocalPropertiesList.GetItemChecked(i))
                {
                    index = FindIndexByPropertyName(this.LocalPropertiesList.Items[i].ToString());
                    DataGridViewRow row = this.LocalAnalyzeOptionsGrd.Rows[index];
                    double delta = Convert.ToDouble(row.Cells[1].Value), thickening = Convert.ToDouble(row.Cells[2].Value);
                    double value = delta > 0 ? delta : thickening;
                    bool useDelta = delta > 0 ? true : false;

                    AnalyseOptions param = AnalyseOptions.None;
                    switch (i)
                    {
                        case 0:
                            {
                                param = AnalyseOptions.ClusteringCoefficient;
                                break;
                            }
                        case 1:
                            {
                                param = AnalyseOptions.DegreeDistribution;
                                break;
                            }
                        case 2:
                            {
                                param = AnalyseOptions.ConnSubGraph;
                                break;
                            }
                        case 3:
                            {
                                param = AnalyseOptions.MinPathDist;
                                break;
                            }
                        case 4:
                            {
                                param = AnalyseOptions.EigenValue;
                                break;
                            }
                        case 5:
                            {
                                param = AnalyseOptions.DistEigenPath;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    localOptions[param] = new StAnalyzeOptions(useDelta, value);
                }
            }
            analyzer.AnalyzeOptions = localOptions;

            //m_analyzer.SetAnalyzeParameters(this.ByAllJobsCheck.Checked, localOptions);
        }

        private int FindIndexByPropertyName(string name)
        {
            for (int i = 0; i < this.LocalAnalyzeOptionsGrd.Rows.Count; ++i)
            {
                if (this.LocalAnalyzeOptionsGrd.Rows[i].Cells[0].Value.ToString() == name)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}