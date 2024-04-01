using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PSKReporterHelper
{
	class LoadCallsigns
	{
		const Int32 BufferSize = 128;
		public List<string> callsigns = new List<string>();
		string fileName = "TestCallsigns.txt";

		public LoadCallsigns() 
		{
			MainWindow wnd = MainWindow.Instance;

			callsigns.Add(wnd.GetMyCallsign());


			const Int32 BufferSize = 128;
			using (var fileStream = File.OpenRead(fileName))
			using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
			{
				String line;
				while ((line = streamReader.ReadLine()) != null)
				{
					Console.WriteLine(line );

					callsigns.Add(line);
									
				}
			}
		}	
	}
}
