using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using iCodeGenerator.DatabaseStructure;

namespace iCodeGenerator.Generator
{
	/// <summary>
	/// Summary description for FileGenerator.
	/// </summary>
	public class FileGenerator
	{
		private IDictionary _CustomValue;

        public const string TEMPLATE_EXTENSION = ".icode";

		public IDictionary CustomValue
		{
			get { return _CustomValue; }
			set { _CustomValue = value; }
		}

		public event EventHandler OnComplete;
		protected void CompleteNotifier(EventArgs e)
		{
			if (OnComplete != null)
			{
				OnComplete(this, e);
			}
		}
		
		public void Generate(Table table,string inputDir, string outputDir)
		{
			var directoryInfo = new DirectoryInfo(inputDir);
			var client = new Client();
		    var originalSd = client.StartDelimiter;
		    var originalEd = client.EndingDelimiter;
			if(_CustomValue != null)
			{
				client.CustomValues = _CustomValue;	
			}


			foreach(var fileInfo in directoryInfo.GetFiles("*.*"+TEMPLATE_EXTENSION,SearchOption.AllDirectories))
			{
				client.StartDelimiter = originalSd;
				client.EndingDelimiter = originalEd;
				var sr = File.OpenText(fileInfo.FullName);
				var fileContent = sr.ReadToEnd();
				sr.Close();
				var codeGenerated = client.Parse(table,fileContent);
				client.StartDelimiter = String.Empty;
				client.EndingDelimiter = String.Empty;
				var filename = client.Parse(table,fileInfo.Name);

                filename = filename.Remove(filename.IndexOf(TEMPLATE_EXTENSION));
                var outputWorkDirectory = Path.Combine(outputDir, fileInfo.DirectoryName.Replace(inputDir + Path.DirectorySeparatorChar, String.Empty));

                if (Directory.Exists(outputWorkDirectory) == false)
                    Directory.CreateDirectory(outputWorkDirectory);

				try
				{
                    var sw = new StreamWriter(outputWorkDirectory + Path.DirectorySeparatorChar + filename);
					sw.Write(codeGenerated);
					sw.Flush();
					sw.Close();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}
			}

            client.StartDelimiter = originalSd;
            client.EndingDelimiter = originalEd;

			CompleteNotifier(new EventArgs());
		}
	}
}
