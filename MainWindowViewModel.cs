﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using IbDataTool.Model;
using IbDataTool.Queries;

namespace IbDataTool
{
    /// <summary>
    /// MainWindowViewModel
    /// </summary>
    public class MainWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty PortIbProperty;
        public static readonly DependencyProperty ConnectionStringProperty;
        public static readonly DependencyProperty LogSymbolsProperty;
        public static readonly DependencyProperty LogFundamentalsProperty;
        public static readonly DependencyProperty CompaniesProperty;
        public static readonly DependencyProperty SymbolsProperty;
        public static readonly DependencyProperty ExchangesIbProperty;
        public static readonly DependencyProperty ExchangeSelectedProperty;
        public static readonly DependencyProperty BackgroundLogProperty;
        public static readonly DependencyProperty DateProperty;
        public static readonly DependencyProperty LogCurrentProperty;
        public static readonly DependencyProperty ConnectedToIbProperty;
        public static readonly DependencyProperty InventoryTextProperty;
        public static readonly DependencyProperty CompaniesForSymbolResolutionProperty;
        public static readonly DependencyProperty ExchangesFmpProperty;
        public static readonly DependencyProperty ExchangesFmpSelectedProperty;
        public static readonly DependencyProperty ExchangeFmpInitialProperty;
        public static readonly DependencyProperty CompaniesForSymbolResolutionHeaderProperty;
        public static readonly DependencyProperty CompaniesForSymbolResolutionTextProperty;
        public static readonly DependencyProperty DatesProperty;
        public static readonly DependencyProperty SelectTop1000Property;
        public static readonly DependencyProperty CompaniesForFundamenatalsTextProperty;

        public RelayCommand CommandConnectToIb { get; set; }
        public RelayCommand CommandImportFundamentals { get; set; }
        public RelayCommand CommandImportContracts { get; set; }

        #region Constructors

