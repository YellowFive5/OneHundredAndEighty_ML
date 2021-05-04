#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;

#endregion

namespace BinaryLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            var workspace = Path.Combine(projectDir, "workspace");
            var assets = Path.Combine(projectDir, "assets"); 
            
            var myContext = new MLContext();

            Console.WriteLine("Hello ML!");
            Console.ReadKey();
        }
    }
}