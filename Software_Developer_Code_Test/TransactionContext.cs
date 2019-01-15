using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;

namespace Test
{
    public class TransactionContext : DbContext
    {

        public string FileName { get; set; }

        public TransactionContext(string fileName)
        {
            //Input Specification: CSV file. First line is a header. Columns are:
            //    TXN_DATE - date transaction took place
            //    TXN_TYPE - type of transaction (BUY/SELL)
            //    TXN_SHARES - number of shares affected by transaction
            //    TXN_PRICE - price per share
            //    FUND - name of fund in which shares are transacted
            //    INVESTOR - name of the owner of the shares being transacted
            //    SALES_REP - name of the sales rep advising the investor

            TransactionQueue = new Queue<Transaction>();
            if (File.Exists(fileName))
                using (var rdr = new TextFieldParser("Data.csv"))
                {

                    rdr.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
                    rdr.Delimiters = new string[] { "," };
                    string[] values;
                    int i = 0;
                    while (!rdr.EndOfData)
                    {
                        try
                        {
                            values = rdr.ReadFields();
                            if (i > 0)
                            {
                                var t = new TransactionContext.Transaction
                                {
                                    Date = DateTime.Parse(values[(int)TransactionContext.Transaction.Column.TXN_DATE]),
                                    Type = values[(int)TransactionContext.Transaction.Column.TXN_TYPE],
                                    Shares = (int)(float.Parse(values[(int)TransactionContext.Transaction.Column.TXN_SHARES])),
                                    Price = float.Parse(values[(int)TransactionContext.Transaction.Column.TXN_PRICE].Trim(new char[] { '$', ' ' })),
                                    Fund = values[(int)TransactionContext.Transaction.Column.FUND],
                                    Investor = values[(int)TransactionContext.Transaction.Column.INVESTOR].Trim(new char[] { '"', ' ' }),
                                    SalesRepresentative = values[(int)TransactionContext.Transaction.Column.SALES_REP],


                                };
                                TransactionQueue.Enqueue(t);
                                Console.WriteLine(t);
                            }
                            i++;                            
                        }
                        catch (MalformedLineException ex)
                        {
                            Console.Error.WriteLine(ex);
                        }
                    }

                }        
        }

        public void GenerateInvestorProfitReport(DateTime? requestDate = null)
        {
            if (requestDate == null) requestDate = DateTime.Now;
            //    4. Investor Profit:
            //        For each Investor and Fund, return net profit or loss on investment.

            throw new NotImplementedException();
        }

        public void GenerateBreakReport(DateTime? requestDate = null)
        {
            if (requestDate == null) requestDate = DateTime.Now;
            //    3. Break Report:
            //        Assuming the information in the data provided is complete and accurate,
            //        generate a report that shows any errors (negative cash balances,
            //        negative share balance) by investor.
            var investors = this.TransactionQueue.Select(x => x.Investor)
                .Distinct()
                .ToDictionary(x=>x, x=>TransactionQueue.Where(y=>y.Investor==x));
            this.NegativeShareBalance = investors.Where(x => x.Value.Sum(y => y.Shares) < 0).ToDictionary(x=>x, x=>x.Value);
            this.NegativeCashBalance = investors.Where(x => x.Value.Sum(y => y.Value) < 0).ToDictionary(x => x, x => x.Value);
            
        }

        public void GenerateAssetsUnderManagementSummary()
        {
            //    2. Provide an Assets Under Management Summary:
            //        For each Sales Rep, generate a summary of the net amount held by
            //        investors across all funds.
            var assetsUnderManagementSummary = new Dictionary<string, Dictionary<string, float>>();
            var salesReps = this.TransactionQueue
                .Select(x => x.SalesRepresentative)
                .Distinct();
            foreach (var salesRep in salesReps)
            {
                var srt = this.TransactionQueue.Where(x => x.SalesRepresentative == salesRep);
                var funds = srt.Select(x => x.Fund).Distinct().ToList();
                var fsums=funds.ToDictionary(x => x, x => srt.Select(y => y.Value).Sum());
                assetsUnderManagementSummary.Add(salesRep, fsums);
            }
            this.AssetsUnderManagementSummary = assetsUnderManagementSummary;
        }

