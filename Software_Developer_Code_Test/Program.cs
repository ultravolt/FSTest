using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test.TransactionContext;

namespace Test
{
    //Summary: Write code to consume data in a CSV file, and generate reports.

    //You may use any language, libraries or tools you want, as long as you will be
    //able to demonstrate your solution in person. Code that you can email to the
    //interviewers ahead of time usually works best, but other means of
    //demonstration, such as a laptop with the necessary tools loaded, would be fine
    //as well.

    //(Please confirm that we've received your code submission, since our email
    // gateway will often remove attachments. We've found a public GitHub repository
    // to be the safest bet.)


    class Program
    {

        static void Main(string[] args)
        {

            
            using (var ctx = new TransactionContext("Data.csv"))
            {
                //  var transactions = ctx.Transactions;
                //Debug.WriteLine(ctx.Transactions);
                //Output Specification:

                //This is older than the oldest transaction in the file, but still in the same year
                var assumedDate = DateTime.Parse("04/28/18");

                ctx.GenerateSalesSummary(assumedDate);
                ctx.GenerateAssetsUnderManagementSummary();
                ctx.GenerateBreakReport();
                ctx.GenerateInvestorProfitReport(assumedDate);

            }
        }

        
    }
}
