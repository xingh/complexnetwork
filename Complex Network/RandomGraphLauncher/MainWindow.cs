﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyControlLibrary;
using RandomGraph.Core.Manager.Impl;
using RandomGraph.Common.Storage;
using RandomGraphLauncher.models;
using CommonLibrary.Model.Attributes;
using RandomGraph.Common.Model;
using ResultStorage.Storage;
using System.Configuration;
using SettingsConfiguration;
using AnalyzerFramework.Manager.ModelRepo;
using AnalyzerFramework.Manager.Impl;
using System.Diagnostics;
using log4net;

using RandomGraph.Common.Model.Settings;

namespace RandomGraphLauncher
{
    public partial class MainWindow : Form
    {
        // Организация работы с лог файлом.
        protected static readonly ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));

        // model name, model factory, model
        private IDictionary<string, Tuple<Type, Type>> models = new Dictionary<string, Tuple<Type, Type>>();
        private AbstractGraphManager manager;
        private List<string> runningJobs = new List<string>();

        private IResultStorage storageManager;

        public MainWindow()
        {
            InitializeComponent();
            InitStorageManager();

            List<Type> availableModelFactoryTypes = ModelRepository.GetInstance().GetAvailableModelFactoryTypes();
            foreach (Type modelFactoryType in availableModelFactoryTypes)
            {
                object[] attr = modelFactoryType.GetCustomAttributes(typeof(TargetGraphModel), false);
                TargetGraphModel targetGraphMetadata = (TargetGraphModel)attr[0];
                Type modelType = targetGraphMetadata.GraphModelType;

                attr = modelType.GetCustomAttributes(typeof(GraphModel), false);
                string modelName = ((GraphModel)attr[0]).Name;

                models.Add(modelName, Tuple.Create<Type, Type>(modelFactoryType, modelType));
            }
        }

        private void InitStorageManager()
        {
            string provider = ConfigurationManager.AppSettings["Storage"];
            if (provider == "XmlProvider")
            {
                storageManager = new XMLResultStorage(ConfigurationManager.AppSettings[provider]);
            }
            else if (provider == "SQLProvider")
            {
                storageManager = new SQLResultStorage(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings[provider]]);
            }
        }

        private void userControl11_OnClose(object sender, CloseEventArgs e)
        {
            TabPage tab = this.userControl11.TabPages[e.TabIndex];
            ((CalculationControl)tab.Controls.Find("calculationControl", true)[0]).closeCalculation();
            if (this.userControl11.TabPages.Count > 1)
            {
                this.userControl11.Controls.Remove(tab);
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void NewJob()
        {
            ModelChooserWindow modelChooser = new ModelChooserWindow(models.Keys, storageManager, runningJobs);

            modelChooser.ShowInTaskbar = false;
            if (modelChooser.ShowDialog() == DialogResult.OK)
            {
                string modelName = modelChooser.comboBox_ModelType.SelectedItem.ToString();
                string jobName = modelChooser.textBox_JobName.Text;
                if (modelChooser.textBox_JobName.Text != null)
                {
                    jobName = modelChooser.textBox_JobName.Text;
                }
                modelChoosed(models[modelName], jobName);
            }
        }

        private void modelChoosed(Tuple<Type, Type> modelType, string jobName)
        {
            if (Options.DistributedMode)
            {
                manager = new DistributedGraphManager(storageManager);
            }
            else
            {
                manager = new MultiTreadGraphManager(storageManager);
            }
            runningJobs.Add(jobName);
            TabPageEx tabPage = new MyControlLibrary.TabPageEx(new System.ComponentModel.Container());

            tabPage.SuspendLayout();
            this.userControl11.Controls.Add(tabPage);
            this.userControl11.SelectedTab = tabPage;
            // 
            // calculationControl
            // 
            CalculationControl calculationControl = new CalculationControl(modelType.Item1, 
                modelType.Item2, 
                jobName, manager);
            calculationControl.Dock = System.Windows.Forms.DockStyle.Fill;
            calculationControl.Location = new System.Drawing.Point(0, 0);
            calculationControl.Name = "calculationControl";
            calculationControl.Size = new System.Drawing.Size(1005, 457);
            calculationControl.TabIndex = 0;

            // 
            // tabPage
            // 
            tabPage.Controls.Add(calculationControl);
            tabPage.Location = new System.Drawing.Point(4, 28);
            tabPage.Menu = null;
            tabPage.Name = "tabPage";
            tabPage.Size = new System.Drawing.Size(1005, 457);
            tabPage.TabIndex = 1;
            tabPage.Text = jobName;

            tabPage.ResumeLayout(false);

            //calculationControl.ExecutionStatusChange += new CalculationControl.AnalyzeEventHandler(graphWindow_ExecutionStatusChange);
            Width++;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void statisticAnalyzerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("StatisticAnalyzerUI.exe");
        }

        private void dataExportIMportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataExportImport window = new DataExportImport(storageManager);
            window.ShowDialog();
        }

        private void FormClosing_Event(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void testerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TesterForm tester = new TesterForm();
            tester.ShowDialog();
        }

        private void modelCheckingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModelCheckWindow wnd = new ModelCheckWindow();
            wnd.ShowDialog();
        }

        private void newJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewJob(); 
        }
        
        // <Mikayel Samvelyan>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsOptionsWindow window = new SettingsOptionsWindow();
            window.ShowDialog();    
        }     
        // </Mikayel Samvelyan>
    }
}
