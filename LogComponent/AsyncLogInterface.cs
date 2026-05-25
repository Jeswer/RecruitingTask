namespace LogTest
{
	using System;
    using System.Collections.Concurrent;
	using System.IO;
	using System.Text;
	using System.Threading;

	public class AsyncLogInterface : LogInterface
	{
		private Thread _runThread;
		private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
		private DateTime _curDate = DateTime.Now;
		private StreamWriter _writer;

		private bool _exit;

		public AsyncLogInterface()
		{
			if (!Directory.Exists(@"./LogTest"))
				Directory.CreateDirectory(@"./LogTest");

			this._writer = File.AppendText(@"./LogTest/Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

			this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

			this._writer.AutoFlush = false;

			this._runThread = new Thread(this.MainLoop);
			this._runThread.Start();
		}

		private bool _QuitWithFlush = false;

		private void MainLoop()
		{
			
			while (!this._exit)
			{
				try 
				{
				// Midnight check 
				if(DateTime.Now.Date != _curDate.Date)
				{
					_curDate = DateTime.Now;
            		this._writer.Flush();
            		this._writer.Close();
            		this._writer = File.AppendText(@"./LogTest/Log" + DateTime.Now.ToString("yyyy-MM-dd HHmmssfff") + ".log");
            		this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
            		this._writer.AutoFlush = false;
				}

				if (!this._lines.IsEmpty)	
					{
						StringBuilder stringBuilder = new StringBuilder();
						while (this._lines.TryDequeue(out LogLine logLine))
						{
							stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
							stringBuilder.Append("\t");
							stringBuilder.Append(logLine.LineText());
							stringBuilder.Append("\t");
							stringBuilder.Append(Environment.NewLine);
						}
						if (stringBuilder.Length > 0)
						{
							this._writer.Write(stringBuilder.ToString()); // single write per batch
							this._writer.Flush();
						}
					}

					if (this._QuitWithFlush && this._lines.IsEmpty)
						this._exit = true;

					if (this._lines.IsEmpty)
						Thread.Sleep(50);
				}
			
			catch (System.Exception ex)
			{
				 Console.WriteLine($"Logger error: {ex}");
			}
			}
			
			this._writer.Flush();
			this._writer.Close();
			this._writer.Dispose();
		}
		

		public void Stop_Without_Flush()
		{
			this._exit = true;
			this._runThread.Join();
		}

		public void Stop_With_Flush()
		{
			this._QuitWithFlush = true;
			this._runThread.Join();
		}

		public void WriteLog(string s)
		{
			this._lines.Enqueue(new LogLine() {Text = s, Timestamp = DateTime.Now});
		}
	}
}