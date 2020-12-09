using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Management.Automation;
using System.Reactive.Kql;
using System.Reactive.Kql.CustomTypes;
using Microsoft.EvtxEventXmlScrubber;
using System.Threading;
using System.Collections.Concurrent;

namespace Kql.PowerShell
{
    [Cmdlet(VerbsCommunications.Read, "Winlog")]
    public class ReadWinlog : Cmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Log;

        [Parameter(Mandatory = false)]
        public string File;

        [Parameter(Mandatory = false)]
        public string Query;

        [Parameter(Mandatory = false)]
        public bool ReadExisting;

        private ConcurrentQueue<IDictionary<string, object>> kqlOutput;

        protected override void BeginProcessing()
        {
            kqlOutput = new ConcurrentQueue<IDictionary<string, object>>();
        }
        protected override void ProcessRecord()
        {
            if(Log != null)
            {
                Console.WriteLine("Reading the OS log: {0}", Log);

                var etw = Tx.Windows.EvtxObservable.FromLog(Log, null, ReadExisting).Select(x => EvtxExtensions.Deserialize(x));
                ProcessStream(etw);
            }
            else
            {
                Console.WriteLine("Reading local file: {0}", File);
                // TODO: add in functionality to process local file
            }

            while(true)
            {
                int eventsPrinted = 0;
                while (kqlOutput.TryDequeue(out IDictionary<string, object> eventOutput))
                {
                    PSObject row = new PSObject();
                    foreach (var pair in eventOutput)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }
                    WriteObject(row);
                    eventsPrinted++;

                    if (eventsPrinted % 10 == 0) { Thread.Sleep(20); }
                }
            }
        }

        void ProcessStream(IObservable<IDictionary<string, object>> events)
        {
            Console.WriteLine("Processing Stream...");
            if (Query != null)
            {
                Console.WriteLine("Firing up Rx.Kql with Query: {0}", Query);
                var node = KqlNodeHub.FromFiles(events, OutputCallback, "WinLog", Query);
            }
            else
            {
                var enumerable = events.ToEnumerable();
                foreach (var e in enumerable)
                {
                    PSObject row = new PSObject();
                    foreach (var pair in e)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }
                    WriteObject(row);
                }
            }
        }

        private void OutputCallback(KqlOutput obj)
        {
            try
            {
                kqlOutput.Enqueue(obj.Output);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}