        public void GenerateSalesSummary(DateTime? requestDate=null)
        {
            //    1. Provide a Sales Summary:
            //        For each Sales Rep, generate Year to Date, Month to Date, Quarter to
            //        Date, and Inception to Date summary of cash amounts sold across all
            //        funds.
            if (requestDate == null) requestDate = DateTime.Now;
            var transactionWithinRequestDateYear=this.TransactionQueue
                .Where(x=>x.Date.Year==requestDate.Value.Year)
                .ToList();
            this.BySalesRep=transactionWithinRequestDateYear.Select(x => x.SalesRepresentative).Distinct().ToDictionary(x => x, x=>transactionWithinRequestDateYear.Where(y => y.SalesRepresentative == x));
            this.BySalesRepYearToDate = BySalesRep.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Value).Sum());
            this.BySalesRepMonthToDate = BySalesRep.ToDictionary(x => x.Key, x => x.Value.Where(y=>y.Date.Month==requestDate.Value.Month).Select(y => y.Value).Sum());
            this.BySalesRepInceptionToDate = BySalesRep.ToDictionary(x => x.Key, x => this.TransactionQueue.Where(y => y.SalesRepresentative == x.Key).Select(y=>y.Value).Sum());        
            
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        public DbSet<Transaction> Transactions { get; set; }
        public Queue<Transaction> TransactionQueue { get; set; }
        public Dictionary<string, IEnumerable<Transaction>> BySalesRep { get; private set; }
        public Dictionary<string, float> BySalesRepYearToDate { get; private set; }
        public Dictionary<string, float> BySalesRepMonthToDate { get; private set; }
        public Dictionary<string, float> BySalesRepInceptionToDate { get; private set; }
        public Dictionary<string, Dictionary<string, float>> AssetsUnderManagementSummary { get; private set; }
        public Dictionary<KeyValuePair<string, IEnumerable<Transaction>>, IEnumerable<Transaction>> NegativeShareBalance { get; private set; }
        public Dictionary<KeyValuePair<string, IEnumerable<Transaction>>, IEnumerable<Transaction>> NegativeCashBalance { get; private set; }

        public class Transaction
        {
            enum TypeEnum: int
            {
                BUY=1,
                SELL=-1
            }

            public enum Column : int
            {
                TXN_DATE = 0,
                TXN_TYPE = 1,
                TXN_SHARES = 2,
                TXN_PRICE = 3,
                FUND = 4,
                INVESTOR = 5,
                SALES_REP = 6
            }
            
            public static Dictionary<string, int> columns = typeof(Transaction)
                .GetProperties()
                .SelectMany(x => x.GetCustomAttributes(typeof(ColumnAttribute), false)
                .Cast<ColumnAttribute>())
                .OrderBy(x => x.Order)
                .ToDictionary(x => x.Name, x => x.Order);
            public Transaction()
            {

            }

            [Column(nameof(Column.TXN_DATE), Order = (int)Column.TXN_DATE)]
            public DateTime Date { get; set; }

            [Column(nameof(Column.TXN_TYPE), Order = (int)Column.TXN_TYPE)]
            public string Type { get; set; }

            [Column(nameof(Column.TXN_SHARES), Order = (int)Column.TXN_SHARES)]
            public float Shares { get; set; }

            [Column(nameof(Column.TXN_PRICE), Order = (int)Column.TXN_PRICE)]
            public float Price { get; set; }

            [Column(nameof(Column.FUND), Order = (int)Column.FUND)]
            public string Fund { get; set; }

            [Column(nameof(Column.INVESTOR), Order = (int)Column.INVESTOR)]
            public string Investor { get; set; }

            [Column(nameof(Column.SALES_REP), Order = (int)Column.SALES_REP)]
            public string SalesRepresentative { get; set; }

            [NotMapped]
            public float Value { get { return Shares * Price * (int)Enum.Parse(typeof(TypeEnum), Type); } }

            public override string ToString()
            {
                return string.Join(", ", new object[] { Date, Type, Shares, Price, Fund, Investor, SalesRepresentative }.Select(x => x.ToString()));
            }
        }
    }
}