        static MainWindowViewModel()
        {
            PortIbProperty = DependencyProperty.Register("PortIb", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            ConnectionStringProperty = DependencyProperty.Register("ConnectionString", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            LogSymbolsProperty = DependencyProperty.Register("LogSymbols", typeof(ObservableCollection<string>), typeof(MainWindowViewModel), new PropertyMetadata(new ObservableCollection<string>()));
            LogFundamentalsProperty = DependencyProperty.Register("LogFundamentals", typeof(ObservableCollection<string>), typeof(MainWindowViewModel), new PropertyMetadata(new ObservableCollection<string>()));
            LogCurrentProperty = DependencyProperty.Register("LogCurrent", typeof(ObservableCollection<string>), typeof(MainWindowViewModel), new PropertyMetadata(new ObservableCollection<string>()));
            CompaniesProperty = DependencyProperty.Register("Companies", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            SymbolsProperty = DependencyProperty.Register("Symbols", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            ExchangesIbProperty = DependencyProperty.Register("ExchangesIb", typeof(List<string>), typeof(MainWindowViewModel), new PropertyMetadata(new List<string>()));
            ExchangeSelectedProperty = DependencyProperty.Register("ExchangeIbSelected", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            BackgroundLogProperty = DependencyProperty.Register("BackgroundLog", typeof(Brush), typeof(MainWindowViewModel), new PropertyMetadata(default(Brush)));
            DateProperty = DependencyProperty.Register("Date", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty, DatePropertyChanged));
            ConnectedToIbProperty = DependencyProperty.Register("ConnectedToIb", typeof(bool), typeof(MainWindowViewModel), new PropertyMetadata(false));
            InventoryTextProperty = DependencyProperty.Register("InventoryText", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            CompaniesForSymbolResolutionProperty = DependencyProperty.Register("CompaniesWoDocumentsIbSymbolNotResolvedText", typeof(List<string>), typeof(MainWindowViewModel), new PropertyMetadata(new List<string>()));
            ExchangesFmpProperty = DependencyProperty.Register("ExchangesFmp", typeof(List<string>), typeof(MainWindowViewModel), new PropertyMetadata(new List<string>()));
            ExchangesFmpSelectedProperty = DependencyProperty.Register("ExchangesFmpSelected", typeof(List<string>), typeof(MainWindowViewModel), new PropertyMetadata(new List<string>()));
            ExchangeFmpInitialProperty = DependencyProperty.Register("ExchangeFmpInitial", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            CompaniesForSymbolResolutionHeaderProperty = DependencyProperty.Register("CompaniesForSymbolResolutionHeader", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            CompaniesForSymbolResolutionTextProperty = DependencyProperty.Register("CompaniesForSymbolResolutionText", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));
            DatesProperty = DependencyProperty.Register("Dates", typeof(ObservableCollection<string>), typeof(MainWindowViewModel), new PropertyMetadata(new ObservableCollection<string>()));
            SelectTop1000Property = DependencyProperty.Register("SelectTop1000", typeof(bool), typeof(MainWindowViewModel), new PropertyMetadata(false));
            CompaniesForFundamenatalsTextProperty = DependencyProperty.Register("CompaniesForFundamenatalsText", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(string.Empty));

        }

        public MainWindowViewModel()
        {
            PortIb = Convert.ToInt32(Configuration.Instance["PortIb"]);
            ConnectionString = Configuration.Instance["ConnectionString"];
            LogSymbols.Add("Willkommen! Enjoy the day (-:");
            LogFundamentals.Add("Willkommen! Enjoy the day (-:");
            BackgroundLog = Brushes.White;

            InitDatesCombobok();
            InitExchangeCombobox();
            InitExchangeFmpCombobox();

            UpdateDbState();
            InventoryText = GenerateInventoryText();
            SelectTop1000 = true;
            UpdateCompaniesForFundamenatals(SelectTop1000);

            CommandConnectToIb = new RelayCommand(async (p) => await CommandConnectToIbAsync(p));
            CommandImportContracts = new RelayCommand(async (p) => await CommandImportContractsAsync(p));
            CommandImportFundamentals = new RelayCommand(async (p) => await CommandImportFundamentalsAsync(p));

            IbClient.Instance.NextValidId += ResponseHandlerNextValidId;
            IbClient.Instance.ConnectionClosed += ResponseHandlerConnectionClosed;
            IbClient.Instance.SymbolSamples += ResponseHandlerSymbolSamples;
            IbClient.Instance.FundamentalData += ResponseHandlerFundamentalData;
        }

        #endregion

        #region Properties

        /// <summary>
        /// PortIb
        /// </summary>
        public int PortIb
        {
            get { return (int)GetValue(PortIbProperty); }
            set { SetValue(PortIbProperty, value); }
        }

        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return (string)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        /// <summary>
        /// LogSymbols
        /// </summary>
        public ObservableCollection<string> LogSymbols
        {
            get { return (ObservableCollection<string>)GetValue(LogSymbolsProperty); }
            set { SetValue(LogSymbolsProperty, value); }
        }

        /// <summary>
        /// LogFundamentals
        /// </summary>
        public ObservableCollection<string> LogFundamentals
        {
            get { return (ObservableCollection<string>)GetValue(LogFundamentalsProperty); }
            set { SetValue(LogFundamentalsProperty, value); }
        }

        /// <summary>
        /// LogCurrent
        /// </summary>
        public ObservableCollection<string> LogCurrent
        {
            get { return (ObservableCollection<string>)GetValue(LogCurrentProperty); }
            set { SetValue(LogCurrentProperty, value); }
        }

        /// <summary>
        /// RequestPending
        /// </summary>
        public bool RequestPending { get; set; }

        /// <summary>
        /// CompaniesForSymbolResolution
        /// </summary>
        public List<string> CompaniesForSymbolResolution
        {
            get { return (List<string>)GetValue(CompaniesForSymbolResolutionProperty); }
            set { SetValue(CompaniesForSymbolResolutionProperty, value); }
        }


        /// <summary>
        /// Symbols
        /// </summary>
        public string Symbols
        {
            get { return (string)GetValue(SymbolsProperty); }
            set { SetValue(SymbolsProperty, value); }
        }

        /// <summary>
        /// ExchangesIb
        /// </summary>
        public List<string> ExchangesIb
        {
            get { return (List<string>)GetValue(ExchangesIbProperty); }
            set { SetValue(ExchangesIbProperty, value); }
        }

        /// <summary>
        /// ExchangeIbSelected
        /// </summary>
        public string ExchangeIbSelected
        {
            get { return (string)GetValue(ExchangeSelectedProperty); }
            set { SetValue(ExchangeSelectedProperty, value); }
        }

        /// <summary>
        /// ExchangesFmpSelected
        /// </summary>
        public List<string> ExchangesFmpSelected
        {
            get { return (List<string>)GetValue(ExchangesFmpSelectedProperty); }
            set
            {
                SetValue(ExchangesFmpSelectedProperty, value);
                UpdateCompaniesForSymbolResolution(value);
            }
        }

        /// <summary>
        /// ExchangeFmpInitial
        /// </summary>
        public string ExchangeFmpInitial
        {
            get { return (string)GetValue(ExchangeFmpInitialProperty); }
            set { SetValue(ExchangeFmpInitialProperty, value); }
        }

        /// <summary>
        /// BackgroundLog
        /// </summary>
        public Brush BackgroundLog
        {
            get { return (Brush)GetValue(BackgroundLogProperty); }
            set { SetValue(BackgroundLogProperty, value); }
        }

        /// <summary>
        /// Date
        /// </summary>
        public string Date
        {
            get { return (string)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        /// <summary>
        /// ConnectedToIb
        /// </summary>
        public bool ConnectedToIb
        {
            get { return (bool)GetValue(ConnectedToIbProperty); }
            set { SetValue(ConnectedToIbProperty, value); }
        }

        /// <summary>
        /// CompaniesList
        /// </summary>
        public List<string> CompaniesList { get; set; }

        /// <summary>
        /// CurrentCompany
        /// </summary>
        public string CurrentCompany { get; private set; }

        /// <summary>
        /// CompaniesProcessed
        /// </summary>
        public List<string> CompaniesProcessed { get; set; }

        /// <summary>
        /// SymbolProcessed
        /// </summary>
        public List<string> SymbolProcessed { get; set; }

        /// <summary>
        /// ContractList
        /// </summary>
        public List<Contract> ContractList { get; set; }

        /// <summary>
        /// CurrentContract
        /// </summary>
        public Contract CurrentContract { get; private set; }

        /// <summary>
        /// ContractsProcessed
        /// </summary>
        public List<Contract> ContractsProcessed { get; set; }

        /// <summary>
        /// InventoryText
        /// </summary>
        public string InventoryText
        {
            get { return (string)GetValue(InventoryTextProperty); }
            set { SetValue(InventoryTextProperty, value); }
        }

        /// <summary>
        /// ExchangesFmp
        /// </summary>
        public List<string> ExchangesFmp
        {
            get { return (List<string>)GetValue(ExchangesFmpProperty); }
            set { SetValue(ExchangesFmpProperty, value); }
        }

        /// <summary>
        /// CompaniesForSymbolResolutionHeader
        /// </summary>
        public string CompaniesForSymbolResolutionHeader
        {
            get { return (string)GetValue(CompaniesForSymbolResolutionHeaderProperty); }
            set { SetValue(CompaniesForSymbolResolutionHeaderProperty, value); }
        }

        /// <summary>
        /// CompaniesForSymbolResolutionText
        /// </summary>
        public string CompaniesForSymbolResolutionText
        {
            get { return (string)GetValue(CompaniesForSymbolResolutionTextProperty); }
            set { SetValue(CompaniesForSymbolResolutionTextProperty, value); }
        }

        /// <summary>
        /// Dates
        /// </summary>
        public ObservableCollection<string> Dates
        {
            get { return (ObservableCollection<string>)GetValue(DatesProperty); }
            set { SetValue(DatesProperty, value); }
        }

        /// <summary>
        /// SelectTop1000
        /// </summary>
        public bool SelectTop1000
        {
            get { return (bool)GetValue(SelectTop1000Property); }
            set { SetValue(SelectTop1000Property, value); }
        }

        /// <summary>
        /// CompaniesForFundamenatalsText
        /// </summary>
        public string CompaniesForFundamenatalsText
        {
            get { return (string)GetValue(CompaniesForFundamenatalsTextProperty); }
            set { SetValue(CompaniesForFundamenatalsTextProperty, value); }
        }

        #endregion

        #region Commands

        /// <summary>
        /// ConnectToIbAsync
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private async Task CommandConnectToIbAsync(object p)
        {
            LogFundamentals.Clear();
            LogSymbols.Clear();
            BackgroundLog = Brushes.Gray;

            await Task.Run(() =>
            {
                int portIb = 0;
                string host = string.Empty;
                int delay = 0;
                int timeout = 0;
                Dispatcher.Invoke(() =>
                {
                    portIb = PortIb;
                    host = Configuration.Instance["Localhost"];
                    delay = Convert.ToInt32(Configuration.Instance["DelayFundamentals"]);
                    timeout = Convert.ToInt32(Configuration.Instance["TimeoutIbConnection"]);
                });

                IbClient.Instance.Connect(host, portIb, 1);
            });
        }

        /// <summary>
        /// ImportContracts
        /// </summary>
        /// <param name="p"></param>
        private async Task CommandImportContractsAsync(object p)
        {
            if (String.IsNullOrWhiteSpace(ExchangeIbSelected))
            {
                LogCurrent.Add($"ERROR! Exchange must be selected.");
                return;
            }

            LogCurrent = LogSymbols;
            LogCurrent.Clear();
            BackgroundLog = Brushes.Gray;
            await Task.Run(() =>
            {
                try
                {
                    string[] companiesArray = null;
                    int delay = 0;
                    Dispatcher.Invoke(() =>
                    {
                        delay = Convert.ToInt32(Configuration.Instance["DelayMathingSymbols"]);
                        companiesArray = CompaniesForSymbolResolution.ToArray();
                        CompaniesList = companiesArray.ToList();
                        SymbolProcessed = new List<string>();
                        CompaniesProcessed = new List<string>();
                    });

                    foreach (var company in companiesArray)
                    {
                        CurrentCompany = company;
                        IbClient.Instance.LookForSymbols(CurrentCompany);
                        Thread.Sleep(delay);
                    }
                }
                catch (Exception exception)
                {
                    Dispatcher.Invoke(() => { LogCurrent.Add(exception.ToString()); });
                }
            });

            if (ConnectedToIb)
            {
                LogCurrent.Add($"OK! Import completed.");
                DataContext.Instance.SaveChanges();
                LogCurrent.Add("OK! All contracts saved in database.");
                IbClient.Instance.Disonnect();

                UpdateDbState();
                InventoryText = GenerateInventoryText();
                UpdateCompaniesForSymbolResolution(ExchangesFmpSelected);
            }
            else
            {
                LogCurrent.Add($"ERROR! Error while connection to IB server.");
            }
        }

        /// <summary>
        /// ImportFundamentals
        /// </summary>
        /// <param name="p"></param>
        private async Task CommandImportFundamentalsAsync(object p)
        {
            LogCurrent = LogFundamentals;
            LogCurrent.Clear();
            BackgroundLog = Brushes.Gray;

            await Task.Run(() =>
            {
                try
                {
                    List<Contract> contractsToProcess = new List<Contract>();
                    int delay = 0;
                    Dispatcher.Invoke(() =>
                    {
                        contractsToProcess = ContractsForIbFundamentalsQueries();
                        ContractList = new List<Contract>(contractsToProcess);
                        ContractsProcessed = new List<Contract>();
                        delay = Convert.ToInt32(Configuration.Instance["DelayFundamentals"]);
                    });

                    foreach (var contract in contractsToProcess)
                    {
                        CurrentContract = contract;
                        IbClient.Instance.RequestFundamentals(contract);
                        Thread.Sleep(delay);
                    }
                }
                catch (Exception exception)
                {
                    Dispatcher.Invoke(() => { LogCurrent.Add(exception.ToString()); });
                }
            });

            if (ConnectedToIb)
            {
                LogCurrent.Add($"OK! Import completed.");
                IbClient.Instance.Disonnect();

                UpdateDbState();
                InventoryText = GenerateInventoryText();
                UpdateCompaniesForFundamenatals(SelectTop1000);
            }
            else
            {
                LogCurrent.Add($"ERROR! Error while connection to IB server.");
            }
        }

        #endregion

        #region ResponseHandler

        /// <summary>
        /// ResponseHandlerNextValidId
        /// </summary>
        /// <param name="obj"></param>
        private void ResponseHandlerNextValidId(IBSampleApp.messages.ConnectionStatusMessage obj)
        {
            BackgroundLog = Brushes.White;
            var message = string.Empty;

            if (obj.IsConnected)
            {
                ConnectedToIb = true;
                message = "OK! Connected to IB server.";
            }
            else
            {
                ConnectedToIb = false;
                message = "ERROR! error connecting to IB server.";
            }

            LogSymbols.Add(message);
            LogFundamentals.Add(message);
        }

        /// <summary>
        /// ResponseHandlerConnectionClosed
        /// </summary>
        private void ResponseHandlerConnectionClosed()
        {
            ConnectedToIb = false;
            LogCurrent.Add($"Connection to IB server closed.");
        }

        /// <summary>
        /// ResponseHandlerSymbolSamples
        /// </summary>
        /// <param name="obj"></param>
        private void ResponseHandlerSymbolSamples(IBSampleApp.messages.SymbolSamplesMessage obj)
        {
            BackgroundLog = Brushes.White;
            var message = string.Empty;

            LogCurrent.Add($"{obj.ContractDescriptions.Count()} symbols found for company {CurrentCompany}. {CompaniesList.Count()} companies more.");
            CompaniesList.Remove(CurrentCompany);
            var contracts = SymbolManager.FilterSymbols(CurrentCompany, obj, ExchangeIbSelected);
            LogCurrent.Add($"{contracts.Count()} symbols filtered out for company {CurrentCompany}");

            try
            {
                if (!contracts.Any())
                {
                    ProcessNotResolved();
                    ProcessDatabaseBatch();
                    return;
                }

                ProcessResolved(contracts);
                ProcessDatabaseBatch();
            }
            catch (Exception exception)
            {
                LogCurrent.Add(exception.ToString());
            }
        }


        /// <summary>
        /// ResponseHandlerFundamentalData
        /// </summary>
        /// <param name="obj"></param>
        private void ResponseHandlerFundamentalData(IBSampleApp.messages.FundamentalsMessage obj)
        {
            BackgroundLog = Brushes.White;
            var message = string.Empty;

            LogCurrent.Add($"Processing {CurrentContract.Company} ... {ContractList.Count()} companies more.");
            ContractList.Remove(CurrentContract);

            string fmpSymbol = QueryFactory.SymbolByCompanyNameQuery.Run(CurrentContract.Company);
            if (string.IsNullOrWhiteSpace(fmpSymbol))
            {
                LogCurrent.Add($"ERROR! FMP symbol for {CurrentContract.Company} could not be found.");
                return;
            }

            foreach (string date in Dates)
            {
                FundamentalsXmlDocument xmlDocument = XmlFactory.Instance.CreateXml(obj, date);
                SaveIncomeStatement(CurrentContract, fmpSymbol, xmlDocument, date);
                SaveBalanceSheet(CurrentContract, fmpSymbol, xmlDocument, date);
                SaveCashFlowStatement(CurrentContract, fmpSymbol, xmlDocument, date);

            }
        }

        #endregion

        /// <summary>
        /// ContractsForIbFundamentalsQueries
        /// </summary>
        /// <returns></returns>
        private List<Contract> ContractsForIbFundamentalsQueries()
        {
            var companiesToProcess = CompaniesForFundamenatalsText.Split("\r\n").ToList();
            var contractsToProcessRaw = QueryFactory.ContractsByCompanyName.Run(companiesToProcess).ToList();
            var contractsToProcess = contractsToProcessRaw.GroupBy(c => c.Company).Select(g => g.First()).OrderBy(c => c.Company).ToList();
            return contractsToProcess;
        }

        /// <summary>
        /// EnsureEmptyDatabase
        /// </summary>
        /// <returns></returns>
        private bool EnsureEmptyDatabase()
        {
            if (DataContext.Instance.Contracts.Any())
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Database table 'Contracts' has already data. Do you want to overwrite it?", "Warning! Data exists!", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    DataContext.Instance.Contracts.RemoveRange(DataContext.Instance.Contracts);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void InitDatesCombobok()
        {
            var dates = Configuration.Instance["Dates"].Split(",").ToList();
            Dates = new ObservableCollection<string>(dates.Select(e => e.Trim()).ToList());
            Date = Configuration.Instance["SelectedDate"];
        }

        /// <summary>
        /// InitExchangeCombobox
        /// </summary>
        private void InitExchangeCombobox()
        {
            var exchanges = Configuration.Instance["ExchangesNorthAmerica"].Split(",").ToList();
            exchanges.AddRange(Configuration.Instance["ExchangesAsia"].Split(",").ToList());
            exchanges.AddRange(Configuration.Instance["ExchangesEurope"].Split(",").ToList());

            ExchangesIb = exchanges.Select(e => e.Trim()).ToList();
        }

        /// <summary>
        /// InitExchangeFmpCombobox
        /// </summary>
        private void InitExchangeFmpCombobox()
        {
            ExchangesFmp = QueryFactory.ExchangesFmpQuery.Run();
            ExchangeFmpInitial = ExchangesFmp.First();
        }

        /// <summary>
        /// ProcessResolved
        /// </summary>
        /// <param name="contracts"></param>
        private void ProcessResolved(IList<Contract> contracts)
        {
            for (int i = 0; i < contracts.Count(); i++)
            {
                Contract contract = contracts[i];
                if (!SymbolProcessed.Any(s => s.ToUpper() == contract.Symbol.ToUpper()))
                {
                    if (DataContext.Instance.Contracts.Any(c => c.Symbol == contract.Symbol))
                    {
                        LogCurrent.Add($"Symbol {contract.Symbol} exists already in database table Contracts. Adding record in table NotUnique.");
                        DataContext.Instance.NotUnique.Add(new NotUnique { Symbol = contract.Symbol, Company = contract.Company });
                        continue;
                    }
                    DataContext.Instance.Contracts.Add(contract);
                    SymbolProcessed.Add(contract.Symbol);
                }
            }

            CompaniesProcessed.Add(CurrentCompany);
        }

        /// <summary>
        /// ProcessNotResolved
        /// </summary>
        private void ProcessNotResolved()
        {
            if (!CompaniesProcessed.Any(s => s.ToUpper() == CurrentCompany.ToUpper()))
            {
                if (DataContext.Instance.NotResolved.Any(n => n.Company == CurrentCompany))
                {
                    LogCurrent.Add($"Company {CurrentCompany} exists already in database table NotResolved.");
                    return;
                }

                DataContext.Instance.NotResolved.Add(new NotResolved { Company = CurrentCompany });
            }

            CompaniesProcessed.Add(CurrentCompany);
        }

        /// <summary>
        /// ProcessDatabaseBatch
        /// </summary>
        private void ProcessDatabaseBatch()
        {
            if (CompaniesProcessed.Count() > 0 && CompaniesProcessed.Count() % Convert.ToInt32(Configuration.Instance["BatchSizeDatabase"]) == 0
                || CompaniesList.Count() == 0)
            {
                DataContext.Instance.SaveChanges();
                LogCurrent.Add("OK! Current batch saved in database.");
            }
        }

        /// <summary>
        /// SaveIncomeStatement
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="fmpSymbol"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="date"></param>
        private void SaveIncomeStatement(Contract contract, string fmpSymbol, FundamentalsXmlDocument xmlDocument, string date)
        {
            if (DataContext.Instance.IncomeStatements.Any(i => i.Symbol == fmpSymbol && i.Date == date))
            {
                LogCurrent.Add($"Income statement for {contract.Company} for year {date} already exists in database.");
                return;
            }

            if(xmlDocument.Revenue == 0 && xmlDocument.OperatingIncome == 0 && xmlDocument.Eps == 0 && xmlDocument.NetIncome == 0)
            {
                LogCurrent.Add($"No IB data for income statement for {contract.Company} for year {date}.");
                return;
            }

            var incomeStatement = new IncomeStatement()
            {
                Symbol = fmpSymbol,
                Date = date,
                Revenue = xmlDocument.Revenue,
                OperatingIncome = xmlDocument.OperatingIncome,
                Epsdiluted = xmlDocument.Eps,
                NetIncome = xmlDocument.NetIncome
            };
            try
            {
                DataContext.Instance.IncomeStatements.Add(incomeStatement);
                DataContext.Instance.SaveChanges();
                LogCurrent.Add($"OK! Income statement for {contract.Company} {date} saved in database.");
            }
            catch (Exception exception)
            {
                LogCurrent.Add(exception.ToString());
            }
        }

        /// <summary>
        /// SaveBalanceSheet
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="fmpSymbol"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="date"></param>
        private void SaveBalanceSheet(Contract contract, string fmpSymbol, FundamentalsXmlDocument xmlDocument, string date)
        {
            if (DataContext.Instance.BalanceSheets.Any(i => i.Symbol == fmpSymbol && i.Date == date))
            {
                LogCurrent.Add($"Balance sheet for {contract.Company} ad date {date} already exists in database.");
                return;
            }

            if (xmlDocument.Equity == 0)
            {
                LogCurrent.Add($"No IB data for balance sheet for {contract.Company} for year {date}.");
                return;
            }

            var balanceSheet = new BalanceSheet()
            {
                Symbol = fmpSymbol,
                Date = date,
                TotalStockholdersEquity = xmlDocument.Equity
            };
            try
            {
                DataContext.Instance.BalanceSheets.Add(balanceSheet);
                DataContext.Instance.SaveChanges();
                LogCurrent.Add($"OK! Balance sheet for {contract.Company} {date} saved in database.");
            }
            catch (Exception exception)
            {
                LogCurrent.Add(exception.ToString());
            }
        }


        /// <summary>
        /// SaveCashFlowStatement
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="fmpSymbol"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="date"></param>
        private void SaveCashFlowStatement(Contract contract, string fmpSymbol, FundamentalsXmlDocument xmlDocument, string date)
        {
            if (DataContext.Instance.CashFlowStatements.Any(i => i.Symbol == fmpSymbol && i.Date == date))
            {
                LogCurrent.Add($"Cash flow statement for {contract.Company} and date {date} already exists in database.");
                return;
            }

            if (xmlDocument.NetIncomeFromCashStatement == 0  && xmlDocument.OperatingCashFlow == 0 && xmlDocument.InvestmentsInPropertyPlantAndEquipment == 0)
            {
                LogCurrent.Add($"No IB data for cash flow statement for {contract.Company} for year {date}.");
                return;
            }

            var cashFlowStatement = new CashFlowStatement()
            {
                Symbol = fmpSymbol,
                Date = date,
                NetIncome = xmlDocument.NetIncomeFromCashStatement,
                OperatingCashFlow = xmlDocument.OperatingCashFlow,
                InvestmentsInPropertyPlantAndEquipment = xmlDocument.InvestmentsInPropertyPlantAndEquipment
            };
            try
            {
                DataContext.Instance.CashFlowStatements.Add(cashFlowStatement);
                DataContext.Instance.SaveChanges();
                LogCurrent.Add($"OK! Cash flow statement for {contract.Company} {date} saved in database.");
            }
            catch (Exception exception)
            {
                LogCurrent.Add(exception.ToString());
            }
        }

        /// <summary>
        /// UpdateDbState
        /// </summary>
        private void UpdateDbState()
        {
            DbState.StocksTotal = QueryFactory.StocksTotalQuery.Run();
            DbState.CompaniesWoDocuments = QueryFactory.CompaniesWoDocumentsQuery.Run(Date);
            DbState.CompaniesWoDocumentsAndIbSymbol = QueryFactory.CompaniesWoDocumentsAndIbSymbolQuery.Run(Date);
            DbState.CompaniesWoDocumentsIbSymbolNotResolvedNotUnique = QueryFactory.CompaniesWoDocumentsIbSymbolNotResolvedNotUniqueQuery.Run(Date);
            DbState.CompaniesWoDocumentsButWithIbSymbol = QueryFactory.CompaniesWoDocumentsButWithIbSymbolQuery.Run(Date);
        }

        /// <summary>
        /// GenerateInventoryText
        /// </summary>
        /// <returns></returns>
        private string GenerateInventoryText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Summmary for {Date}:");
            sb.AppendLine();

            sb.AppendLine($"Total {DbState.StocksTotal} stocks in database.");
            sb.AppendLine($"{DbState.StocksTotal - DbState.CompaniesWoDocuments.Count()} stocks ready to analyze.");
            sb.AppendLine($"{DbState.CompaniesWoDocuments.Count()} stocks could be enriched with fundamental data.");
            sb.AppendLine();

            sb.AppendLine("Step 1");
            sb.AppendLine($"For {DbState.CompaniesWoDocumentsIbSymbolNotResolvedNotUnique.Count()} IB symbol could be found.");
            sb.AppendLine();
            sb.AppendLine("Step 2");
            sb.AppendLine($"For {DbState.CompaniesWoDocumentsButWithIbSymbol.Count()} stocks IB fundamental data could be found. (IB symbol exists already in the table Contracts)");

            return sb.ToString();
        }

        /// <summary>
        /// UpdateCompaniesForSymbolResolution
        /// </summary>
        /// <param name="exchangesFmpSelected"></param>
        private void UpdateCompaniesForSymbolResolution(List<string> exchangesFmpSelected)
        {
            CompaniesForSymbolResolutionText = String.Empty;
            CompaniesForSymbolResolution = QueryFactory.CompaniesForSymbolResolutionQuery.Run(DbState.CompaniesWoDocumentsIbSymbolNotResolvedNotUnique, exchangesFmpSelected);

            if (CompaniesForSymbolResolution.Any())
            {
                CompaniesForSymbolResolutionText = CompaniesForSymbolResolution.Aggregate((r, n) => r + "\r\n" + n);
            }
            else
            {
                CompaniesForSymbolResolutionText = string.Empty;
            }

            string text = $"Companies for Symbol resolution ({CompaniesForSymbolResolution.Count} selected)\r\nExchanges: {ExchangesFmpSelected.Aggregate((r, n) => r + ", " + n)} ";
            CompaniesForSymbolResolutionHeader = text;
        }

        /// <summary>
        /// UpdateCompaniesForFundamenatals
        /// </summary>
        /// <param name="selectTop1000"></param>
        private void UpdateCompaniesForFundamenatals(bool selectTop1000)
        {
            CompaniesForFundamenatalsText = String.Empty;

            if (DbState.CompaniesWoDocumentsButWithIbSymbol.Any())
            {
                if (selectTop1000)
                {
                    CompaniesForFundamenatalsText = DbState.CompaniesWoDocumentsButWithIbSymbol.Take(1000).Aggregate((r, n) => r + "\r\n" + n);
                }
                else
                {
                    CompaniesForFundamenatalsText = DbState.CompaniesWoDocumentsButWithIbSymbol.Aggregate((r, n) => r + "\r\n" + n);
                }
            }
            else
            {
                CompaniesForFundamenatalsText = string.Empty;
            }
        }

        /// <summary>
        /// DatePropertyChanged
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void DatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindowViewModel instance = d as MainWindowViewModel;
            if (instance == null)
            {
                return;
            }

            instance.UpdateDbState();
            instance.InventoryText = instance.GenerateInventoryText();
        }

    }
}
