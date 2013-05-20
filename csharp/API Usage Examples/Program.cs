using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace API_Usage_Examples
{
    class Program
    {
        /// <summary>
        /// Welcome to MemberSuite API Samples. You can run this console app and be presented with a list of samples
        /// in this assembly. Selecting the appropriate menu item will run the selected sample.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
          

            // let's find all types in this assembly that dervice from ConciergeSampleBase
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsSubclassOf(typeof(ConciergeSampleBase))).ToList();

            // sort the types by name
            types.Sort( (x,y) => System.String.CompareOrdinal(x.Name, y.Name));

            do
            {
                Console.WriteLine();
                Console.WriteLine("Welcome to the MemberSuite API samples application. Select the sample to run.");
                Console.WriteLine();

                // now, let's present all types to the user
                for (int index = 0; index < types.Count; index++)
                {
                    var type = types[index];
                    Console.WriteLine("{0}. {1}", index, type.Name);
                }
                Console.WriteLine("X. Exit Sample Application");

                var key = Console.ReadKey();

                if (key.KeyChar == 'X' || key.KeyChar == 'x')
                    return; // application is done

                int number;

                // let's see if a valid number was press
                if (!int.TryParse(key.KeyChar.ToString(), out number) || number >= types.Count)
                {
                    Console.WriteLine("Invalid number selected. Exiting.");
                    return;
                }
                Console.WriteLine();

                var selectedType = types[number];
                Console.WriteLine("Demo '{0}' selected... executing...", selectedType.Name);

                // instantiate the demo class
                ConciergeSampleBase demoToRun = (ConciergeSampleBase)Activator.CreateInstance(selectedType);
                demoToRun.Run(); // and run it
            } while (1 == 1);




        }
    }
}
