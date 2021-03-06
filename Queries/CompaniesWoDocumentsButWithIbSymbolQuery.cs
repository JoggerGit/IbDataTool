﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IbDataTool.Model;

namespace IbDataTool.Queries
{
    public class CompaniesWoDocumentsButWithIbSymbolQuery : DataContext
    {
        public List<string> Run(string date)
        {
            return (from stock in Stocks
                    join income in IncomeStatements
                    on new { a = stock.Symbol, b = date } equals new { a = income.Symbol, b = income.Date } into stockIncomeRecords
                    from stockIncome in stockIncomeRecords.DefaultIfEmpty()
                    join balance in BalanceSheets
                    on new { a = stock.Symbol, b = date } equals new { a = balance.Symbol, b = balance.Date } into stockIncomeBalanceRecords
                    from stockIncomeBalance in stockIncomeBalanceRecords.DefaultIfEmpty()
                    join cash in CashFlowStatements
                    on new { a = stock.Symbol, b = date } equals new { a = cash.Symbol, b = cash.Date } into stockIncomeBalanceCashRecords
                    from stockIncomeBalanceCash in stockIncomeBalanceCashRecords.DefaultIfEmpty()
                    join contract in Contracts
                    on stock.Name equals contract.Company into stockIncomeBalanceCashContractRecords
                    from stockIncomeBalanceCashContract in stockIncomeBalanceCashContractRecords
                    where ((stockIncome == null) || (stockIncomeBalance == null) || (stockIncomeBalanceCash == null))
                    && stockIncomeBalanceCashContract != null
                    && !string.IsNullOrWhiteSpace(stock.Name)
                    select stock.Name).Distinct().ToList();
        }
    }
}