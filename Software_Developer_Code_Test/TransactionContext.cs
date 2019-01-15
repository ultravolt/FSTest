using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Test
{
    public class TransactionContext : IDisposable
    {
        public string FileName { get; set; }

        public TransactionContext(string fileName)
        {
            if (File.Exists(fileName))
            {
                this.FileName = fileName;
                var q = new Queue<string>(File.ReadLines(fileName).Where(x=>!string.IsNullOrEmpty(x)));
                var columnHeaders = q.Dequeue();
                Transactions = new List<Transaction>();
                var sc = new char[] { ',' };
                while (q.Any())
                {
                    var src = q.Dequeue();
                    var itms = src.Split(sc);
                    while (itms.Count() < Transaction.columns.Count())
                    {
                        src += (" "+q.Dequeue().Trim());
                        itms = src.Split(sc);
                    }
                    var r = new Transaction(src);
                    Transactions.Add(r);
                }
                

            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public List<Transaction> Transactions { get; set; }

        public class Transaction
        {
            enum Column : int
            {
                TXN_DATE = 0,
                TXN_TYPE = 1,
                TXN_SHARES = 2,
                TXN_PRICE = 3,
                FUND = 4,
                INVESTOR = 5,
                SALES_REP = 6
            }
            private string source;
            public static Dictionary<string, int> columns = typeof(Transaction)
                .GetProperties()
                .SelectMany(x => x.GetCustomAttributes(typeof(ColumnAttribute), false)
                .Cast<ColumnAttribute>())
                .OrderBy(x => x.Order)
                .ToDictionary(x => x.Name, x => x.Order);

            public Transaction(string source)
            {
                this.source = source;
                var values = source.Split(new char[] { ',' });
                if (values.Count() == columns.Count())
                {
                    Date = DateTime.Parse(values[(int)Column.TXN_DATE]);
                    Type = values[(int)Column.TXN_TYPE];
                    Shares = (int)(float.Parse(values[(int)Column.TXN_SHARES]));
                    Price = float.Parse(values[(int)Column.TXN_PRICE].Trim(new char[] { '$', ' ' }));
                    Fund = values[(int)Column.FUND];
                    Investor = values[(int)Column.INVESTOR].Trim(new char[] { '"', ' ' });
                    SalesRepresentative = values[(int)Column.SALES_REP];
                }
                else
                {
                    Debug.WriteLine(values);
                }

            }

            [Column(nameof(Column.TXN_DATE), Order = (int)Column.TXN_DATE)]
            public DateTime Date { get; set; }

            [Column(nameof(Column.TXN_TYPE), Order = (int)Column.TXN_TYPE)]
            public string Type { get; set; }

            [Column(nameof(Column.TXN_SHARES), Order = (int)Column.TXN_SHARES)]
            public int Shares { get; set; }

            [Column(nameof(Column.TXN_PRICE), Order = (int)Column.TXN_PRICE)]
            public float Price { get; set; }

            [Column(nameof(Column.FUND), Order = (int)Column.FUND)]
            public string Fund { get; set; }

            [Column(nameof(Column.INVESTOR), Order = (int)Column.INVESTOR)]
            public string Investor { get; set; }

            [Column(nameof(Column.SALES_REP), Order = (int)Column.SALES_REP)]
            public string SalesRepresentative { get; set; }

            public override string ToString()
            {
                return string.Join(", ", new object[] { Date, Type, Shares, Price, Fund, Investor, SalesRepresentative }.Select(x => x.ToString()));
            }
        }
    }
}