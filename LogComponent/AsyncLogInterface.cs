namespace LogTest
{
	using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading;

	public class AsyncLogInterface : LogInterface
	{
		private Thread _runThread;
		private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();

		private StreamWriter _writer;

		private bool _exit;

		public AsyncLogInterface()
		{
			if (!Directory.Exists(@"./LogTest"))
				Directory.CreateDirectory(@"./LogTest");

			this._writer = File.AppendText(@"./LogTest/Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

			this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

			this._writer.AutoFlush = true;

			this._runThread = new Thread(this.MainLoop);
			this._runThread.Start();
		}

		private bool _QuitWithFlush = false;


		DateTime _curDate = DateTime.Now;

		private void MainLoop()
		{
			while (!this._exit)
			{
				if (this._lines.Count > 0)
				{
					int processed = 0;
					while(processed < 5 && this._lines.TryDequeue(out LogLine logLine))
					{
						processed++;
						if (!this._exit || this._QuitWithFlush)
						{
							StringBuilder stringBuilder = new StringBuilder();

							if ((DateTime.Now - _curDate).Days != 0)
							{
								_curDate = DateTime.Now;

								this._writer = File.AppendText(@"./LogTest/Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

								this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

								stringBuilder.Append(Environment.NewLine);

								this._writer.Write(stringBuilder.ToString());

								this._writer.AutoFlush = true;
							}

							stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
							stringBuilder.Append("\t");
							stringBuilder.Append(logLine.LineText());
							stringBuilder.Append("\t");

							stringBuilder.Append(Environment.NewLine);

							this._writer.Write(stringBuilder.ToString());
						}
					}

					if (this._QuitWithFlush && this._lines.IsEmpty)
						this._exit = true;

					Thread.Sleep(50);
				}
			}
		}

		public void Stop_Without_Flush()
		{
			this._exit = true;
		}

		public void Stop_With_Flush()
		{
			this._QuitWithFlush = true;
		}

		public void WriteLog(string s)
		{
			this._lines.Enqueue(new LogLine() {Text = s, Timestamp = DateTime.Now});
		}
	}
}